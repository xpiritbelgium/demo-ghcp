using CleanArchitecture.UI.Web;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace CleanArchitecture.IntegrationTests
{
    public class HomeControllerTests : IClassFixture<TestingWebAppFactory<Program>>
    {
		private readonly TestingWebAppFactory<Program> _factory;

		public HomeControllerTests(TestingWebAppFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HealthCheckShouldReturnSuccess()
        {
            var client = _factory.GetClientWithClaim("Admin");

            var response = await client.GetAsync("/Home/HealthCheck");

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task HealthCheckShouldFailWhenCalledWithoutAdminRole()
        {
			var client = _factory.GetClientWithClaim("Reader");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Scheme");

            var response = await client.GetAsync("/Home/HealthCheck");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}