using NotificationService.Models;

namespace NotificationService.Utilities
{
    public static class Utilities
    {
        // ใช้ Random เพื่อสุ่มเลือกข้อความในแต่ละหมวด
        private static readonly Random _random = new Random();

        // กลุ่มที่ 1: มีทั้ง Usual Order และ Upsell (20 รูปแบบ)
        // {0} = Name, {1} = UsualOrder, {2} = Upsell
        private static readonly string[] BothTemplates = new[]
        {
            "Hi {0}! The usual {1} today? Or maybe try our new {2} with it?",
            "Welcome back, {0}! We have your {1} ready to order. Care for some {2} on the side?",
            "Hello {0}! Time for your {1}? It pairs perfectly with {2}.",
            "Hey {0}, craving your {1}? We recommend pairing it with {2} today!",
            "Good to see you, {0}! How about your favorite {1} and a delicious {2}?",
            "Hi {0}! Let's get that {1} started for you. Would you like to add {2}?",
            "Welcome, {0}! Your {1} is waiting. Treat yourself to {2} as well!",
            "Hey {0}! Ready for your {1}? It goes great with our {2}.",
            "Hello {0}! We know you love {1}. Why not try it with {2}?",
            "Hi {0}! Brighten your day with your usual {1} and some {2}!",
            "Welcome back {0}! Is it {1} time? Add {2} to make it extra special.",
            "Hey {0}, your go-to {1} is calling! Want to match it with {2}?",
            "Good day {0}! Grab your {1} and maybe a bite of {2}?",
            "Hi {0}! Let's make today great with your {1} and a fresh {2}.",
            "Hello again, {0}! How about your {1} paired with our lovely {2}?",
            "Hey {0}! Thirsty for your {1}? Hungry for some {2}?",
            "Welcome {0}! Your {1} is a classic. Try it with {2} today!",
            "Hi {0}! We've got your {1} ready. Care to explore our {2}?",
            "Hello {0}! Kickstart your day with {1} and a delightful {2}.",
            "Hey {0}! Your {1} and our {2} are a match made in heaven."
        };

        // กลุ่มที่ 2: มีแค่ Usual Order (20 รูปแบบ)
        // {0} = Name, {1} = UsualOrder
        private static readonly string[] UsualOnlyTemplates = new[]
        {
            "Hi {0}! The usual {1} today?",
            "Welcome back, {0}! Craving your favorite {1}?",
            "Good to see you, {0}! Should we get your {1} started?",
            "Hello {0}! Is it time for your {1}?",
            "Hey {0}! We know you're here for the {1}!",
            "Welcome, {0}! Your go-to {1} is ready to be ordered.",
            "Hi {0}! Let's get you that {1} you love.",
            "Hey {0}! Ready for your daily {1}?",
            "Hello {0}! Another great day for your favorite {1}.",
            "Welcome back {0}! We have your {1} on standby.",
            "Hi {0}! Looking for your usual {1}?",
            "Hey {0}! Let us prepare your {1} for you.",
            "Good day {0}! Can we get your {1} going?",
            "Hello {0}! Treat yourself to your favorite {1} today.",
            "Hi {0}! Your {1} is calling your name!",
            "Welcome {0}! Need your {1} fix today?",
            "Hey {0}! We've got the perfect {1} waiting for you.",
            "Hello again, {0}! Time to enjoy your {1}.",
            "Hi {0}! Kick back and relax with your {1}.",
            "Welcome back, {0}! Let's make today a {1} kind of day."
        };

        // กลุ่มที่ 3: มีแค่ Upsell (20 รูปแบบ)
        // {0} = Name, {2} = Upsell (ใช้ {2} ตามตำแหน่งอาร์กิวเมนต์ใน string.Format)
        private static readonly string[] UpsellOnlyTemplates = new[]
        {
            "Hi {0}! Looking for something tasty? We highly recommend the {2}!",
            "Hello {0}! Treat yourself today with our delicious {2}.",
            "Welcome, {0}! Have you tried our {2} yet?",
            "Hey {0}! Discover something new today, like our {2}.",
            "Good to see you, {0}! Our {2} is fresh and ready for you.",
            "Hi {0}! Hungry? The {2} is a fan favorite.",
            "Welcome back, {0}! We suggest giving our {2} a try today.",
            "Hello {0}! Looking for a recommendation? Go for the {2}!",
            "Hey {0}! You deserve a treat. How about some {2}?",
            "Hi {0}! Spice up your day with our special {2}.",
            "Welcome {0}! Don't miss out on our freshly made {2}.",
            "Hello {0}! Craving something sweet? The {2} is perfect.",
            "Hey {0}! Give our amazing {2} a taste today!",
            "Good day {0}! We think you'll absolutely love the {2}.",
            "Hi {0}! Looking for a quick bite? Grab a {2}!",
            "Welcome back, {0}! Have a moment to enjoy our {2}?",
            "Hello {0}! The {2} is calling your name.",
            "Hey {0}! Ready for a delightful {2}?",
            "Hi {0}! Elevate your day with our signature {2}.",
            "Welcome {0}! How about trying our highly-rated {2}?"
        };

