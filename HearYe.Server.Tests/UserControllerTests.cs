// See: https://stackoverflow.com/questions/69659645/c-sharp-mock-unit-test-graphserviceclient
// And: https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database
// And: https://gunnarpeipman.com/aspnet-core-test-controller-fake-user/

using Microsoft.Graph;

namespace HearYe.Server.Tests
{
    public class UserControllerTests : IClassFixture<HearYeDatabaseFixture>
    {
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly GraphServiceClient _graphServiceClient;

        public UserControllerTests(HearYeDatabaseFixture fixture)
        {
            Mock<IAuthenticationProvider> mockAuthProvider = new();
            Mock<IHttpProvider> mockHttpProvider = new();
            _authenticationProvider = mockAuthProvider.Object;
            _httpProvider = mockHttpProvider.Object;

            Mock<GraphServiceClient> mockGraphClient = new(_authenticationProvider, _httpProvider);
            //mockGraphClient.Setup(g => g...)
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
                var controller = new UserController(context, _graphServiceClient).WithAnonymousIdentity();

                // Act
                var result = await controller.GetUser(1);

                // Assert
                var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
            }
        }
    }
}