using CleanArchitecture.Application.Extensions;

namespace CleanArchitecture.WebAPI
{
    public class ApplicationUserAccessor : IApplicationUserAccessor
    {
                
        public Task<User> GetUserAsync()
        {
            var user = new User
            {
                UserName = "web api",
                EmailAddress = string.Empty,
                IsAdmin = false
            };

            return Task.FromResult(user);
        }
    }
}