        // กลุ่มที่ 4: ไม่มีทั้ง Usual Order และ Upsell (20 รูปแบบ)
        // {0} = Name
        private static readonly string[] GenericTemplates = new[]
        {
            "Hi {0}! What can we get for you today?",
            "Welcome back, {0}! Ready to order?",
            "Hello {0}! We're ready when you are.",
            "Hey {0}! Great to see you again. What's on your mind?",
            "Good to see you, {0}! How can we make your day better?",
            "Hi {0}! Drop by and grab your favorites.",
            "Welcome, {0}! Check out our menu today.",
            "Hello {0}! Hope you're having a wonderful day. Ready for a drink?",
            "Hey {0}! Take a break and treat yourself.",
            "Hi {0}! We've missed you! What are you craving?",
            "Welcome back {0}! Let's get you something delicious.",
            "Hello {0}! Step right up and place your order.",
            "Hey {0}! Need a pick-me-up today?",
            "Good day {0}! Explore our menu and find something you love.",
            "Hi {0}! We are thrilled to serve you today.",
            "Welcome {0}! Your perfect order is just a few taps away.",
            "Hello again, {0}! What sounds good right now?",
            "Hey {0}! Let us know what you'd like to have.",
            "Hi {0}! Ready to explore our fresh offerings?",
            "Welcome back, {0}! We are here to craft your perfect drink."
        };

        /// <summary>
        /// ฟังก์ชันสำหรับสร้างข้อความทักทายลูกค้า
        /// </summary>
        /// <param name="firstName">ชื่อลูกค้า</param>
        /// <param name="usualOrder">เมนูประจำ</param>
        /// <param name="upsell">เมนูแนะนำเพิ่มเติม</param>
        /// <returns>ข้อความทักทายที่ถูกจัดรูปแบบแล้ว</returns>
        public static string GetGreetingMessage(string firstName, string usualOrder, string upsell)
        {
            // ตรวจสอบว่ามีข้อมูลส่งมาหรือไม่
            bool hasUsual = !string.IsNullOrWhiteSpace(usualOrder);
            bool hasUpsell = !string.IsNullOrWhiteSpace(upsell);

            // เผื่อกรณี firstName เป็น null หรือ empty ให้ใช้คำว่า "there" แทน
            string name = string.IsNullOrWhiteSpace(firstName) ? "there" : firstName;

            string[] selectedTemplates;

            // เลือกหมวดหมู่ของ Template ตามข้อมูลที่มี
            if (hasUsual && hasUpsell)
            {
                selectedTemplates = BothTemplates;
            }
            else if (hasUsual && !hasUpsell)
            {
                selectedTemplates = UsualOnlyTemplates;
            }
            else if (!hasUsual && hasUpsell)
            {
                selectedTemplates = UpsellOnlyTemplates;
            }
            else
            {
                selectedTemplates = GenericTemplates;
            }

            // สุ่มเลือก Template จากหมวดที่ตรงเงื่อนไข
            int index = _random.Next(selectedTemplates.Length);
            string template = selectedTemplates[index];

            // นำข้อมูลไปแทนที่ใน Placeholder: {0} = Name, {1} = UsualOrder, {2} = Upsell
            return string.Format(template, name, usualOrder, upsell);
        }

        public static NotificationMessage MapData(NotificationDocument document)
        {
            // ทำการ Mapping ข้อมูลจาก NotificationDocument ไปเป็นรูปแบบที่ต้องการส่งผ่าน SignalR
            // เช่น สร้าง ViewModel หรือ DTO ที่เหมาะสม

            string fullName = $"{document.FirstName} {document.LastName}".Trim();
            string status = string.IsNullOrEmpty(document.Rank) ? "Guest" : $"{document.Rank} Member";
            string usualOrder = document.RecommendedMenu.FirstOrDefault() ?? "";
            string usualSweetness = "0%"; // ตัวอย่างค่า sweetness ที่แนะนำ
            string upsell = document.RecommendedMenu.Count > 1 ? document.RecommendedMenu[1] : ""; // ตัวอย่างการเลือก upsell item
            bool isRecommendationDown = document.RecommendedMenu.Count == 0;
            bool isGuest = string.IsNullOrEmpty(document.Username) || string.Compare(document.Username.Trim(), "Unknown", StringComparison.OrdinalIgnoreCase) == 0;
            string greeting = GetGreetingMessage(fullName, usualOrder, upsell);
            return new NotificationMessage
            {
                Customer = new CustomerData
                {
                    Id = document.FaceId,
                    Name = document.Username,
                    FirstName = fullName,
                    Status = status,
                    Points = document.Points,
                    Rank = document.Rank,
                    Image = document.ImageBase64,
                    UsualOrderId = usualOrder,
                    UsualOrder = usualOrder,
                    UsualSweetness = usualSweetness,
                    UpsellId = upsell,
                    Upsell = upsell,
                    Greeting = greeting,
                    IsGuest = isGuest,
                    IsRecommendationDown = isRecommendationDown,
                    UsualOrderIcon = "",
                    OrderId = ""
                }
            };
        }
    }
}
