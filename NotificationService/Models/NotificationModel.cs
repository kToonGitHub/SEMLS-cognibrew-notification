namespace NotificationService.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class NotificationDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("device_id")]
        public string DeviceId { get; set; } = string.Empty;

        [BsonElement("recommended_menu")]
        public List<string> recommendedMenu { get; set; } = new();
    }
}
