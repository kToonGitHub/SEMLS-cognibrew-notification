using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using Google.Protobuf;
using Cognibrew.Events; // เปลี่ยน Namespace ให้ตรงกับ Protobuf ของคุณ

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestPublishController : ControllerBase
    {
        private readonly IConfiguration _config;

        // Inject IConfiguration ผ่าน Constructor
        public TestPublishController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("publish-mock")]
        public async Task<IActionResult> PublishMockData()
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:HostName"] ?? "localhost",
                UserName = _config["RabbitMQ:UserName"] ?? "guest",
                Password = _config["RabbitMQ:Password"] ?? "guest"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            string faceIdMock = $"test-face-{Guid.NewGuid().ToString()[..8]}";

            // ==========================================
            // 1. Publish Mock Data สำหรับ Face Result
            // ==========================================
            var mockFaceResult = new FaceResult
            {
                FaceId = faceIdMock,
                Username = "Tester One",
                Score = 0.98f
            };

            var faceBody = mockFaceResult.ToByteArray();
            string faceExchange = _config["RabbitMQ:FaceResultExchange"] ?? "cognibrew.inference";
            string faceRoutingKey = _config["RabbitMQ:FaceResultRoutingKey"] ?? "face.recognized";

            await channel.BasicPublishAsync(
                exchange: faceExchange,
                routingKey: faceRoutingKey,
                body: faceBody
            );

            // ==========================================
            // 2. Publish Mock Data สำหรับ Recommended Menu
            // ==========================================
            var mockRecommendation = new Recommendation
            {
                FaceId = faceIdMock,
            };
            mockRecommendation.RecommendedMenu.Add("Iced Americano");
            mockRecommendation.RecommendedMenu.Add("Matcha Latte");

            var menuBody = mockRecommendation.ToByteArray();
            string menuExchange = _config["RabbitMQ:RecommendationExchange"] ?? "cognibrew.recommendation";
            string menuRoutingKey = _config["RabbitMQ:RecommendationRoutingKey"] ?? "menu.recommended";

            await channel.BasicPublishAsync(
                exchange: menuExchange,
                routingKey: menuRoutingKey,
                body: menuBody
            );

            return Ok(new
            {
                Message = "Published mock messages successfully!",
                FaceId = faceIdMock
            });
        }
    }
}