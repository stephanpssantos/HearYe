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

            User demoUser1 = new()
            {
                //Id = 1, // Set by DB
                AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e0"),
                DisplayName = "TestUser",
                AcceptGroupInvitations = true,
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            User demoUser2 = new()
            {
                //Id = 2, // Set by DB
                AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e1"),
                DisplayName = "TestUser2",
                AcceptGroupInvitations = true,
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            User demoUser3 = new()
            {
                //Id = 3, // Set by DB
                AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e2"),
                DisplayName = "TestUser3",
                AcceptGroupInvitations = true,
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            User demoUser4 = new()
            {
                //Id = 4, // Set by DB
                AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e4"),
                DisplayName = "TestUser4",
                AcceptGroupInvitations = true,
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            MessageGroup demoMG1 = new()
            {
                //Id = 1,
                MessageGroupName = "TestMessageGroup1",
                IsDeleted = false,
                CreatedDate = DateTime.Now
            };

            MessageGroup demoMG2 = new()
            {
                //Id = 2,
                MessageGroupName = "Test Message_Group2",
                IsDeleted = false,
                CreatedDate = DateTime.Now
            };

            MessageGroup demoMG3 = new()
            {
                //Id = 3,
                MessageGroupName = "Test Message_Group3",
                IsDeleted = false,
                CreatedDate = DateTime.Now
            };

            MessageGroupRole demoMGR1 = new()
            {
                //Id = 1,
                RoleName = "Admin"
            };

            MessageGroupRole demoMGR2 = new()
            {
                //Id = 2,
                RoleName = "User"
            };

            MessageGroupMember demoMGM1 = new()
            {
                //Id = 1,
                MessageGroupId = 1,
                MessageGroupRoleId = 1,
                UserId = 1
            };

            MessageGroupMember demoMGM2 = new()
            {
                //Id = 2,
                MessageGroupId = 2,
                MessageGroupRoleId = 1,
                UserId = 1
            };

            MessageGroupMember demoMGM3 = new()
            {
                //Id = 3,
                MessageGroupId = 2,
                MessageGroupRoleId = 2,
                UserId = 2
            };

            MessageGroupMember demoMGM4 = new()
            {
                //Id = 4,
                MessageGroupId = 1,
                MessageGroupRoleId = 2,
                UserId = 4
            };

            MessageGroupMember demoMGM5 = new()
            {
                //Id = 5,
                MessageGroupId = 3,
                MessageGroupRoleId = 1,
                UserId = 1
            };

            Post demoPost1 = new()
            {
                //Id = 1, // Set by DB
                UserId = 1,
                MessageGroupId = 1,
                Message = "This is test message 1. Wow.",
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                StaleDate = null
            };

            Post demoPost2 = new()
            {
                //Id = 2, // Set by DB
                UserId = 1,
                MessageGroupId = 2,
                Message = "This is test message 2. Amazing.",
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                StaleDate = DateTime.Now.AddDays(1)
            };

            Post demoPost3 = new()
            {
                //Id = 3, // Set by DB
                UserId = 1,
                MessageGroupId = 1,
                Message = "This is test message 3. Wow.",
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                StaleDate = null
            };

            Post demoPost4 = new()
            {
                //Id = 4, // Set by DB
                UserId = 1,
                MessageGroupId = 1,
                Message = "This is test message 4. Wow.",
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                StaleDate = null
            };

            Post demoPost5 = new()
            {
                //Id = 5, // Set by DB
                UserId = 1,
                MessageGroupId = 1,
                Message = "This is test message 5. Stale!",
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                StaleDate = DateTime.Now.AddDays(-1)
            };

            Post demoPost6 = new()
            {
                //Id = 6, // Set by DB
                UserId = 2,
                MessageGroupId = 2,
                Message = "This is test message 6. Delete me!",
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                StaleDate = DateTime.Now.AddDays(-1)
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

            Acknowledgement demoAcknowledgement3 = new()
            {
                //Id = 3, // Set by DB
                PostId = 4,
                UserId = 4,
                CreatedDate = DateTime.Now
            };

            Acknowledgement demoAcknowledgement4 = new()
            {
                //Id = 4, // Set by DB
                PostId = 5,
                UserId = 4,
                CreatedDate = DateTime.Now
            };

            context.Users!.Add(demoUser1);
            context.Users!.Add(demoUser2);
            context.Users!.Add(demoUser3);
            context.Users!.Add(demoUser4);
            context.MessageGroups!.Add(demoMG1);
            context.MessageGroups!.Add(demoMG2);
            context.MessageGroups!.Add(demoMG3);
            context.MessageGroupRoles!.Add(demoMGR1);
            context.MessageGroupRoles!.Add(demoMGR2);
            context.SaveChanges();
            context.MessageGroupMembers!.Add(demoMGM1);
            context.MessageGroupMembers!.Add(demoMGM2);
            context.MessageGroupMembers!.Add(demoMGM3);
            context.MessageGroupMembers!.Add(demoMGM4);
            context.MessageGroupMembers!.Add(demoMGM5);
            context.Posts!.Add(demoPost1);
            context.Posts!.Add(demoPost2);
            context.Posts!.Add(demoPost3);
            context.Posts!.Add(demoPost4);
            context.Posts!.Add(demoPost5);
            context.Posts!.Add(demoPost6);
            context.SaveChanges();
            context.Acknowledgements!.Add(demoAcknowledgement1);
            context.Acknowledgements!.Add(demoAcknowledgement2);
            context.Acknowledgements!.Add(demoAcknowledgement3);
            context.Acknowledgements!.Add(demoAcknowledgement4);
            context.SaveChanges();
        }
    }
}
