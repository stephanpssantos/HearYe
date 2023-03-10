using Microsoft.Extensions.Logging;

namespace HearYe.Server.Tests
{
    public class PostControllerTests : IClassFixture<HearYeDatabaseFixture>
    {
        private readonly ILogger<PostController> logger;

        public PostControllerTests(HearYeDatabaseFixture fixture) 
        {
            Mock<ILogger<PostController>> mockLogger = new();
            this.logger = mockLogger.Object;
            Fixture = fixture;
        }

        public HearYeDatabaseFixture Fixture { get; }

        [Fact]
        public async void GetPost_ReturnsNotFoundWhenNonExistent()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.GetPost(9999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void GetPost_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.GetPost(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetPost_ReturnsUnauthorizedWhenUserNotInMessageGroup()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.GetPost(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetPost_ReturnsPostWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetPost(1);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as Shared.Post;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Equal(1, resultBody!.Id);
                Assert.Equal(1, resultBody!.MessageGroupId);
                Assert.Equal("This is test message 1. Wow.", resultBody!.Message);
            }
        }

        [Fact]
        public async void GetPostAcknowledgedUsers_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.GetPostAcknowledgedUsers(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetPostAcknowledgedUsers_ReturnsNotFoundWhenNonExistent()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetPostAcknowledgedUsers(9999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void GetPostAcknowledgedUsers_ReturnsUnauthorizedWhenNotGroupMember()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.GetPostAcknowledgedUsers(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetPostAcknowledgedUsers_ReturnsPublicUserInfoWithValidRequest()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetPostAcknowledgedUsers(1);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as IEnumerable<UserPublicInfo>;
                var resultSample = resultBody!.First();

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Single(resultBody!);
                Assert.IsType<UserPublicInfo>(resultSample);
                Assert.NotNull(resultSample.DisplayName);
            }
        }

        [Fact]
        public async void GetNewPosts_ReturnsUnauthorizedIfNotInGroup()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("3");

                // Act
                var result = await controller.GetNewPosts(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetNewPosts_ReturnsNotAcknowledgedNotStalePosts()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetNewPosts(1, 100);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as IEnumerable<Shared.PostWithUserName>;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                //Assert.Equal(2, resultBody!.Count());
                Assert.Contains(resultBody!, post => post.Id == 3);
                Assert.Contains(resultBody!, post => post.Id == 4);
            }
        }

        [Fact]
        public async void GetNewPosts_ReturnsAcknowledgementsWithResults()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetNewPosts(1, 100);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as IEnumerable<Shared.PostWithUserName>;
                var post1 = resultBody!.Where(p => p.Id == 4).FirstOrDefault();

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Single(post1!.Acknowledgements!);
                Assert.Equal(4, post1!.Acknowledgements!.FirstOrDefault()!.PostId);
                Assert.Equal(4, post1!.Acknowledgements!.FirstOrDefault()!.UserId);
            }
        }

