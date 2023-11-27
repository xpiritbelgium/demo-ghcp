namespace CleanArchitecture.Application.Extensions
{
    public interface IApplicationUserAccessor
    {
        Task<User> GetUserAsync();
    }

    public class User
    {
        public string UserName { get; set; }

        public string EmailAddress { get; set; }

        public bool IsAdmin { get; set; }
    }
}
