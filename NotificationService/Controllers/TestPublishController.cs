using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using Google.Protobuf;
using Cognibrew.Events;

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
            mockFaceResult.Embedding.AddRange(new float[] { 0, 0.2f, 0.1f, 0.7f, 0.02f }); // เพิ่มข้อมูล Embedding

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
                Username = "Tester One"
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

            // ==========================================
            // 3. Publish Mock Data สำหรับ Member Info
            // ==========================================
            var mockMemberInfo = new MemberInfo
            {
                FaceId = faceIdMock,
                FirstName = "Tester",
                LastName = "One",
                Rank = "Gold", // "Silver" "Gold" "Platinum"
                Points = 1500, // คะแนนสะสมของสมาชิก
                ImageBase64 = "<Base64ImageString>" // รูปภาพของสมาชิกในรูปแบบ Base64
            };

            var memberBody = mockMemberInfo.ToByteArray();
            string memberExchange = _config["RabbitMQ:MemberExchange"] ?? "cognibrew.member";
            string memberRoutingKey = _config["RabbitMQ:MemberRoutingKey"] ?? "member.memberInfo";

            await channel.BasicPublishAsync(
                exchange: memberExchange,
                routingKey: memberRoutingKey,
                body: memberBody
            );

            return Ok(new
            {
                Message = "Published mock messages successfully for Face, Recommendation, and Member!",
                FaceId = faceIdMock
            });
        }
    }
}