        [Fact]
        public async void GetNewPosts_IncludesThePostersUserName()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetNewPosts(1, 100);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as IEnumerable<Shared.PostWithUserName>;
                var samplePost = resultBody!.Where(p => p.Id == 3).FirstOrDefault();

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.NotNull(samplePost);
                Assert.Equal("TestUser", samplePost.DisplayName);
            }
        }

        [Fact]
        public async void GetAcknowledgedPosts_ReturnsUnauthorizedIfNotInGroup()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("3");

                // Act
                var result = await controller.GetAcknowledgedPosts(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetAcknowledgedPosts_ReturnsAcknowledgedNotStalePosts()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetAcknowledgedPosts(1);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as IEnumerable<Shared.PostWithUserName>;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Single(resultBody!);
                Assert.Contains(resultBody!, post => post.Id == 1);
            }
        }

        [Fact]
        public async void GetAcknowledgedPosts_ReturnsAcknowledgementsWithResults()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetAcknowledgedPosts(1);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as IEnumerable<Shared.PostWithUserName>;
                var post1 = resultBody!.Where(p => p.Id == 1).FirstOrDefault();

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Single(post1!.Acknowledgements!);
                Assert.Equal(1, post1.Acknowledgements!.FirstOrDefault()!.PostId);
                Assert.Equal(1, post1.Acknowledgements!.FirstOrDefault()!.UserId);
            }
        }

        [Fact]
        public async void GetStalePosts_ReturnsUnauthorizedIfNotInGroup()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("3");

                // Act
                var result = await controller.GetStalePosts(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void GetStalePosts_ReturnsAllStalePosts()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetStalePosts(1);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as IEnumerable<Shared.PostWithUserName>;

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Single(resultBody!);
                Assert.Contains(resultBody!, post => post.Id == 5);
            }
        }

        [Fact]
        public async void GetStalePosts_ReturnsAcknowledgementsWithResults()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.GetStalePosts(1);
                var okResult = result as OkObjectResult;
                var resultBody = okResult!.Value as IEnumerable<Shared.PostWithUserName>;
                var post1 = resultBody!.Where(p => p.Id == 5).FirstOrDefault();

                // Assert
                Assert.IsType<OkObjectResult>(result);
                Assert.Single(post1!.Acknowledgements!);
                Assert.Equal(5, post1.Acknowledgements!.FirstOrDefault()!.PostId);
                Assert.Equal(4, post1.Acknowledgements!.FirstOrDefault()!.UserId);
            }
        }

        [Fact]
        public async void NewPost_ReturnsBadRequestWhenPostIsInvalid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAnonymousIdentity();
                controller.ModelState.AddModelError("MessageGroupId", "Required key missing.");
                var newPost = new Shared.Post();

                // Act
                var result = await controller.NewPost(newPost);
                var resultBody = result as BadRequestObjectResult;

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(resultBody!.Value, "Complete post object required.");
            }
        }

        [Fact]
        public async void NewPost_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAnonymousIdentity();
                var newPost = new Shared.Post()
                {
                    UserId = 2,
                    MessageGroupId = 1,
                    Message = "I'm not in this group!",
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                };

                // Act
                var result = await controller.NewPost(newPost);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void NewUser_ReturnsBadRequestWhenNotGroupMember()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("2");
                var newPost = new Shared.Post()
                {
                    UserId = 2,
                    MessageGroupId = 1,
                    Message = "I'm not in this group!",
                    IsDeleted = false,
                    CreatedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.NewPost(newPost);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void NewUser_ReturnsCreatedAtRouteResultWhenValid()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("1");
                var newPost = new Shared.Post()
                {
                    UserId = 1,
                    MessageGroupId = 1,
                    Message = "I'm making a valid post. Test test...",
                    IsDeleted = false,
                    CreatedDate = DateTimeOffset.Now
                };

                // Act
                var result = await controller.NewPost(newPost);

                // Assert
                Assert.IsType<CreatedAtRouteResult>(result);
            }
        }

        [Fact]
        public async void DeletePost_ReturnsNotFoundWhenRequestedPostDoesntExist()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("1");

                // Act
                var result = await controller.DeletePost(99);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async void DeletePost_ReturnsUnauthorizedWhenUnauthorized()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAnonymousIdentity();

                // Act
                var result = await controller.DeletePost(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async void DeletePost_ReturnsUnauthorizedWhenNotOwnerOfPost()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.DeletePost(1);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }


        [Fact]
        public async void DeletePost_ReturnsNoContentWhenPostDeleted()
        {
            // Arrange
            using (HearYeContext context = Fixture.CreateContext())
            {
                var controller = new PostController(context, this.logger).WithAuthenticatedIdentity("2");

                // Act
                var result = await controller.DeletePost(6);

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.Single(context.Posts!.Where(p => p.Id == 6)); // Record retained
                Assert.True(context.Posts!.Where(p => p.Id == 6).First().IsDeleted); // Soft delete applied
            }
        }
    }
}
