using CleanArchitecture.Persistence;
using CleanArchitecture.UI.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Testcontainers.Azurite;
using Testcontainers.MsSql;
using Xunit;

namespace CleanArchitecture.IntegrationTests
{
	public class TestingWebAppFactory<TEntryPoint> : WebApplicationFactory<Program>, IAsyncLifetime where TEntryPoint : Program
	{
		private readonly MsSqlContainer _msSqlContainer;
		private readonly AzuriteContainer _azuriteContainer;

		public TestingWebAppFactory()
		{
			_msSqlContainer = new MsSqlBuilder().Build();
			_azuriteContainer = new AzuriteBuilder()
				.WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
				.Build();
		}

		// TODO: Can be removed if we don't check database or blob storage
		public MsSqlContainer MsSqlContainer => _msSqlContainer;
		public AzuriteContainer AzuriteContainer => _azuriteContainer;

		public HttpClient GetClientWithClaim(string claim)
		{
			var client = WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddScoped<ITestClaimsProvider, TestClaimsProvider>(provider => new TestClaimsProvider(claim));
				});
			}).CreateClient(new WebApplicationFactoryClientOptions
			{
				AllowAutoRedirect = false,
			});
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Scheme");
			return client;
		}

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
				services.AddAuthentication("Scheme")
					.AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
						"Scheme", options => { });

				services.AddAzureClients(cfg =>
					cfg.AddBlobServiceClient(_azuriteContainer.GetConnectionString()));

				var descriptor = services.SingleOrDefault(
					d => d.ServiceType == typeof(DbContextOptions<CleanArchitectureDbContext>));
				if (descriptor != null)
					services.Remove(descriptor);
				services.AddDbContext<CleanArchitectureDbContext>(options =>
				{
					options.UseSqlServer(_msSqlContainer.GetConnectionString());
				});
				var sp = services.BuildServiceProvider();
				using var scope = sp.CreateScope();
				using var appContext = scope.ServiceProvider.GetRequiredService<CleanArchitectureDbContext>();
				appContext.Database.EnsureCreated();
			});
		}

		public async Task InitializeAsync()
		{
			await _msSqlContainer.StartAsync();
			await _azuriteContainer.StartAsync();
		}

		public new async Task DisposeAsync()
		{
			await _msSqlContainer.StopAsync();
			await _azuriteContainer.StopAsync();
		}
	}
}