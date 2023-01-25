using HearYe.Shared;
using Microsoft.Extensions.Logging;

namespace HearYe.Server.Tests
{
    public class MessageGroupInvitationControllerTests : IClassFixture<HearYeDatabaseFixture>
    {
        private readonly ILogger<MessageGroupInvitationController> logger;

        public MessageGroupInvitationControllerTests(HearYeDatabaseFixture fixture)
        {
            Mock<ILogger<MessageGroupInvitationController>> mockLogger = new();
            this.logger = mockLogger.Object;
            Fixture = fixture;
        }

        public HearYeDatabaseFixture Fixture { get; }

        [Fact]
        public async void GetMessageGroupInvitation_ReturnsNotFoundWhenNonExistent()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.GetMessageGroupInvitation(9999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void GetMessageGroupInvitation_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.GetMessageGroupInvitation(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetMessageGroupInvitation_ReturnsUnauthorizedWhenNotInvolvedWithInvite()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("3");

                // Act
                var result = await controller.GetMessageGroupInvitation(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetMessageGroupInvitation_ReturnsMessageGroupInvitationWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetMessageGroupInvitation(1);
                var resultObject = result as OkObjectResult;
                var resultBody = resultObject!.Value as MessageGroupInvitation;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Equal(1, resultBody!.Id);
                Assert.Equal(1, resultBody!.MessageGroupId);
                Assert.Equal(2, resultBody!.InvitedUserId);
                Assert.Equal(1, resultBody!.InvitingUserId);
            }
        }

        [Fact]
        public async void GetMessageGroupInvitations_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.GetMessageGroupInvitations(9999);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetMessageGroupInvitations_ReturnsUnauthorizedWhenNotRequestingOwn()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.GetMessageGroupInvitations(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetMessageGroupInvitations_ReturnsEmptyListWhenNoInvitations()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("3");

                // Act
                var result = await controller.GetMessageGroupInvitations(3);
                var resultObject = result as OkObjectResult;
                var resultBody = resultObject!.Value as IEnumerable<MessageGroupInvitation>;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Empty(resultBody!);
            }
        }

