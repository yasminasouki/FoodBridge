using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace FoodBridge.Api.Authorization;

public sealed class ForbiddenResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (!authorizeResult.Forbidden || context.User.Identity?.IsAuthenticated != true)
        {
            await _default.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        var waitingForApproval = authorizeResult.AuthorizationFailure?.FailedRequirements
            .Any(requirement => requirement is CanClaimRequirement or ApprovedShopRequirement) == true;

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://httpstatuses.com/403",
            title = "Forbidden",
            status = StatusCodes.Status403Forbidden,
            detail = waitingForApproval
                ? "An admin still has to approve this account, or it is blocked."
                : "You cannot do that."
        });
    }
}
