using HearYe.Shared;
using Microsoft.Extensions.Logging;

namespace HearYe.Server.Tests
{
    public class MessageGroupShortcutControllerTests : IClassFixture<HearYeDatabaseFixture>
    {
        private readonly ILogger<MessageGroupShortcutController> logger;

        public MessageGroupShortcutControllerTests(HearYeDatabaseFixture fixture)
        {
            Mock<ILogger<MessageGroupShortcutController>> mockLogger = new();
            this.logger = mockLogger.Object;
            Fixture = fixture;
        }

        public HearYeDatabaseFixture Fixture { get; }

        [Fact]
        public async void GetMessageGroupShortcuts_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.GetMessageGroupShortcuts(9999);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetMessageGroupShortcuts_ReturnsUnauthorizedWhenNotRequestingOwn()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetMessageGroupShortcuts(2);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetMessageGroupShortcuts_ReturnsEmptyListWhenNoShortcuts()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.GetMessageGroupShortcuts(2);
                var resultBody = result as OkObjectResult;
                var mg = resultBody!.Value as IEnumerable<MessageGroup?>;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Empty(mg!);
            }
        }

        [Fact]
        public async void GetMessageGroupShortcuts_ReturnsMessageGroupListWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetMessageGroupShortcuts(1);
                var resultBody = result as OkObjectResult;
                var mg = resultBody!.Value as IEnumerable<MessageGroup?>;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Contains(mg!, mg => mg!.Id == 1);
                Assert.Contains(mg!, mg => mg!.Id == 2);
            }
        }

        [Fact]
        public async void NewMessageGroupShortcut_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAnonymousIdentity();
                var newShortcut = new MessageGroupShortcut()
                {
                    MessageGroupId = 2,
                    UserId = 2,
                };

                // Act
                var result = await controller.NewMessageGroupShortcut(newShortcut);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void NewMessageGroupShortcut_ReturnsUnauthorizedWhenNotGroupMember()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAuthenticatedIdentity("2");
                var newShortcut = new MessageGroupShortcut()
                {
                    MessageGroupId = 3,
                    UserId = 2,
                };

                // Act
                var result = await controller.NewMessageGroupShortcut(newShortcut);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void NewMessageGroupShortcut_ReturnsBadRequestWhenShortcutAlreadyExists()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAuthenticatedIdentity("1");
                var newShortcut = new MessageGroupShortcut()
                {
                    MessageGroupId = 1,
                    UserId = 1,
                };

                // Act
                var result = await controller.NewMessageGroupShortcut(newShortcut);
                var resultObject = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Duplicate shortcut exists.", resultObject!.Value);
            }
        }

        [Fact]
        public async void NewMessageGroupShortcut_ReturnsNoContentWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAuthenticatedIdentity("2");
                var newShortcut = new MessageGroupShortcut()
                {
                    MessageGroupId = 2,
                    UserId = 2,
                };

                // Act
                var result = await controller.NewMessageGroupShortcut(newShortcut);
                var verify = context.MessageGroupShortcuts!.FirstOrDefault(x => x.MessageGroupId == 2 && x.UserId == 2);

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.NotNull(verify);
            }
        }

        [Fact]
        public async void DeleteMessageGroupShortcut_ReturnsBadRequestWhenIdsInvalid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.DeleteMessageGroupShortcut(0, 0);

                // Assert
                Assert.IsType<BadRequestResult>(result);
            }
        }

        [Fact]
        public async void DeleteMessageGroupShortcut_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.DeleteMessageGroupShortcut(2, 2);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void DeleteMessageGroupShortcut_ReturnsNotFoundWhenGroupNonexistent()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.DeleteMessageGroupShortcut(2, 9999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void DeleteMessageGroupShortcut_ReturnsUnauthorizedWhenNotShortcutOwner()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.DeleteMessageGroupShortcut(1, 2);
                var resultObject = result as UnauthorizedObjectResult;

                // Assert
                Assert.IsType<UnauthorizedObjectResult>(result);
                Assert.Equal("Not shortcut owner.", resultObject!.Value);
            }
        }

        [Fact]
        public async void DeleteMessageGroupShortcut_ReturnsNoContentWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupShortcutController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.DeleteMessageGroupShortcut(1, 3);
                var verify = context.MessageGroupShortcuts!.FirstOrDefault(x => x.MessageGroupId == 1 && x.UserId == 3);

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.Null(verify);
            }
        }
    }
}