        [Fact]
        public async void GetMessageGroupInvitations_ReturnsInvitationsWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetMessageGroupInvitations(1);
                var resultObject = result as OkObjectResult;
                var resultBody = resultObject!.Value as IEnumerable<MessageGroupInvitation>;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Equal(3, resultBody!.Count());
                Assert.Contains(resultBody!, rb => rb.Id == 1);
                Assert.Contains(resultBody!, rb => rb.Id == 2);
            }
        }

        [Fact]
        public async void GetMessageGroupInvitations_DoesNotReturnExpiredInvitations()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetMessageGroupInvitations(1);
                var resultObject = result as OkObjectResult;
                var resultBody = resultObject!.Value as IEnumerable<MessageGroupInvitation>;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.DoesNotContain(resultBody!, rb => rb.Id == 3);
            }
        }

        [Fact]
        public async void NewMessageGroupInvitation_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAnonymousIdentity();
                var invite = new MessageGroupInvitation();

                // Act
                var result = await controller.NewMessageGroupInvitation(invite);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void NewMessageGroupInvitation_ReturnsBadRequestWhenInviteIncomplete()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("4");
                controller.ModelState.AddModelError("MessageGroupId", "Required key missing.");
                var invite = new MessageGroupInvitation();

                // Act
                var result = await controller.NewMessageGroupInvitation(invite);

                // Assert
                Assert.IsType<BadRequestResult>(result);
            }
        }

        [Fact]
        public async void NewMessageGroupInvitation_ReturnsBadRequestWhenInviterIsNotMember()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("2");
                var invite = new MessageGroupInvitation()
                {
                    MessageGroupId = 3,
                    InvitedUserId = 4,
                    InvitingUserId = 2,
                    InvitationActive = true,
                    InvitationAccepted = false,
                    CreatedDate = DateTime.Now
                };

                // Act
                var result = await controller.NewMessageGroupInvitation(invite);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void NewMessageGroupInvitation_ReturnsBadRequestWhenUserIsAlreadyMember()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");
                var invite = new MessageGroupInvitation()
                {
                    MessageGroupId = 1,
                    InvitedUserId = 4,
                    InvitingUserId = 1,
                    InvitationActive = true,
                    InvitationAccepted = false,
                    CreatedDate = DateTime.Now
                };

                // Act
                var result = await controller.NewMessageGroupInvitation(invite);
                var resultObject = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Invited user is already a group member.", resultObject!.Value);
            }
        }

        [Fact]
        public async void NewMessageGroupInvitation_ReturnsBadRequestWhenInvitePropertiesInvalid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");
                var invite = new MessageGroupInvitation()
                {
                    MessageGroupId = 2,
                    InvitedUserId = 4,
                    InvitingUserId = 1,
                    InvitationActive = false,
                    InvitationAccepted = true,
                    CreatedDate = DateTime.Now
                };

                // Act
                var result = await controller.NewMessageGroupInvitation(invite);

                // Assert
                Assert.IsType<BadRequestResult>(result);
            }
        }

        [Fact]
        public async void NewMessageGroupInvitation_ReturnsBadRequestWhenSameInviteExists()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");
                var invite = new MessageGroupInvitation()
                {
                    MessageGroupId = 2,
                    InvitedUserId = 4,
                    InvitingUserId = 1,
                    InvitationActive = true,
                    InvitationAccepted = false,
                    CreatedDate = DateTime.Now
                };

                // Act
                var result = await controller.NewMessageGroupInvitation(invite);
                var resultObject = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Error when creating invitation.", resultObject!.Value);
            }
        }

        [Fact]
        public async void NewMessageGroupInvitation_ReturnsBadRequestWhenInviteeNotAcceptingInvites()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");
                var invite = new MessageGroupInvitation()
                {
                    MessageGroupId = 2,
                    InvitedUserId = 3,
                    InvitingUserId = 1,
                    InvitationActive = true,
                    InvitationAccepted = false,
                    CreatedDate = DateTime.Now
                };

                // Act
                var result = await controller.NewMessageGroupInvitation(invite);
                var resultObject = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("User not accepting invitations", resultObject!.Value);
            }
        }

        [Fact]
        public async void NewMessageGroupInvitation_ReturnsNoContentResultWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");
                var invite = new MessageGroupInvitation()
                {
                    MessageGroupId = 3,
                    InvitedUserId = 4,
                    InvitingUserId = 1,
                    InvitationActive = true,
                    InvitationAccepted = false,
                    CreatedDate = DateTime.Now
                };

                // Act
                var result = await controller.NewMessageGroupInvitation(invite);
                var verify = await controller.GetMessageGroupInvitations(1);
                var verifyObject = verify as OkObjectResult;
                var verifyBody = verifyObject!.Value as IEnumerable<MessageGroupInvitation>;
                var verifyInvite = verifyBody!.Where(inv => inv.MessageGroupId == 2 && inv.InvitedUserId == 4).FirstOrDefault();

                // Assert
                Assert.IsType<CreatedAtRouteResult>(result);
                Assert.NotNull(verifyInvite);
                Assert.Equal(2, verifyInvite.MessageGroupId);
                Assert.Equal(4, verifyInvite.InvitedUserId);
                Assert.Equal(1, verifyInvite.InvitingUserId);
                Assert.True(verifyInvite.InvitationActive);
            }
        }

        [Fact]
        public async void DeclineMessageGroupInvitation_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.DeclineMessageGroupInvitation(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void DeclineMessageGroupInvitation_ReturnsBadRequestWhenIdInvalid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.DeclineMessageGroupInvitation(0);
                var resultObject = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Invalid invite id.", resultObject!.Value);
            }
        }

        [Fact]
        public async void DeclineMessageGroupInvitation_ReturnsNotFoundWhenInviteNonExistent()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.DeclineMessageGroupInvitation(9999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void DeclineMessageGroupInvitation_ReturnsBadRequestInvitationUsed()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.DeclineMessageGroupInvitation(3);
                var resultObject = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Invitation already used.", resultObject!.Value);
            }
        }

        [Fact]
        public async void DeclineMessageGroupInvitation_ReturnsUnauthorizedWhenNotInvitee()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.DeclineMessageGroupInvitation(2);
                var resultObject = result as UnauthorizedObjectResult;

                // Assert
                Assert.IsType<UnauthorizedObjectResult>(result);
                Assert.Equal("Not invite recipient.", resultObject!.Value);
            }
        }

        [Fact]
        public async void DeclineMessageGroupInvitation_ReturnsNoContentResultWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("5");

                // Act
                var result = await controller.DeclineMessageGroupInvitation(4);
                var verify = await controller.GetMessageGroupInvitation(4);
                var verifyObject = verify as OkObjectResult;
                var verifyBody = verifyObject!.Value as MessageGroupInvitation;

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.Equal(4, verifyBody!.Id);
                Assert.False(verifyBody!.InvitationActive);
                Assert.False(verifyBody!.InvitationAccepted);
            }
        }

        [Fact]
        public async void AcceptMessageGroupInvitation_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.AcceptMessageGroupInvitation(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void AcceptMessageGroupInvitation_ReturnsBadRequestWhenIdInvalid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.AcceptMessageGroupInvitation(0);
                var resultObject = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Invalid invite id.", resultObject!.Value);
            }
        }

        [Fact]
        public async void AcceptMessageGroupInvitation_ReturnsNotFoundWhenInviteNonExistent()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.AcceptMessageGroupInvitation(9999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void AcceptMessageGroupInvitation_ReturnsBadRequestInvitationUsed()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.AcceptMessageGroupInvitation(3);
                var resultObject = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Invitation already used.", resultObject!.Value);
            }
        }

        [Fact]
        public async void AcceptMessageGroupInvitation_ReturnsUnauthorizedWhenNotInvitee()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.AcceptMessageGroupInvitation(2);
                var resultObject = result as UnauthorizedObjectResult;

                // Assert
                Assert.IsType<UnauthorizedObjectResult>(result);
                Assert.Equal("Not invite recipient.", resultObject!.Value);
            }
        }

        [Fact]
        public async void AcceptMessageGroupInvitation_ReturnsNoContentResultWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("6");

                // Act
                var result = await controller.AcceptMessageGroupInvitation(5);
                var verify = await controller.GetMessageGroupInvitation(5);
                var verifyObject = verify as OkObjectResult;
                var verifyBody = verifyObject!.Value as MessageGroupInvitation;
                var verifyMembership = context.MessageGroupMembers!
                    .Where(mgm => mgm.UserId == 6 && mgm.MessageGroupId == 1 && mgm.MessageGroupRoleId == 2)
                    .FirstOrDefault();

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.Equal(5, verifyBody!.Id);
                Assert.False(verifyBody!.InvitationActive);
                Assert.True(verifyBody!.InvitationAccepted);
                Assert.NotNull(verifyMembership);
            }
        }

        [Fact]
        public async void DeleteMessageGroupInvitation_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.DeleteMessageGroupInvitation(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void DeleteMessageGroupInvitation_ReturnsBadRequestWhenIdInvalid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.DeleteMessageGroupInvitation(0);
                var resultObject = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Invalid invite id.", resultObject!.Value);
            }
        }

        [Fact]
        public async void DeleteMessageGroupInvitation_ReturnsNotFoundWhenInviteNonExistent()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.DeleteMessageGroupInvitation(9999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void DeleteMessageGroupInvitation_ReturnsUnauthorizedWhenNotInviter()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("4");

                // Act
                var result = await controller.DeleteMessageGroupInvitation(2);
                var resultObject = result as UnauthorizedObjectResult;

                // Assert
                Assert.IsType<UnauthorizedObjectResult>(result);
                Assert.Equal("Not invite sender.", resultObject!.Value);
            }
        }

        [Fact]
        public async void DeleteMessageGroupInvitation_ReturnsNoContentResultWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupInvitationController(context, this.logger).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.DeleteMessageGroupInvitation(6);
                var verify = await controller.GetMessageGroupInvitation(6);

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.IsType<NotFoundResult>(verify);
            }
        }
    }
}
