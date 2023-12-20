using CleanArchitecture.Persistence;
using CleanArchitecture.UI.Web;
using CleanArchitecture.UI.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Testcontainers.Azurite;
using Testcontainers.MsSql;
using Xunit;

namespace CleanArchitecture.IntegrationTests.Documents
{
    public class DocumentControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly MsSqlContainer _msSqlContainer;
        private readonly AzuriteContainer _azuriteContainer;

        public DocumentControllerTests()
        {
            _msSqlContainer = new MsSqlBuilder().Build();
            _azuriteContainer = new AzuriteBuilder()
                .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
                .Build();

            _factory = new WebApplicationFactory<Program>();
        }

        [Fact]
        public async Task Index_WhenCalled_ShouldReturnSuccess()
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<ITestClaimsProvider, TestClaimsProvider>(provider => new TestClaimsProvider("Reader"));
                    services.AddAuthentication("Scheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Scheme", options => { });
                    services.Remove(services.SingleOrDefault(service => service.ServiceType == typeof(CleanArchitectureDbContext)));
                    services.AddDbContext<CleanArchitectureDbContext>(options => options.UseSqlServer(_msSqlContainer.GetConnectionString()));
                });
            }).CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

            _factory.Services.CreateScope().ServiceProvider.GetRequiredService<CleanArchitectureDbContext>().Database.Migrate();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Scheme");

            var response = await client.GetAsync("/Documents/Index");

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Create_WhenCalled_ShouldReturnSuccess()
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<ITestClaimsProvider, TestClaimsProvider>(provider => new TestClaimsProvider("Document.Creator"));
                    services.AddAuthentication("Scheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Scheme", options => { });
                    services.Remove(services.SingleOrDefault(service => service.ServiceType == typeof(CleanArchitectureDbContext)));
                    services.AddDbContext<CleanArchitectureDbContext>(options => options.UseSqlServer(_msSqlContainer.GetConnectionString()));
                    services.AddAzureClients(cfg =>
                    {
                        cfg.AddBlobServiceClient(_azuriteContainer.GetConnectionString());
                    });
                });
            }).CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

            _factory.Services.CreateScope().ServiceProvider.GetRequiredService<CleanArchitectureDbContext>().Database.Migrate();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Scheme");

            var model = new CreateDocumentViewModel
            {
                Title = "Test Document"
            };
            var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 20, "postedFile", "test.txt");

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Title), "Title");
            content.Add(new StreamContent(file.OpenReadStream()), "postedFile", file.FileName);

            var response = await client.PostAsync("/Documents/Create", content);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        public async Task InitializeAsync()
        {
            await _msSqlContainer.StartAsync();
            await _azuriteContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _msSqlContainer.StopAsync();
            await _azuriteContainer.StopAsync();
        }

    }


}
