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
                AcceptGroupInvitations = false,
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                LastModifiedDate = DateTimeOffset.Now
            };

            User demoUser2 = new()
            {
                //Id = 2, // Set by DB
                AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e1"),
                DisplayName = "TestUser2",
                AcceptGroupInvitations = true,
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                LastModifiedDate = DateTimeOffset.Now
            };

            User demoUser3 = new()
            {
                //Id = 3, // Set by DB
                AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e2"),
                DisplayName = "TestUser3",
                AcceptGroupInvitations = false,
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                LastModifiedDate = DateTimeOffset.Now
            };

            User demoUser4 = new()
            {
                //Id = 4, // Set by DB
                AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e4"),
                DisplayName = "TestUser4",
                AcceptGroupInvitations = true,
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                LastModifiedDate = DateTimeOffset.Now
            };

            User demoUser5 = new()
            {
                //Id = 5, // Set by DB
                AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e8ec"),
                DisplayName = "TestUser5",
                AcceptGroupInvitations = true,
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                LastModifiedDate = DateTimeOffset.Now
            };

            User demoUser6 = new()
            {
                //Id = 6, // Set by DB
                AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4efec"),
                DisplayName = "TestUser6",
                AcceptGroupInvitations = true,
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                LastModifiedDate = DateTimeOffset.Now
            };

            User demoUser7 = new()
            {
                //Id = 7, // Set by DB
                AadOid = new Guid("f09cc0b1-f05d-41e0-9684-c8a945d4efec"),
                DisplayName = "TestUser7",
                AcceptGroupInvitations = true,
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                LastModifiedDate = DateTimeOffset.Now
            };

            User demoUser8 = new()
            {
                //Id = 8, // Set by DB
                AadOid = new Guid("f09cc0b1-f08d-48e0-9684-c8a945d4efec"),
                DisplayName = "TestUser8",
                AcceptGroupInvitations = true,
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                LastModifiedDate = DateTimeOffset.Now
            };

            MessageGroup demoMG1 = new()
            {
                //Id = 1,
                MessageGroupName = "TestMessageGroup1",
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now
            };

            MessageGroup demoMG2 = new()
            {
                //Id = 2,
                MessageGroupName = "Test Message_Group2",
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now
            };

            MessageGroup demoMG3 = new()
            {
                //Id = 3,
                MessageGroupName = "Test Message_Group3",
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now
            };

            MessageGroup demoMG4 = new()
            {
                //Id = 4,
                MessageGroupName = "Test Message_Group4",
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now
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

            MessageGroupMember demoMGM6 = new()
            {
                //Id = 6,
                MessageGroupId = 4,
                MessageGroupRoleId = 1,
                UserId = 2
            };

            MessageGroupMember demoMGM7 = new()
            {
                //Id = 7,
                MessageGroupId = 1,
                MessageGroupRoleId = 2,
                UserId = 7
            };

            MessageGroupMember demoMGM8 = new()
            {
                //Id = 8,
                MessageGroupId = 1,
                MessageGroupRoleId = 1,
                UserId = 8
            };

            MessageGroupInvitation demoMGI1 = new()
            {
                //Id = 1,
                MessageGroupId = 1,
                InvitedUserId = 2,
                InvitingUserId = 1,
                InvitationActive = true,
                InvitationAccepted = false,
                CreatedDate = DateTimeOffset.Now
            };

            MessageGroupInvitation demoMGI2 = new()
            {
                //Id = 2,
                MessageGroupId = 2,
                InvitedUserId = 4,
                InvitingUserId = 1,
                InvitationActive = true,
                InvitationAccepted = false,
                CreatedDate = DateTimeOffset.Now
            };

            MessageGroupInvitation demoMGI3 = new()
            {
                //Id = 3,
                MessageGroupId = 3,
                InvitedUserId = 4,
                InvitingUserId = 1,
                InvitationActive = false,
                InvitationAccepted = false,
                CreatedDate = DateTimeOffset.Now.AddDays(-1),
                ActionDate = DateTimeOffset.Now,
            };

            MessageGroupInvitation demoMGI4 = new()
            {
                //Id = 4,
                MessageGroupId = 2,
                InvitedUserId = 5,
                InvitingUserId = 1,
                InvitationActive = true,
                InvitationAccepted = false,
                CreatedDate = DateTimeOffset.Now
            };

            MessageGroupInvitation demoMGI5 = new()
            {
                //Id = 5,
                MessageGroupId = 1,
                InvitedUserId = 6,
                InvitingUserId = 4,
                InvitationActive = true,
                InvitationAccepted = false,
                CreatedDate = DateTimeOffset.Now
            };

            MessageGroupInvitation demoMGI6 = new()
            {
                //Id = 6,
                MessageGroupId = 2,
                InvitedUserId = 6,
                InvitingUserId = 2,
                InvitationActive = true,
                InvitationAccepted = false,
                CreatedDate = DateTimeOffset.Now
            };

            MessageGroupInvitation demoMGI7 = new()
            {
                //Id = 7,
                MessageGroupId = 4,
                InvitedUserId = 1,
                InvitingUserId = 2,
                InvitationActive = true,
                InvitationAccepted = false,
                CreatedDate = DateTimeOffset.Now
            };

            Post demoPost1 = new()
            {
                //Id = 1, // Set by DB
                UserId = 1,
                MessageGroupId = 1,
                Message = "This is test message 1. Wow.",
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                StaleDate = null
            };

            Post demoPost2 = new()
            {
                //Id = 2, // Set by DB
                UserId = 1,
                MessageGroupId = 2,
                Message = "This is test message 2. Amazing.",
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                StaleDate = DateTimeOffset.Now.AddDays(1)
            };

            Post demoPost3 = new()
            {
                //Id = 3, // Set by DB
                UserId = 1,
                MessageGroupId = 1,
                Message = "This is test message 3. Wow.",
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                StaleDate = null
            };

            Post demoPost4 = new()
            {
                //Id = 4, // Set by DB
                UserId = 1,
                MessageGroupId = 1,
                Message = "This is test message 4. Wow.",
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                StaleDate = null
            };

            Post demoPost5 = new()
            {
                //Id = 5, // Set by DB
                UserId = 1,
                MessageGroupId = 1,
                Message = "This is test message 5. Stale!",
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                StaleDate = DateTimeOffset.Now.AddDays(-1)
            };

            Post demoPost6 = new()
            {
                //Id = 6, // Set by DB
                UserId = 2,
                MessageGroupId = 2,
                Message = "This is test message 6. Delete me!",
                IsDeleted = false,
                CreatedDate = DateTimeOffset.Now,
                StaleDate = DateTimeOffset.Now.AddDays(-1)
            };

            MessageGroupShortcut demoMGS1 = new()
            {
                //Id = 1,
                MessageGroupId = 1,
                UserId = 1
            };

            MessageGroupShortcut demoMGS2 = new()
            {
                //Id = 2,
                MessageGroupId = 2,
                UserId = 1
            };

            MessageGroupShortcut demoMGS3 = new()
            {
                //Id = 3,
                MessageGroupId = 3,
                UserId = 1
            };

            Acknowledgement demoAcknowledgement1 = new()
            {
                //Id = 1, // Set by DB
                PostId = 1,
                UserId = 1,
                CreatedDate = DateTimeOffset.Now
            };

            Acknowledgement demoAcknowledgement2 = new()
            {
                //Id = 2, // Set by DB
                PostId = 2,
                UserId = 1,
                CreatedDate = DateTimeOffset.Now
            };

            Acknowledgement demoAcknowledgement3 = new()
            {
                //Id = 3, // Set by DB
                PostId = 4,
                UserId = 4,
                CreatedDate = DateTimeOffset.Now
            };

            Acknowledgement demoAcknowledgement4 = new()
            {
                //Id = 4, // Set by DB
                PostId = 5,
                UserId = 4,
                CreatedDate = DateTimeOffset.Now
            };

            Acknowledgement demoAcknowledgement5 = new()
            {
                //Id = 5, // Set by DB
                PostId = 2,
                UserId = 2,
                CreatedDate = DateTimeOffset.Now
            };

            context.Users!.Add(demoUser1);
            context.Users!.Add(demoUser2);
            context.Users!.Add(demoUser3);
            context.Users!.Add(demoUser4);
            context.Users!.Add(demoUser5);
            context.Users!.Add(demoUser6);
            context.Users!.Add(demoUser7);
            context.Users!.Add(demoUser8);
            context.MessageGroups!.Add(demoMG1);
            context.MessageGroups!.Add(demoMG2);
            context.MessageGroups!.Add(demoMG3);
            context.MessageGroups!.Add(demoMG4);
            context.MessageGroupRoles!.Add(demoMGR1);
            context.MessageGroupRoles!.Add(demoMGR2);
            context.SaveChanges();
            context.MessageGroupMembers!.Add(demoMGM1);
            context.MessageGroupMembers!.Add(demoMGM2);
            context.MessageGroupMembers!.Add(demoMGM3);
            context.MessageGroupMembers!.Add(demoMGM4);
            context.MessageGroupMembers!.Add(demoMGM5);
            context.MessageGroupMembers!.Add(demoMGM6);
            context.MessageGroupMembers!.Add(demoMGM7);
            context.MessageGroupMembers!.Add(demoMGM8);
            context.Posts!.Add(demoPost1);
            context.Posts!.Add(demoPost2);
            context.Posts!.Add(demoPost3);
            context.Posts!.Add(demoPost4);
            context.Posts!.Add(demoPost5);
            context.Posts!.Add(demoPost6);
            context.SaveChanges();
            context.MessageGroupShortcuts!.Add(demoMGS1);
            context.MessageGroupShortcuts!.Add(demoMGS2);
            context.MessageGroupShortcuts!.Add(demoMGS3);
            context.MessageGroupInvitations!.Add(demoMGI1);
            context.MessageGroupInvitations!.Add(demoMGI2);
            context.MessageGroupInvitations!.Add(demoMGI3);
            context.MessageGroupInvitations!.Add(demoMGI4);
            context.MessageGroupInvitations!.Add(demoMGI5);
            context.MessageGroupInvitations!.Add(demoMGI6);
            context.MessageGroupInvitations!.Add(demoMGI7);
            context.Acknowledgements!.Add(demoAcknowledgement1);
            context.Acknowledgements!.Add(demoAcknowledgement2);
            context.Acknowledgements!.Add(demoAcknowledgement3);
            context.Acknowledgements!.Add(demoAcknowledgement4);
            context.Acknowledgements!.Add(demoAcknowledgement5);
            context.SaveChanges();
        }
    }
}
