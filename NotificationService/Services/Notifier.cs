
using Cognibrew.Events;
using MongoDB.Bson;
using MongoDB.Driver;
using NotificationService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Services
{
    public class Notifier : BackgroundService
    {
        private readonly IMongoCollection<NotificationDocument> _collection;
        private readonly IConfiguration _config;
        private readonly ILogger<Notifier> _logger;
        private IConnection? _connection; 
        private IChannel? _channelFaceResult;
        private IChannel? _channelRecommendedMenu;
        private readonly NotificationService.SignalR.NotificationService _notificationService;

        public Notifier(
            IMongoCollection<NotificationDocument> collection, 
            IConfiguration config, 
            ILogger<Notifier> logger,
            NotificationService.SignalR.NotificationService notificationService)
        {
            _collection = collection;
            _config = config;
            _logger = logger;
            _notificationService = notificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:HostName"] ?? "localhost",
                UserName = _config["RabbitMQ:UserName"] ?? "guest",
                Password = _config["RabbitMQ:Password"] ?? "guest"
            };

            try
            {
                // เชื่อมต่อ RabbitMQ
                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channelFaceResult = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
                _channelRecommendedMenu = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                string faceQueueName = await ConnectFaceResult(_channelFaceResult, stoppingToken);
                string menuQueueName = await ConnectRecommendedMenu(_channelRecommendedMenu, stoppingToken);

                // ==========================================
                // Consumer 1: สำหรับ Face Result
                // ==========================================
                var faceConsumer = new AsyncEventingBasicConsumer(_channelFaceResult);
                faceConsumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        FaceResult faceResult = await ProcessFaceResultMessageAsync(body); // ทำ Logic ของ Face Result
                        await _channelFaceResult.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        await ReadAndNotify(faceResult.FaceId); // อ่านข้อมูลจาก MongoDB และส่ง Notification ผ่าน SignalR
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing Face Result message.");
                        // await _channelFaceResult.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                };
                await _channelFaceResult.BasicConsumeAsync(queue: faceQueueName, autoAck: false, consumer: faceConsumer, cancellationToken: stoppingToken);
                _logger.LogInformation($"[*] Subscribed to Face Result queue: {faceQueueName}");

                // ==========================================
                // Consumer 2: สำหรับ Recommended Menu
                // ==========================================
                var menuConsumer = new AsyncEventingBasicConsumer(_channelRecommendedMenu);
                menuConsumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        Recommendation recommendation = await ProcessRecommendedMenuMessageAsync(body); // ทำ Logic ของ Menu
                        await _channelRecommendedMenu.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        await ReadAndNotify(recommendation.FaceId); // อ่านข้อมูลจาก MongoDB และส่ง Notification ผ่าน SignalR
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing Recommended Menu message.");
                        // await _channelRecommendedMenu.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                };
                await _channelRecommendedMenu.BasicConsumeAsync(queue: menuQueueName, autoAck: false, consumer: menuConsumer, cancellationToken: stoppingToken);
                _logger.LogInformation($"[*] Subscribed to Recommended Menu queue: {menuQueueName}");

                // รอจนกว่า Service จะถูกสั่งหยุด
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ");
            }
        }

        private async Task<string> ConnectFaceResult(IChannel channel, CancellationToken stoppingToken)
        {
            if (channel is null)
            {
                _logger.LogError("RabbitMQ channel is not initialized.");
                return string.Empty;
            }

            string queueName = "cognibrew.inference.face_embeded.feedback_service";
            if (!string.IsNullOrEmpty(_config["RabbitMQ:FaceResultQueue"]))
            {
                queueName = _config["RabbitMQ:FaceResultQueue"]!;
                _logger.LogInformation($"[*] Using queue name from config: {queueName}");
            }
            else
            {
                _logger.LogInformation($"[*] Using default queue name: {queueName}");
            }

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken
            );
            _logger.LogInformation($"[*] Connected to RabbitMQ and declared queue: {queueName}");

            string exchangeName = "cognibrew.inference";
            if (!string.IsNullOrEmpty(_config["RabbitMQ:FaceResultExchange"]))
            {
                exchangeName = _config["RabbitMQ:FaceResultExchange"]!;
                _logger.LogInformation($"[*] Using exchange from config: {exchangeName}");
            }
            else
            {
                _logger.LogInformation($"[*] Using default exchange: {exchangeName}");
            }
            // ==========================================
            // เพิ่มการประกาศ (Declare) Exchange
            // ==========================================
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );
            _logger.LogInformation($"[*] Declared exchange: {exchangeName}");

            string routingKey = "face.recognized";
            if (!string.IsNullOrEmpty(_config["RabbitMQ:FaceResultRoutingKey"]))
            {
                routingKey = _config["RabbitMQ:FaceResultRoutingKey"]!;
                _logger.LogInformation($"[*] Using routing key from config: {routingKey}");
            }
            else
            {
                _logger.LogInformation($"[*] Using default routing key: {routingKey}");
            }

            await channel.QueueBindAsync(
                queue: queueName,
                exchange: exchangeName,
                routingKey: routingKey,
                cancellationToken: stoppingToken
            );
            _logger.LogInformation($"[*] Bound queue '{queueName}' to exchange '{exchangeName}' with routing key '{routingKey}'");
            return queueName;
        }

        private async Task<string> ConnectRecommendedMenu(IChannel channel, CancellationToken stoppingToken)
        {
            if (channel is null)
            {
                _logger.LogError("RabbitMQ channel is not initialized.");
                return string.Empty;
            }

            string queueName = "cognibrew.recommendation.notification_service";
            if (!string.IsNullOrEmpty(_config["RabbitMQ:RecommendationQueue"]))
            {
                queueName = _config["RabbitMQ:RecommendationQueue"]!;
                _logger.LogInformation($"[*] Using queue name from config: {queueName}");
            }
            else
            {
                _logger.LogInformation($"[*] Using default queue name: {queueName}");
            }

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken
            );
            _logger.LogInformation($"[*] Connected to RabbitMQ and declared queue: {queueName}");

            string exchangeName = "cognibrew.recommendation";
            if (!string.IsNullOrEmpty(_config["RabbitMQ:RecommendationExchange"]))
            {
                exchangeName = _config["RabbitMQ:RecommendationExchange"]!;
                _logger.LogInformation($"[*] Using exchange from config: {exchangeName}");
            }
            else
            {
                _logger.LogInformation($"[*] Using default exchange: {exchangeName}");
            }
            // ==========================================
            // เพิ่มการประกาศ (Declare) Exchange
            // ==========================================
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );
            _logger.LogInformation($"[*] Declared exchange: {exchangeName}");

            string routingKey = "menu.recommended";
            if (!string.IsNullOrEmpty(_config["RabbitMQ:RecommendationRoutingKey"]))
            {
                routingKey = _config["RabbitMQ:RecommendationRoutingKey"]!;
                _logger.LogInformation($"[*] Using routing key from config: {routingKey}");
            }
            else
            {
                _logger.LogInformation($"[*] Using default routing key: {routingKey}");
            }

            await channel.QueueBindAsync(
                queue: queueName,
                exchange: exchangeName,
                routingKey: routingKey,
                cancellationToken: stoppingToken
            );
            _logger.LogInformation($"[*] Bound queue '{queueName}' to exchange '{exchangeName}' with routing key '{routingKey}'");
            return queueName;
        }

        public async Task<Recommendation> ProcessRecommendedMenuMessageAsync(byte[] body)
        {
            Recommendation recommendationResult = Recommendation.Parser.ParseFrom(body);
            await SaveToMongoAsync(recommendationResult);
            return recommendationResult;
        }

        public async Task<FaceResult> ProcessFaceResultMessageAsync(byte[] body)
        {
            FaceResult faceResult = FaceResult.Parser.ParseFrom(body);
            await SaveToMongoAsync(faceResult);
            return faceResult;
        }

        private async Task SaveToMongoAsync(Recommendation recommendationResult)
        {
            if (string.IsNullOrEmpty(recommendationResult.FaceId))
            {
                _logger.LogWarning("Received recommendation result with null FaceId. Skipping MongoDB save.");
                return;
            }
            string faceId = recommendationResult.FaceId;
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd"); // ใช้วันที่ปัจจุบัน

            // ค้นหา Document ของเครื่องนี้ ในวันที่นี้
            var filter = Builders<NotificationDocument>.Filter.Eq(x => x.FaceId, faceId);

            // คำสั่งอัปเดต: ถ้าเจอให้ Push ลง Array, ถ้าไม่เจอให้สร้าง Document ใหม่ (Upsert)
            var update = Builders<NotificationDocument>.Update
                .SetOnInsert(x => x.Id, ObjectId.GenerateNewId().ToString())
                .SetOnInsert(x => x.Date, currentDate) // กำหนดวันที่ตอนสร้างใหม่
                .PushEach(x => x.RecommendedMenu, recommendationResult.RecommendedMenu);

            var options = new UpdateOptions { IsUpsert = true };

            await _collection.UpdateOneAsync(filter, update, options);
        }

        private async Task SaveToMongoAsync(FaceResult faceResult)
        {
            if (string.IsNullOrEmpty(faceResult.FaceId))
            {
                _logger.LogWarning("Received face result with null FaceId. Skipping MongoDB save.");
                return;
            }
            string faceId = faceResult.FaceId;
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd"); // ใช้วันที่ปัจจุบัน

            // ค้นหา Document ของเครื่องนี้ ในวันที่นี้
            var filter = Builders<NotificationDocument>.Filter.Eq(x => x.FaceId, faceId);

            // คำสั่งอัปเดต: ถ้าไม่เจอให้สร้าง Document ใหม่ (Upsert)
            var update = Builders<NotificationDocument>.Update
                .SetOnInsert(x => x.Id, ObjectId.GenerateNewId().ToString())
                .SetOnInsert(x => x.Date, currentDate) // กำหนดวันที่ตอนสร้างใหม่
                .Set(x => x.Username, faceResult.Username)
                .Set(x => x.Score, faceResult.Score);

            var options = new UpdateOptions { IsUpsert = true };

            await _collection.UpdateOneAsync(filter, update, options);
        }

        private async Task ReadAndNotify(string faceId)
        {
            var filter = Builders<NotificationDocument>.Filter.Eq(x => x.FaceId, faceId);
            var document = await _collection.Find(filter).FirstOrDefaultAsync();
            if (document != null)
            {
                // ส่ง Notification ผ่าน SignalR
                await _notificationService.SendSystemNotification(
                    document.FaceId,
                    document.Score,
                    document.Username,
                    document.RecommendedMenu,
                    $"User {document.Username} has score {document.Score} and recommended menu: {string.Join(", ", document.RecommendedMenu)}"
                );
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Closing RabbitMQ connections...");
            try
            {
                // ปิดทีละ Channel
                if (_channelFaceResult is not null && _channelFaceResult.IsOpen)
                    await _channelFaceResult.CloseAsync(cancellationToken);

                if (_channelRecommendedMenu is not null && _channelRecommendedMenu.IsOpen)
                    await _channelRecommendedMenu.CloseAsync(cancellationToken);

                if (_connection is not null && _connection.IsOpen)
                    await _connection.CloseAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"RabbitMQ closure warning: {ex.Message}");
            }
            finally
            {
                _channelFaceResult?.Dispose();
                _channelRecommendedMenu?.Dispose();
                _connection?.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
