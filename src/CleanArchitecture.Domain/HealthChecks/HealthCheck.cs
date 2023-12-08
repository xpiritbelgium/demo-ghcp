namespace CleanArchitecture.Domain.HealthChecks
{
    public class HealthCheck
    {
        protected HealthCheck()
        {
            
        }
        public HealthCheck(Guid? id, string userName)
        {
            Id = id ?? Guid.NewGuid();
            UserName = userName;
            DateStamp = DateTime.UtcNow;
        }

        public Guid Id { get; set; }

        public string UserName { get; set; }

        public DateTime DateStamp { get; set; }
    }
}
