using CleanArchitecture.UI.Web;
using CleanArchitecture.UI.Web.Models;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;
using Xunit;

namespace CleanArchitecture.IntegrationTests.Documents
{
	public class DocumentControllerTests : IClassFixture<TestingWebAppFactory<Program>>
	{
		private readonly TestingWebAppFactory<Program> _factory;

		public DocumentControllerTests(TestingWebAppFactory<Program> factory)
		{
			_factory = factory;
		}

		[Fact]
		public async Task Index_WhenCalled_ShouldReturnSuccess()
		{
			var client = _factory.GetClientWithClaim("Reader");
			
			var response = await client.GetAsync("/Documents/Index");

			response.EnsureSuccessStatusCode();
		}

		[Fact]
		public async Task Create_WhenCalled_ShouldReturnSuccess()
		{
			var client = _factory.GetClientWithClaim("Document.Creator");
			
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
	}
}