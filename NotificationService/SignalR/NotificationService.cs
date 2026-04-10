using Microsoft.AspNetCore.SignalR;

namespace NotificationService.SignalR
{
    public class NotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public NotificationService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendSystemNotification(string faceId, double score, string username, List<string> recommendedMenu, string message)
        {
            // ส่งหาทุกคนผ่าน HubContext
            await _hubContext.Clients.All.SendAsync("Notify", new
            {
                FaceId = faceId,
                Score = score,
                Username = username,
                RecommendedMenu = recommendedMenu,
                Message = message
            });
        }
    }
}
