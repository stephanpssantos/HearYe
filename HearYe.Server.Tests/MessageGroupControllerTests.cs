using HearYe.Shared;
using System.Text;

namespace HearYe.Server.Tests
{
    public class MessageGroupControllerTests : IClassFixture<HearYeDatabaseFixture>
    {
        public MessageGroupControllerTests(HearYeDatabaseFixture fixture) 
        {
            Fixture = fixture;
        }

        public HearYeDatabaseFixture Fixture { get; }

        [Fact]
        public async void GetMessageGroup_ReturnsNotFoundWhenNonExistent()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAnonymousIdentity();

                // Act
                var result = await controller.GetMessageGroup(9999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void GetMessageGroup_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAnonymousIdentity();

                // Act
                var result = await controller.GetMessageGroup(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetMessageGroup_ReturnsUnauthorizedWhenNotGroupMember()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.GetMessageGroup(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetMessageGroup_ReturnsMessageGroupWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetMessageGroup(1);
                var resultBody = result as OkObjectResult;
                var mg = resultBody!.Value as MessageGroup;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.NotNull(mg);
                Assert.Equal("TestMessageGroup1", mg.MessageGroupName);
            }
        }

        [Fact]
        public async void GetMessageGroupMembers_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAnonymousIdentity();

                // Act
                var result = await controller.GetMessageGroupMembers(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetMessageGroupMembers_ReturnsUnauthorizedWhenNotGroupMember()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.GetMessageGroupMembers(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetMessageGroupMembers_ReturnsMemberListWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetMessageGroupMembers(1);
                var resultBody = result as OkObjectResult;
                var mg = resultBody!.Value as IEnumerable<MessageGroupMember>;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.NotEmpty(mg!);
                Assert.Contains<MessageGroupMember>(mg!, mgm => mgm.UserId == 1);
                Assert.DoesNotContain<MessageGroupMember>(mg!, mgm => mgm.UserId == 2);
            }
        }

        [Fact]
        public async void NewMessageGroupMembers_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAnonymousIdentity();

                // Act
                var result = await controller.NewMessageGroup("UnitTestGroup1");

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void NewMessageGroupMembers_ReturnsBadRequestWhenGroupNameIsEmpty()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.NewMessageGroup("");

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        [Fact]
        public async void NewMessageGroupMembers_ReturnsBadRequestWhenGroupNameIsTooShort()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.NewMessageGroup("a");

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        [Fact]
        public async void NewMessageGroupMembers_ReturnsBadRequestWhenGroupNameIsTooLong()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.NewMessageGroup(new string('a', 51));

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        [Fact]
        public async void NewMessageGroupMembers_CreatesNewGroupWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.NewMessageGroup("UnitTestGroup") as CreatedAtRouteResult;
                var mg = result!.Value as MessageGroup;
                var mgVerify = await controller.GetMessageGroup(mg!.Id) as OkObjectResult;
                var mgBody = mgVerify!.Value as MessageGroup;
                var mgmVerify = await controller.GetMessageGroupMembers(mg!.Id) as OkObjectResult;
                var mgmBody = mgmVerify!.Value as IEnumerable<MessageGroupMember>;

                // Assert
                Assert.IsType<CreatedAtRouteResult>(result);
                Assert.IsType<OkObjectResult>(mgVerify);
                Assert.Equal("UnitTestGroup", mgBody!.MessageGroupName);
                Assert.Single(mgmBody!);
                Assert.Contains(mgmBody!, mgm => mgm.UserId == 1);
                Assert.Equal(1, mgmBody!.First().MessageGroupRoleId!);
            }
        }

        [Fact]
        public async void SetMessageGroupRole_ReturnsBadRequestWhenObjectInvalid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAnonymousIdentity();
                controller.ModelState.AddModelError("MessageGroupId", "Required key missing.");
                var mgm = new MessageGroupMember();

                // Act
                var result = await controller.SetMessageGroupRole(mgm);

                // Assert
                Assert.IsType<BadRequestResult>(result);
            }
        }

        [Fact]
        public async void SetMessageGroupRole_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAnonymousIdentity();
                var mgm = new MessageGroupMember();

                // Act
                var result = await controller.SetMessageGroupRole(mgm);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void SetMessageGroupRole_ReturnsUnauthorizedWhenNotGroupMember()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("2");
                var mgm = new MessageGroupMember();
                mgm.MessageGroupId = 1;

                // Act
                var result = await controller.SetMessageGroupRole(mgm);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void SetMessageGroupRole_ReturnsUnauthorizedWhenNotGroupAdmin()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("2");
                var mgm = new MessageGroupMember();
                mgm.Id = 3;
                mgm.MessageGroupId = 2;
                mgm.MessageGroupRoleId = 1;
                mgm.UserId = 2;

                // Act
                var result = await controller.SetMessageGroupRole(mgm);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void SetMessageGroupRole_ReturnsBadRequestWhenNotAlreadyMember()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("1");
                var mgm = new MessageGroupMember();
                mgm.MessageGroupId = 1;
                mgm.MessageGroupRoleId = 1;
                mgm.UserId = 2;

                // Act
                var result = await controller.SetMessageGroupRole(mgm);
                var resultObject = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Specified user is not group member.", resultObject!.Value);
            }
        }

        [Fact]
        public async void SetMessageGroupRole_NoContentResultWhenRequestIsValid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("1");
                var mgm = new MessageGroupMember();
                mgm.Id = 4;
                mgm.MessageGroupId = 1;
                mgm.MessageGroupRoleId = 1;
                mgm.UserId = 4;

                // Act
                var result = await controller.SetMessageGroupRole(mgm);
                var verifymgm = await controller.GetMessageGroupMembers(1) as OkObjectResult;
                var verifymgmbody = verifymgm!.Value as IEnumerable<MessageGroupMember>;
                var verifymgmuser = verifymgmbody!.Where(mgm => mgm.Id == 4).First();

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.Equal(1, verifymgmuser.MessageGroupRoleId);
            }
        }

        [Fact]
        public async void DeleteMessageGroup_ReturnsNotFoundWhenNonExistent()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAnonymousIdentity();

                // Act
                var result = await controller.DeleteMessageGroup(9999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void DeleteMessageGroup_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAnonymousIdentity();

                // Act
                var result = await controller.DeleteMessageGroup(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void DeleteMessageGroup_ReturnsUnauthorizedWhenNotAdmin()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.DeleteMessageGroup(2);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void DeleteMessageGroup_NoContentResultWhenRequestIsValid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new MessageGroupController(context).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.DeleteMessageGroup(3);
                var verifymg = await controller.GetMessageGroup(3) as OkObjectResult;
                var verifymgbody = verifymg!.Value as MessageGroup;

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.True(verifymgbody!.IsDeleted);
            }
        }
    }
}
