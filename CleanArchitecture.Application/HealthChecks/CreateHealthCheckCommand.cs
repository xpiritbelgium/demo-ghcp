using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Domain.HealthChecks;
using MediatR;

namespace CleanArchitecture.Application.HealthChecks
{
    public class CreateHealthCheckCommand: IRequest<Guid>
    {
        public CreateHealthCheckCommand(string username)
        {
            UserName = username;
        }

        public string UserName { get; set; }
    }

    public class CreateHealthCheckCommandHandler : IRequestHandler<CreateHealthCheckCommand, Guid>
    {
        private readonly IApplicationUserAccessor _applicationUserAccessor;
        private readonly IAsyncRepository<HealthCheck> _repo;

        public CreateHealthCheckCommandHandler(IApplicationUserAccessor applicationUserAccessor, IAsyncRepository<HealthCheck> repo)
        {
            _applicationUserAccessor = applicationUserAccessor;
            _repo = repo;
        }

        public async Task<Guid> Handle(CreateHealthCheckCommand request, CancellationToken cancellationToken)
        {
            var applicationUser = await _applicationUserAccessor.GetUserAsync();

            var healthCheck = new HealthCheck(Guid.NewGuid(), applicationUser.UserName);

            await _repo.AddAsync(healthCheck);

            return healthCheck.Id;
        }
    }
}
