using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FoodBridge.Api.Notifications;

[Authorize]
public sealed class NotificationsHub : Hub
{
}
