using Microsoft.Extensions.Logging;

namespace HearYe.Server.Tests
{
    public class AcknowledgementControllerTests : IClassFixture<HearYeDatabaseFixture>
    {
        private readonly ILogger<AcknowledgementController> logger;

        public AcknowledgementControllerTests(HearYeDatabaseFixture fixture)
        {
            Mock<ILogger<AcknowledgementController>> mockLogger = new ();
            this.logger = mockLogger.Object;
            Fixture = fixture;
        }

        public HearYeDatabaseFixture Fixture { get; }

        [Fact]
        public async void NewAcknowledgement_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new AcknowledgementController(context, this.logger).WithAnonymousIdentity();
                var acknowledgement = new Acknowledgement();

                // Act
                var result = await controller.NewAcknowledgement(acknowledgement);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void NewAcknowledgement_ReturnsUnauthorizedWhenNotPostingAsSelf()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new AcknowledgementController(context, this.logger).WithAuthenticatedIdentity("1");
                var acknowledgement = new Acknowledgement()
                {
                    PostId = 1,
                    UserId = 2,
                    CreatedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.NewAcknowledgement(acknowledgement);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void NewAcknowledgement_ReturnsBadRequestWhenPostNonExistent()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new AcknowledgementController(context, this.logger).WithAuthenticatedIdentity("2");
                var acknowledgement = new Acknowledgement()
                {
                    PostId = 9999,
                    UserId = 2,
                    CreatedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.NewAcknowledgement(acknowledgement);

                // Assert
                Assert.IsType<BadRequestResult>(result);
            }
        }

        [Fact]
        public async void NewAcknowledgement_ReturnsUnauthorizedWhenNotGroupMember()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new AcknowledgementController(context, this.logger).WithAuthenticatedIdentity("2");
                var acknowledgement = new Acknowledgement()
                {
                    PostId = 1,
                    UserId = 2,
                    CreatedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.NewAcknowledgement(acknowledgement);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void NewAcknowledgement_CreatesNewAcknowledgementWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new AcknowledgementController(context, this.logger).WithAuthenticatedIdentity("4");
                var acknowledgement = new Acknowledgement()
                {
                    PostId = 3,
                    UserId = 4,
                    CreatedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.NewAcknowledgement(acknowledgement);
                var verify = context.Acknowledgements!
                    .Where(a => a.PostId == 3 && a.UserId == 4)
                    .FirstOrDefault();

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.NotNull(verify);
            }
        }

        [Fact]
        public async void NewAcknowledgement_ReturnsBadRequestWhenDoublingAcknowledgement()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new AcknowledgementController(context, this.logger).WithAuthenticatedIdentity("4");
                var acknowledgement = new Acknowledgement()
                {
                    PostId = 4,
                    UserId = 4,
                    CreatedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.NewAcknowledgement(acknowledgement);
                var resultObject = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Error when creating new acknowledgement.", resultObject!.Value);
            }
        }

        [Fact]
        public async void DeleteAcknowledgement_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new AcknowledgementController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.DeleteAcknowledgement(1, 1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void DeleteAcknowledgement_ReturnsBadRequestWhenIdInvalid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new AcknowledgementController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.DeleteAcknowledgement(0, 0);

                // Assert
                Assert.IsType<BadRequestResult>(result);
            }
        }

        [Fact]
        public async void DeleteAcknowledgement_ReturnsNotFoundWhenInviteNonExistent()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new AcknowledgementController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.DeleteAcknowledgement(9999, 9999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void DeleteAcknowledgement_ReturnsUnauthorizedWhenNotAcknowledger()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new AcknowledgementController(context, this.logger).WithAuthenticatedIdentity("4");

                // Act
                var result = await controller.DeleteAcknowledgement(1, 1);
                var resultObject = result as UnauthorizedObjectResult;

                // Assert
                Assert.IsType<UnauthorizedObjectResult>(result);
                Assert.Equal("Not poster of acknowledgement.", resultObject!.Value);
            }
        }

        [Fact]
        public async void DeleteAcknowledgement_ReturnsNoContentResultWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new AcknowledgementController(context, this.logger).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.DeleteAcknowledgement(2, 2);
                var verify = context.Acknowledgements!.Where(a => a.Id == 5).FirstOrDefault();

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.Null(verify);
            }
        }
    }
}
