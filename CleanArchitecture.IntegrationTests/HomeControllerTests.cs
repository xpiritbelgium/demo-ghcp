using CleanArchitecture.UI.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace CleanArchitecture.IntegrationTests
{
    public class HomeControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public HomeControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HealthCheckShouldReturnSuccess()
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<ITestClaimsProvider, TestClaimsProvider>(provider => new TestClaimsProvider("Admin"));
                    services.AddAuthentication("Scheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Scheme", options => { });
                });
            }).CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Scheme");

            var response = await client.GetAsync("/Home/HealthCheck");

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task HealthCheckShouldFailWhenCalledWithoutAdminRole()
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<ITestClaimsProvider, TestClaimsProvider>(provider => new TestClaimsProvider("Reader"));
                    services.AddAuthentication("Scheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Scheme", options => { });
                });
            }).CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Scheme");

            var response = await client.GetAsync("/Home/HealthCheck");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}