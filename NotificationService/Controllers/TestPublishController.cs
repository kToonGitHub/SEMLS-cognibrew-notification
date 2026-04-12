using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using Google.Protobuf;
using Cognibrew.Events;
using Microsoft.AspNetCore.Connections.Features;

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

        // เพิ่มพารามิเตอร์ [FromQuery] string username เพื่อรับค่าจาก URL
        [HttpPost("publish-mock")]
        public async Task<IActionResult> PublishMockData([FromQuery] string username, [FromQuery] bool mock_member_info = true, [FromQuery] bool mock_recommendation = true, [FromQuery] bool mock_recognition = true)
        {
            // ตรวจสอบว่ามีการส่ง username มาหรือไม่
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { Message = "Username is required." });
            }

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
            if (mock_recognition)
            {
                var mockFaceResult = new FaceResult
                {
                    FaceId = faceIdMock,
                    Username = username, // แทนที่ Hardcode ด้วยตัวแปร username
                    Score = 0.98f
                };
                mockFaceResult.Embedding.AddRange(new float[] { 0, 0.2f, 0.1f, 0.7f, 0.02f });

                var faceBody = mockFaceResult.ToByteArray();
                string faceExchange = _config["RabbitMQ:FaceResultExchange"] ?? "cognibrew.inference";
                string faceRoutingKey = _config["RabbitMQ:FaceResultRoutingKey"] ?? "face.recognized";

                await channel.BasicPublishAsync(
                    exchange: faceExchange,
                    routingKey: faceRoutingKey,
                    body: faceBody
                );
            }

            // ==========================================
            // 2. Publish Mock Data สำหรับ Recommended Menu
            // ==========================================
            if (mock_recommendation)
            {
                // แยกหมวดหมู่เครื่องดื่ม (รวม Hot, Iced, Frappe และ Seasonal ที่เป็นน้ำ)
                string[] drinks = {
                    "Latte", "Cappuccino", "Flat White", "Cortado", "Americano",
                    "Iced Latte", "Cold Brew", "Iced Matcha",
                    "Mocha Frappe", "Caramel Frappe", "Peppermint Mocha"
                };
                // แยกหมวดหมู่ขนม (Pastries และ Seasonal ที่เป็นขนม)
                string[] snacks = {
                    "Croissant", "Blueberry Muffin", "Pumpkin Bread"
                };
                var random = new Random();
                string randomDrink = drinks[random.Next(drinks.Length)];
                string randomSnack = snacks[random.Next(snacks.Length)];

                var mockRecommendation = new Recommendation
                {
                    FaceId = faceIdMock,
                    Username = username // แทนที่ Hardcode ด้วยตัวแปร username
                };
                mockRecommendation.RecommendedMenu.Add(randomDrink);
                mockRecommendation.RecommendedMenu.Add(randomSnack);

                var menuBody = mockRecommendation.ToByteArray();
                string menuExchange = _config["RabbitMQ:RecommendationExchange"] ?? "cognibrew.recommendation";
                string menuRoutingKey = _config["RabbitMQ:RecommendationRoutingKey"] ?? "menu.recommended";

                await channel.BasicPublishAsync(
                    exchange: menuExchange,
                    routingKey: menuRoutingKey,
                    body: menuBody
                );
            }

            // ==========================================
            // 3. Publish Mock Data สำหรับ Member Info
            // ==========================================
            if (mock_member_info)
            {
                var mockMemberInfo = new MemberInfo
                {
                    FaceId = faceIdMock,
                    FirstName = username, // นำ username มาใช้แทน FirstName ชั่วคราวเพื่อให้เห็นความเปลี่ยนแปลง
                    LastName = "Mock",
                    Rank = "Gold",
                    Points = 1500,
                    ImageBase64 = "<Base64ImageString>"
                };

                var memberBody = mockMemberInfo.ToByteArray();
                string memberExchange = _config["RabbitMQ:MemberExchange"] ?? "cognibrew.member";
                string memberRoutingKey = _config["RabbitMQ:MemberRoutingKey"] ?? "member.memberInfo";

                await channel.BasicPublishAsync(
                    exchange: memberExchange,
                    routingKey: memberRoutingKey,
                    body: memberBody
                );
            }

            return Ok(new
            {
                Message = $"Published mock messages successfully for Face, Recommendation, and Member!",
                FaceId = faceIdMock,
                Username = username // คืนค่า username กลับไปเพื่อยืนยัน
            });
        }
    }
}