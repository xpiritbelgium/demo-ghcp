using CleanArchitecture.Application.Extensions;

namespace CleanArchitecture.UI.Web
{
    public class ApplicationUserAccessor : IApplicationUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationUserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Task<User> GetUserAsync()
        {
            var httpContextUser = _httpContextAccessor?.HttpContext?.User;

            var userName = httpContextUser?.Identity?.Name ?? "Unknown";
            var emailAddress = httpContextUser?.FindFirst("email")?.Value ?? string.Empty;
            var isAdmin = httpContextUser?.IsInRole("Admin") ?? false;

            var user = new User
            {
                UserName = userName,
                EmailAddress = emailAddress,
                IsAdmin = isAdmin
            };

            return Task.FromResult(user);
        }
    }
}
