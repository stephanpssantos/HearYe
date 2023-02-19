// See: https://stackoverflow.com/questions/69659645/c-sharp-mock-unit-test-graphserviceclient
// And: https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database
// And: https://gunnarpeipman.com/aspnet-core-test-controller-fake-user/

using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace HearYe.Server.Tests
{
    public class UserControllerTests : IClassFixture<HearYeDatabaseFixture>
    {
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly ILogger<UserController> _logger;
        private readonly GraphServiceClient _graphServiceClient;

        public UserControllerTests(HearYeDatabaseFixture fixture)
        {
            Mock<IAuthenticationProvider> mockAuthProvider = new ();
            Mock<IHttpProvider> mockHttpProvider = new ();
            Mock<ILogger<UserController>> mockLogger = new ();
            _authenticationProvider = mockAuthProvider.Object;
            _httpProvider = mockHttpProvider.Object;
            _logger = mockLogger.Object;

            Mock<GraphServiceClient> mockGraphClient = new(_authenticationProvider, _httpProvider);
            _graphServiceClient = mockGraphClient.Object;

            Fixture = fixture;
        }

        public HearYeDatabaseFixture Fixture { get; }

        [Fact]
        public async void GetUser_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAnonymousIdentity();

                // Act
                var result = await controller.GetUser(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetUser_ReturnsUnauthorizedWhenNotRequestingSelf()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetUser(2);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetUser_ReturnsNotFoundWhenRequestingNonexistentUser()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("9999");

                // Act
                var result = await controller.GetUser(9999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void GetUser_ReturnsUserWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetUser(1);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as Shared.User;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Equal(1, resultBody!.Id);
                Assert.Equal(new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e0"), resultBody.AadOid);
                Assert.Equal("TestUser", resultBody.DisplayName);
            }
        }

        [Fact]
        public async void GetUserPublicInfo_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAnonymousIdentity();

                // Act
                var result = await controller.GetUserPublicInfo(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetUserPublicInfo_ReturnsNotFoundWhenRequestingNonexistentUser()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetUserPublicInfo(9999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void GetUserPublicInfo_ReturnsUserWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetUserPublicInfo(1);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as Shared.UserPublicInfo;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Equal(1, resultBody!.Id);
                Assert.Equal("TestUser", resultBody.DisplayName);
                Assert.False(resultBody.AcceptGroupInvitations);
            }
        }

        [Fact]
        public async void GetUserByOid_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAnonymousIdentity();

                // Act
                var result = await controller.GetUserByOid("test");

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetUserByOid_ReturnsBadRequestWhenPassingInvalidGuid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetUserByOid("test");

                // Assert
                Assert.IsType<BadRequestResult>(result);
            }
        }

        [Fact]
        public async void GetUserByOid_ReturnsNotFoundWhenRequestedUserDoesNotExist()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetUserByOid("f09cc0b1-f05d-40e0-9684-c4a945d4e7ff");

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void GetUserByOid_ReturnsUnauthorizedWhenNotRequestingSelf()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetUserByOid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e1");

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetUserByOid_ReturnsUserWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetUserByOid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e0");
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as Shared.User;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Equal(1, resultBody!.Id);
                Assert.Equal(new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e0"), resultBody.AadOid);
                Assert.Equal("TestUser", resultBody.DisplayName);
            }
        }

        [Fact]
        public async void GetUserMessageGroups_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAnonymousIdentity();

                // Act
                var result = await controller.GetUserMessageGroups(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetUserMessageGroups_ReturnsUnauthorizedWhenNotRequestingSelf()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetUserMessageGroups(2);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetUserMessageGroups_ReturnsEmptyCollectionWhenNoGroupMembership()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("3");

                // Act
                var result = await controller.GetUserMessageGroups(3);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as IEnumerable<MessageGroup>;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.NotNull(resultBody);
                Assert.IsAssignableFrom<IEnumerable<MessageGroup>>(resultBody);
                Assert.Empty(resultBody);
            }
        }

        [Fact]
        public async void GetUserMessageGroups_ReturnsMessageGroupCollection()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetUserMessageGroups(1);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as IEnumerable<MessageGroup>;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.NotNull(resultBody);
                Assert.IsAssignableFrom<IEnumerable<MessageGroup>>(resultBody);
                Assert.Equal(3, resultBody.Where(mg => mg.MessageGroupName != "UnitTestGroup").Count());
            }
        }

        [Fact]
        public async void NewUser_ReturnsBadRequestWhenUserIsInvalid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAnonymousIdentity();
                controller.ModelState.AddModelError("AadOid", "Required key missing.");
                var newUser = new Shared.User();

                // Act
                var result = await controller.NewUser(newUser);
                var resultBody = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(resultBody!.Value, "Valid user object required.");
            }
        }

        [Fact]
        public async void NewUser_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAnonymousIdentity();
                Shared.User newUser = new()
                {
                    AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7f9"),
                    DisplayName = "TestUser_1",
                    AcceptGroupInvitations = true,
                    IsDeleted = false,
                    CreatedDate = DateTimeOffset.Now,
                    LastModifiedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.NewUser(newUser);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void NewUser_ReturnsBadRequestWhenNotCreatingSelf()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");
                Shared.User newUser = new()
                {
                    AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7f9"),
                    DisplayName = "TestUser_1",
                    AcceptGroupInvitations = true,
                    IsDeleted = false,
                    CreatedDate = DateTimeOffset.Now,
                    LastModifiedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.NewUser(newUser);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        [Fact]
        public async void NewUser_WhenGraphFailsUserCreationIsRolledBack()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                Shared.User newUser = new()
                {
                    AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7f9"),
                    DisplayName = "TestUser_fail_1",
                    AcceptGroupInvitations = true,
                    IsDeleted = false,
                    CreatedDate = DateTimeOffset.Now,
                    LastModifiedDate = DateTimeOffset.Now
                };

                var controller = new UserController(context, _graphServiceClient, _logger)
                    .WithAuthenticatedIdentity("1", "f09cc0b1-f05d-40e0-9684-c4a945d4e7f9");

                // Act
                var result = await controller.NewUser(newUser);
                var resultBody = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Failed to register graph record for new user.", resultBody!.Value);
                Assert.DoesNotContain(context.Users!, users => users.DisplayName == "TestUser_fail_1");
            }
        }

        [Fact]
        public async void NewUser_ReturnsCreatedAtRouteResultWhenValid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                Shared.User newUser = new()
                {
                    AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4a7f9"),
                    DisplayName = "TestUser_1",
                    AcceptGroupInvitations = true,
                    IsDeleted = false,
                    CreatedDate = DateTimeOffset.Now,
                    LastModifiedDate = DateTimeOffset.Now
                };

                Microsoft.Graph.User newGraphUser = new()
                {
                    AdditionalData = new Dictionary<string, object>()
                    {
                        { "extension_9ad29a8ab7fc468aa9c975e45b6eb34e_DatabaseId", "8" }
                    }
                };

                Mock<GraphServiceClient> customMockGraphClient = new(_authenticationProvider, _httpProvider);
                customMockGraphClient.Setup(g => g.Users[newUser.AadOid.ToString()]
                    .Request()
                    .UpdateAsync(newGraphUser, CancellationToken.None))
                    .ReturnsAsync(() => null);

                var controller = new UserController(context, customMockGraphClient.Object, _logger)
                    .WithAuthenticatedIdentity("8", "f09cc0b1-f05d-40e0-9684-c4a945d4a7f9");

                // Act
                var result = await controller.NewUser(newUser);

                // Assert
                Assert.IsType<CreatedAtRouteResult>(result);
            }
        }

        [Fact]
        public async void UpdateUser_ReturnsBadRequestWhenIdInvalid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAnonymousIdentity();
                Shared.User newUser = new()
                {
                    AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7f9"),
                    DisplayName = "TestUser_1",
                    AcceptGroupInvitations = true,
                    IsDeleted = false,
                    CreatedDate = DateTimeOffset.Now,
                    LastModifiedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.UpdateUser(99, newUser);
                var resultBody = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(resultBody!.Value, "User ID mismatch.");
            }
        }

        [Fact]
        public async void UpdateUser_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAnonymousIdentity();
                Shared.User newUser = new()
                {
                    Id = 4,
                    AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7f9"),
                    DisplayName = "TestUser_1",
                    AcceptGroupInvitations = true,
                    IsDeleted = false,
                    CreatedDate = DateTimeOffset.Now,
                    LastModifiedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.UpdateUser(4, newUser);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void UpdateUser_ReturnsUnauthorizedWhenNotUpdatingSelf()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");
                Shared.User newUser = new()
                {
                    Id = 4,
                    AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7f9"),
                    DisplayName = "TestUser_1",
                    AcceptGroupInvitations = true,
                    IsDeleted = false,
                    CreatedDate = DateTimeOffset.Now,
                    LastModifiedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.UpdateUser(4, newUser);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void UpdateUser_ReturnsNotFoundWhenUserNonExistent()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("99");
                Shared.User newUser = new()
                {
                    Id = 99,
                    AadOid = new Guid("f09cc0b1-f05d-40e0-9684-c4a945d4e7e0"),
                    DisplayName = "TestUser_1",
                    AcceptGroupInvitations = true,
                    IsDeleted = false,
                    CreatedDate = DateTimeOffset.Now,
                    LastModifiedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.UpdateUser(99, newUser);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }


        [Fact]
        public async void UpdateUser_UpdatesUserWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("3");
                Shared.User existingUser = context.Users!.Where(x => x.Id == 3).First();
                existingUser.DisplayName = "TestUser3_Updated";

                // Act
                var result = await controller.UpdateUser(3, existingUser);

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.Equal("TestUser3_Updated", context.Users!.Where(x => x.Id == 3).First().DisplayName);
            }
        }

        [Fact]
        public async void UpdateUser_OnlyUpdatesUnprotectedFields()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("3");
                var aDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
                var aGuid = Guid.NewGuid();

                Shared.User userUpdates = new()
                {
                    // Unprotected Fields
                    DisplayName = "TestUser3_Updated",
                    LastModifiedDate = DateTimeOffset.Now,
                    AcceptGroupInvitations = false,
                    DefaultGroupId = 1,
                    // Protected Fields
                    Id = 3,
                    AadOid = aGuid,
                    IsDeleted = true,
                    CreatedDate = aDate,
                    DeletedDate = aDate,
                };

                // Act
                var result = await controller.UpdateUser(3, userUpdates);
                var user = context.Users!.Where(x => x.Id == 3).First();

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.Equal("TestUser3_Updated", user.DisplayName);
                Assert.NotEqual(aGuid, user.AadOid);
                Assert.False(user.IsDeleted);
                Assert.NotEqual(aDate, user.CreatedDate);
                Assert.NotEqual(aDate, user.DeletedDate);
            }
        }

        [Fact]
        public async void DeleteUser_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAnonymousIdentity();

                // Act
                var result = await controller.DeleteUser(4);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void DeleteUser_ReturnsUnauthorizedWhenNotDeletingSelf()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.DeleteUser(4);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void DeleteUser_ReturnsNotFoundWhenRequestedUserDoesntExist()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("99");

                // Act
                var result = await controller.DeleteUser(99);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void DeleteUser_ReturnsNoContentWhenUserDeleted()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new UserController(context, _graphServiceClient, _logger).WithAuthenticatedIdentity("7");

                // Act
                var result = await controller.DeleteUser(7);

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.Null(context.Users!.Where(x => x.Id == 7).FirstOrDefault());
            }
        }
    }
}