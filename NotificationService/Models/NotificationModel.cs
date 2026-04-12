namespace NotificationService.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using System.Text.Json.Serialization;

    public class NotificationDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }


        // From the face recognition system, we can store the face ID and username for reference
        [BsonElement("face_id")] // Correlation ID from the face recognition system
        public string FaceId { get; set; } = string.Empty;

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("score")]
        public double Score { get; set; }


        // From the recommendation system, we can store a list of recommended menu items for the user
        [BsonElement("recommended_menu")]
        public List<string> RecommendedMenu { get; set; } = new();


        // From member service, we can store the member's first name, last name, membership status, loyalty points, and profile picture (as a base64 string)
        [BsonElement("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [BsonElement("last_name")] 
        public string LastName { get; set; } = string.Empty;

        [BsonElement("rank")]
        public string Rank { get; set; } = string.Empty; // Rank of this member like "Silver", "Gold", "Platinum"

        [BsonElement("points")]
        public int Points { get; set; } // Loyalty points accumulated by the member

        [BsonElement("image_base64")]
        public string ImageBase64 { get; set; } = string.Empty; // Base64-encoded image data for the member's profile picture


        // Generic field to store any additional information as needed
        [BsonElement("date")]
        public string Date { get; set; } = string.Empty; // รูปแบบ "yyyy-MM-dd"

    }


    // NotificationMessage is the message format that the NotificationService will send to the clients via SignalR
    public class NotificationMessage
    {
        [JsonPropertyName("customer")]
        public required CustomerData Customer { get; set; }
    }

    public class CustomerData
    {
        // OK face_id (unique vector ID) shall be used instead of customer ID
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        // OK - username
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        // unavailable [Waiting for MemberService] mock - full name instead
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        // unavailable [Waiting for MemberService] mock
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        // unavailable [Waiting for MemberService] mock
        [JsonPropertyName("points")]
        public int? Points { get; set; }

        // unavailable [Waiting for MemberService] mock
        [JsonPropertyName("rank")]
        public string? Rank { get; set; }

        // unavailable [Waiting for MemberService] base64
        [JsonPropertyName("image")]
        public string? Image { get; set; }

        // menu name
        [JsonPropertyName("usualOrderId")]
        public string? UsualOrderId { get; set; }

        // menu name
        [JsonPropertyName("usualOrder")]
        public string? UsualOrder { get; set; }

        // 0% - hardcode
        [JsonPropertyName("usualSweetness")]
        public string? UsualSweetness { get; set; }

        // dessert menu name
        [JsonPropertyName("upsellId")]
        public string? UpsellId { get; set; }

        // dessert menu name
        [JsonPropertyName("upsell")]
        public string? Upsell { get; set; }

        // template
        [JsonPropertyName("greeting")]
        public string? Greeting { get; set; }

        // depends on recognition service message: unknown then true
        [JsonPropertyName("isGuest")]
        public bool IsGuest { get; set; }

        [JsonPropertyName("isRecommendationDown")]
        public bool IsRecommendationDown { get; set; }

        [JsonPropertyName("usualOrderIcon")]
        public string? UsualOrderIcon { get; set; }

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }
    }
}
