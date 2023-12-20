using CleanArchitecture.Persistence;
using CleanArchitecture.UI.Web;
using CleanArchitecture.UI.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
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
	public class DocumentControllerTests : IClassFixture<TestingWebAppFactory<Program>> 
	{
		private readonly HttpClient _client;
		private readonly TestingWebAppFactory<Program> _appFactory;

		public DocumentControllerTests(TestingWebAppFactory<Program> factory)
		{
			_appFactory = factory;
			_client = factory.CreateClient(new WebApplicationFactoryClientOptions
			{
				AllowAutoRedirect = false,
			});
		}

		[Fact]
		public async Task Index_WhenCalled_ShouldReturnSuccess()
		{


			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Scheme");

			var response = await _client.GetAsync("/Documents/Index");

			response.EnsureSuccessStatusCode();
		}

		[Fact]
		public async Task Create_WhenCalled_ShouldReturnSuccess()
		{
			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Scheme");

			var model = new CreateDocumentViewModel
			{
				Title = "Test Document"
			};
			var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 20, "postedFile", "test.txt");

			var content = new MultipartFormDataContent();
			content.Add(new StringContent(model.Title), "Title");
			content.Add(new StreamContent(file.OpenReadStream()), "postedFile", file.FileName);

			var response = await _client.PostAsync("/Documents/Create", content);

			Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
		}



	}





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

		public MsSqlContainer MsSqlContainer => _msSqlContainer;
		public AzuriteContainer AzuriteContainer => _azuriteContainer;

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
				services.AddScoped<ITestClaimsProvider, TestClaimsProvider>(provider => new TestClaimsProvider("Document.Creator"));
				services.AddAuthentication("Scheme")
					.AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
						"Scheme", options => { });

				services.AddAzureClients(cfg =>
				{
					cfg.AddBlobServiceClient(_azuriteContainer.GetConnectionString());
				});

				var descriptor = services.SingleOrDefault(
					d => d.ServiceType ==
						typeof(DbContextOptions<CleanArchitectureDbContext>));
				if (descriptor != null)
					services.Remove(descriptor);
				services.AddDbContext<CleanArchitectureDbContext>(options =>
				{
					options.UseSqlServer(_msSqlContainer.GetConnectionString());
				});
				var sp = services.BuildServiceProvider();
				using (var scope = sp.CreateScope())
				using (var appContext = scope.ServiceProvider.GetRequiredService<CleanArchitectureDbContext>())
				{
					try
					{
						appContext.Database.EnsureCreated();
					}
					catch (Exception ex)
					{
						//Log errors or do anything you think it's needed
						throw;
					}
				}
			});
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