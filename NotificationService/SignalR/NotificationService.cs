using Microsoft.AspNetCore.SignalR;
using NotificationService.Models;

namespace NotificationService.SignalR
{
    public class NotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public NotificationService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendSystemNotification(NotificationMessage notificationMessage)
        {
            // ส่งหาทุกคนผ่าน HubContext
            await _hubContext.Clients.All.SendAsync("Notify", notificationMessage);
        }
    }
}
