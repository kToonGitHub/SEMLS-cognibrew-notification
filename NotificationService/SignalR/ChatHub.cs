using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.SignalR
{
    [Authorize]
    public class ChatHub : Hub
    {
    }
}
