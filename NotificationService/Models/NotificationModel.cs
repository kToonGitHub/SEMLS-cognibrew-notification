namespace NotificationService.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class NotificationDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("recommended_menu")]
        public List<string> RecommendedMenu { get; set; } = new();

        [BsonElement("face_id")]
        public string FaceId { get; set; } = string.Empty;

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("score")]
        public double Score { get; set; }

        [BsonElement("date")]
        public string Date { get; set; } = string.Empty; // รูปแบบ "yyyy-MM-dd"

    }
}
