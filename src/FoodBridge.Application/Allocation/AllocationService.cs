using FoodBridge.Application.Persistence;
using FoodBridge.Application.Security;
using FoodBridge.Domain.Allocation;
using FoodBridge.Domain.Entities;
using FoodBridge.Domain.Enums;
using FoodBridge.Domain.Exceptions;

namespace FoodBridge.Application.Allocation;

public sealed class AllocationService : IAllocationService
{
    private readonly IListingRepository _listings;
    private readonly IClaimRequestRepository _claims;
    private readonly IReservationRepository _reservations;
    private readonly IReceiverProfileRepository _profiles;
    private readonly IShopRepository _shops;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IAllocationStrategy _allocator;
    private readonly IEnumerable<IPriorityRule> _rules;
    private readonly ReservationPolicySelector _policies;

    public AllocationService(
        IListingRepository listings,
        IClaimRequestRepository claims,
        IReservationRepository reservations,
        IReceiverProfileRepository profiles,
        IShopRepository shops,
        IUnitOfWork unitOfWork,
        IClock clock,
        IAllocationStrategy allocator,
        IEnumerable<IPriorityRule> rules,
        ReservationPolicySelector policies)
    {
        _listings = listings;
        _claims = claims;
        _reservations = reservations;
        _profiles = profiles;
        _shops = shops;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _allocator = allocator;
        _rules = rules;
        _policies = policies;
    }

    public async Task<ClaimRequestDto> RequestAsync(
        string userId,
        int listingId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profiles.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ForbiddenException("Only a receiver can request food.");
        if (!profile.IsApproved)
        {
            throw new ForbiddenException("This account is not approved yet.");
        }

        if (profile.BlockedUntil is DateTime blockedUntil && blockedUntil > _clock.UtcNow)
        {
            throw new UserBlockedException("This account is blocked.");
        }

        var listing = await _listings.GetByIdAsync(listingId, cancellationToken)
            ?? throw new NotFoundException("Listing was not found.");
        if (listing.Status != ListingStatus.Open)
        {
            throw new ConflictException("This listing is not open for requests.");
        }

        if (listing.RequestWindowEndsAt <= _clock.UtcNow)
        {
            throw new ConflictException("The request window has ended.");
        }

        var maxUnits = _policies.For(profile.ReceiverType).MaxUnitsPerRequest;
        if (quantity > maxUnits)
        {
            throw new DomainException($"You can request at most {maxUnits} units.");
        }

        if (await _claims.ExistsAsync(listingId, userId, cancellationToken))
        {
            throw new ConflictException("You already requested this listing.");
        }

        var claim = new ClaimRequest(listingId, userId, quantity);
        await _claims.AddAsync(claim, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new ClaimRequestDto(claim.Id, claim.ListingId, claim.RequestedQuantity, claim.Status.ToString());
    }

    public async Task<AllocationResultDto> AllocateAsync(
        string userId,
        int listingId,
        CancellationToken cancellationToken = default)
    {
        var shop = await _shops.GetByOwnerAsync(userId, cancellationToken)
            ?? throw new ForbiddenException("Only a shop can allocate food.");
        var listing = await _listings.GetByIdAsync(listingId, cancellationToken)
            ?? throw new NotFoundException("Listing was not found.");
        if (listing.ShopId != shop.Id)
        {
            throw new ForbiddenException("You can only allocate your own listing.");
        }

        var claims = await _claims.ListPendingForListingAsync(listingId, cancellationToken);
        listing.MarkAllocated();
        if (claims.Count == 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new AllocationResultDto(listing.Id, listing.Status.ToString(), listing.RemainingQuantity, []);
        }

        var receiverIds = claims.Select(claim => claim.ReceiverId).Distinct().ToArray();
        var profiles = await _profiles.GetByUserIdsAsync(receiverIds, cancellationToken);
        var profilesByUser = profiles.ToDictionary(profile => profile.UserId);
        var collected = await _reservations.SumCollectedSinceAsync(
            receiverIds,
            _clock.UtcNow.AddDays(-7),
            cancellationToken);

        var scored = new List<(ClaimRequest Claim, int Score, int MaxUnits)>(claims.Count);
        foreach (var claim in claims)
        {
            if (!profilesByUser.TryGetValue(claim.ReceiverId, out var profile))
            {
                throw new NotFoundException("A requester profile was not found.");
            }

            var facts = new PriorityFacts(
                profile.ReceiverType,
                profile.PeopleServed,
                profile.ReliabilityScore,
                collected.GetValueOrDefault(claim.ReceiverId));
            var score = _rules.Sum(rule => rule.Score(facts));
            var maxUnits = _policies.For(profile.ReceiverType).MaxUnitsPerRequest;
            scored.Add((claim, score, maxUnits));
        }

        var grants = _allocator.Allocate(
            listing.RemainingQuantity,
            scored.Select(item => new AllocationRequest(
                item.Claim.Id,
                item.Claim.RequestedQuantity,
                item.Score,
                item.MaxUnits)).ToList());
        var grantByRequest = grants.ToDictionary(line => line.RequestId);

        var lines = new List<AllocationLineDto>(scored.Count);
        foreach (var item in scored)
        {
            var granted = grantByRequest[item.Claim.Id].GrantedQuantity;
            item.Claim.ApplyAllocation(granted, item.Score);
            string? pickupCode = null;
            if (granted > 0)
            {
                listing.Reserve(granted);
                pickupCode = NewPickupCode();
                await _reservations.AddAsync(
                    new Reservation(
                        listing.Id,
                        item.Claim.ReceiverId,
                        item.Claim.Id,
                        granted,
                        pickupCode,
                        ReservationSource.Allocation),
                    cancellationToken);
            }

            lines.Add(new AllocationLineDto(
                item.Claim.Id,
                item.Claim.ReceiverId,
                item.Claim.RequestedQuantity,
                granted,
                item.Score,
                item.Claim.Status.ToString(),
                pickupCode));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new AllocationResultDto(
            listing.Id,
            listing.Status.ToString(),
            listing.RemainingQuantity,
            lines.OrderByDescending(line => line.PriorityScore).ThenBy(line => line.RequestId).ToList());
    }

    private static string NewPickupCode() => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}
