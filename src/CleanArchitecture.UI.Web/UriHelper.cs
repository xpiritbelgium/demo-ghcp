using CleanArchitecture.Application.Extensions;

namespace CleanArchitecture.UI.Web
{
    public class UriHelper : IUriHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        public UriHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<LinkGenerator>();
        }

        public string GetDocumentDetailsUri(Guid id)
        {
            return _linkGenerator.GetUriByAction(_httpContextAccessor?.HttpContext, "Details", "Documents", new { id });
        }
    }
}
