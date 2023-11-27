using CleanArchitecture.Application.Extensions;

namespace CleanArchitecture.WebJob
{
    public class ApplicationUserAccessor : IApplicationUserAccessor
    {

        public Task<User> GetUserAsync()
        {
            var user = new User
            {
                UserName = "web job",
                EmailAddress = string.Empty,
                IsAdmin = false
            };

            return Task.FromResult(user);
        }
    }
}
