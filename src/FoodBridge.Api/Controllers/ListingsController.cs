using FoodBridge.Application.Allocation;
using FoodBridge.Application.Listings;
using FoodBridge.Application.Reservations;
using FoodBridge.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Api.Controllers;

[ApiController]
[Route("api/listings")]
public sealed class ListingsController : ControllerBase
{
    private readonly IListingService _listings;
    private readonly IAllocationService _allocation;
    private readonly IReservationService _reservations;
    private readonly ICurrentUser _currentUser;

    public ListingsController(
        IListingService listings,
        IAllocationService allocation,
        IReservationService reservations,
        ICurrentUser currentUser)
    {
        _listings = listings;
        _allocation = allocation;
        _reservations = reservations;
        _currentUser = currentUser;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ListingSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ListingSummaryDto>>> Search(
        [FromQuery] int? areaId,
        [FromQuery] int[]? categoryIds,
        [FromQuery] bool mine = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _listings.SearchAsync(
            areaId,
            categoryIds,
            mine,
            _currentUser.UserId,
            page,
            pageSize,
            cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ListingDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingDetailsDto>> Get(int id, CancellationToken cancellationToken)
    {
        return Ok(await _listings.GetAsync(id, cancellationToken));
    }

    [Authorize(Policy = "CanPostFood")]
    [HttpPost]
    [ProducesResponseType(typeof(ListingDetailsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ListingDetailsDto>> Create(
        CreateListingRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _listings.CreateAsync(_currentUser.UserId!, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [Authorize(Policy = "CanPostFood")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _listings.CancelAsync(_currentUser.UserId!, id, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = "CanClaim")]
    [HttpPost("{id:int}/requests")]
    [ProducesResponseType(typeof(ClaimRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClaimRequestDto>> CreateRequest(
        int id,
        CreateClaimRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _allocation.RequestAsync(
            _currentUser.UserId!,
            id,
            request.Quantity,
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [Authorize(Policy = "CanPostFood")]
    [HttpPost("{id:int}/allocate")]
    [ProducesResponseType(typeof(AllocationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AllocationResultDto>> Allocate(int id, CancellationToken cancellationToken)
    {
        return Ok(await _allocation.AllocateAsync(_currentUser.UserId!, id, cancellationToken));
    }

    [Authorize(Policy = "CanClaim")]
    [HttpPost("{id:int}/reservations")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> ReserveLeftover(
        int id,
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _reservations.ReserveLeftoverAsync(
            _currentUser.UserId!,
            id,
            request.Quantity,
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [Authorize(Policy = "CanPostFood")]
    [HttpPost("{id:int}/close")]
    [ProducesResponseType(typeof(CloseListingResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CloseListingResultDto>> Close(int id, CancellationToken cancellationToken)
    {
        return Ok(await _listings.CloseAsync(_currentUser.UserId!, id, cancellationToken));
    }
}
