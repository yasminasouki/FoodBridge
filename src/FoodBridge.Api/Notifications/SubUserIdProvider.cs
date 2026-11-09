using Microsoft.AspNetCore.SignalR;

namespace FoodBridge.Api.Notifications;

public sealed class SubUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst("sub")?.Value;
    }
}
