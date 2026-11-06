using FoodBridge.Application.Listings;
using FoodBridge.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Api.Controllers;

[ApiController]
[Route("api/listings")]
public sealed class ListingsController : ControllerBase
{
    private readonly IListingService _listings;
    private readonly ICurrentUser _currentUser;

    public ListingsController(IListingService listings, ICurrentUser currentUser)
    {
        _listings = listings;
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
}
