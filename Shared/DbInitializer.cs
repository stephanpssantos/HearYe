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

            context.Users!.Add(demoUser1);
            context.Users!.Add(demoUser2);
            context.MessageGroups!.Add(demoMG1);
            context.MessageGroups!.Add(demoMG2);
            context.MessageGroupRoles!.Add(demoMGR1);
            context.MessageGroupRoles!.Add(demoMGR2);
            context.SaveChanges();
            context.MessageGroupMembers!.Add(demoMGM1);
            context.MessageGroupMembers!.Add(demoMGM2);
            context.MessageGroupMembers!.Add(demoMGM3);
            context.Posts!.Add(demoPost1);
            context.Posts!.Add(demoPost2);
            context.SaveChanges();
            context.Acknowledgements!.Add(demoAcknowledgement1);
            context.Acknowledgements!.Add(demoAcknowledgement2);
            context.SaveChanges();
        }
    }
}
