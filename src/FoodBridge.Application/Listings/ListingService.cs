using FoodBridge.Application.Allocation;
using FoodBridge.Application.Notifications;
using FoodBridge.Application.Persistence;
using FoodBridge.Application.Security;
using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Application.Listings;

public sealed class ListingService : IListingService
{
    private readonly IListingRepository _listings;
    private readonly IReservationRepository _reservations;
    private readonly IReceiverProfileRepository _profiles;
    private readonly IShopRepository _shops;
    private readonly IUserNotifier _notifier;
    private readonly IAllocationGrantQuery _grants;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public ListingService(
        IListingRepository listings,
        IReservationRepository reservations,
        IReceiverProfileRepository profiles,
        IShopRepository shops,
        IUserNotifier notifier,
        IAllocationGrantQuery grants,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _listings = listings;
        _reservations = reservations;
        _profiles = profiles;
        _shops = shops;
        _notifier = notifier;
        _grants = grants;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<PagedResult<ListingSummaryDto>> SearchAsync(
        int? areaId,
        int[]? categoryIds,
        bool mine,
        string? userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new DomainException("Page starts at 1.");
        }

        pageSize = Math.Clamp(pageSize, 1, 50);
        int? shopId = null;
        if (mine)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedException("Log in to see your listings.");
            }

            var shop = await _shops.GetByOwnerAsync(userId, cancellationToken)
                ?? throw new ForbiddenException("Only a shop can see its own listings.");
            shopId = shop.Id;
        }

        return await _listings.SearchAsync(
            new ListingSearch(areaId, categoryIds, mine, shopId, _clock.UtcNow, page, pageSize),
            cancellationToken);
    }

    public async Task<ListingDetailsDto> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _listings.GetDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Listing was not found.");
    }

    public async Task<ListingDetailsDto> CreateAsync(
        string userId,
        CreateListingRequest request,
        CancellationToken cancellationToken = default)
    {
        var shop = await _shops.GetByOwnerAsync(userId, cancellationToken)
            ?? throw new ForbiddenException("Only a shop can post food.");
        if (!shop.IsApproved)
        {
            throw new ForbiddenException("The shop is not approved yet.");
        }

        if (request.RequestWindowEndsAt <= _clock.UtcNow)
        {
            throw new DomainException("The request window must end in the future.");
        }

        var categoryIds = request.CategoryIds.Distinct().ToArray();
        if (!await _listings.AllCategoriesExistAsync(categoryIds, cancellationToken))
        {
            throw new DomainException("One or more categories do not exist.");
        }

        var listing = new Listing(
            shop.Id,
            request.Title,
            request.Quantity,
            request.WeightPerUnitKg,
            request.RequestWindowEndsAt,
            request.PickupFrom,
            request.PickupUntil);

        await _listings.AddWithCategoriesAsync(listing, categoryIds, cancellationToken);
        return await GetAsync(listing.Id, cancellationToken);
    }

    public async Task CancelAsync(string userId, int id, CancellationToken cancellationToken = default)
    {
        var shop = await _shops.GetByOwnerAsync(userId, cancellationToken)
            ?? throw new ForbiddenException("Only a shop can cancel a listing.");
        var listing = await _listings.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Listing was not found.");
        if (listing.ShopId != shop.Id)
        {
            throw new ForbiddenException("You can only cancel your own listing.");
        }

        listing.Cancel();
        var claims = await _unitOfWork.ClaimRequests.ListPendingForListingAsync(id, cancellationToken);
        foreach (var claim in claims)
        {
            await _notifier.NotifyAsync(
                claim.ReceiverId,
                $"{listing.Title} was cancelled. Your request will not be allocated.",
                listing.Id,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CloseListingResultDto> CloseAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        var shop = await _shops.GetByOwnerAsync(userId, cancellationToken)
            ?? throw new ForbiddenException("Only a shop can close a listing.");
        var listing = await _listings.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Listing was not found.");
        if (listing.ShopId != shop.Id)
        {
            throw new ForbiddenException("You can only close your own listing.");
        }

        listing.Close(_clock.UtcNow);
        var pending = await _reservations.ListPendingForListingAsync(id, cancellationToken);
        var profiles = await _profiles.GetTrackedByUserIdsAsync(
            pending.Select(reservation => reservation.ReceiverId).Distinct().ToArray(),
            cancellationToken);
        var profilesByUser = profiles.ToDictionary(profile => profile.UserId);
        foreach (var reservation in pending)
        {
            reservation.MarkNoShow();
            if (!profilesByUser.TryGetValue(reservation.ReceiverId, out var profile))
            {
                throw new NotFoundException("A receiver profile was not found.");
            }

            if (reservation.Source == ReservationSource.Waitlist)
            {
                await _notifier.NotifyAsync(
                    reservation.ReceiverId,
                    $"{listing.Title}: the pickup window ended before the waitlist offer was collected. Your reliability score was not changed.",
                    listing.Id,
                    cancellationToken);
                continue;
            }

            profile.ApplyNoShow(_clock.UtcNow);
            await _notifier.NotifyAsync(
                reservation.ReceiverId,
                $"{listing.Title}: the pickup window ended before collection. This counts as a no-show.",
                listing.Id,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CloseListingResultDto(listing.Id, listing.Status.ToString(), pending.Count);
    }

    public async Task<IReadOnlyList<AllocationGrantDto>> GetGrantsAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        var shop = await _shops.GetByOwnerAsync(userId, cancellationToken)
            ?? throw new ForbiddenException("Only a shop can see an allocation result.");
        var listing = await _listings.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Listing was not found.");
        if (listing.ShopId != shop.Id)
        {
            throw new ForbiddenException("You can only see your own listings.");
        }

        return await _grants.ListAsync(id, cancellationToken);
    }
}
