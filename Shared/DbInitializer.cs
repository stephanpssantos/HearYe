namespace HearYe.Shared
{
    public static class DbInitializer
    {
        public static void Initialize(HearYeContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if (context.Users!.Any())
            {
                return; // DB has been seeded.
            }

            User demoUser = new()
            {
                //Id = 1, // Set by DB
                AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e0"),
                DisplayName = "TestUser",
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            Post demoPost1 = new()
            {
                //Id = 1, // Set by DB
                UserId = 1,
                Message = "This is test message 1. Wow.",
                CreatedDate = DateTime.Now,
                StaleDate = null
            };

            Post demoPost2 = new()
            {
                //Id = 2, // Set by DB
                UserId = 1,
                Message = "This is test message 2. Amazing.",
                CreatedDate = DateTime.Now,
                StaleDate = null
            };

            Acknowledgement demoAcknowledgement1 = new()
            {
                //Id = 1, // Set by DB
                PostId = 1,
                UserId = 1,
                CreatedDate = DateTime.Now
            };

            Acknowledgement demoAcknowledgement2 = new()
            {
                //Id = 2, // Set by DB
                PostId = 2,
                UserId = 1,
                CreatedDate = DateTime.Now
            };

            context.Users!.Add(demoUser);
            context.SaveChanges();
            context.Posts!.Add(demoPost1);
            context.Posts!.Add(demoPost2);
            context.SaveChanges();
            context.Acknowledgements!.Add(demoAcknowledgement1);
            context.Acknowledgements!.Add(demoAcknowledgement2);
            context.SaveChanges();
        }
    }
}
