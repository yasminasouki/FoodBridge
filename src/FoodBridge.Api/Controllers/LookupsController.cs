using FoodBridge.Application.Listings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBridge.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/lookups")]
public sealed class LookupsController : ControllerBase
{
    private readonly ILookupService _lookups;

    public LookupsController(ILookupService lookups)
    {
        _lookups = lookups;
    }

    [HttpGet]
    [ProducesResponseType(typeof(LookupDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LookupDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _lookups.GetAsync(cancellationToken));
    }
}
