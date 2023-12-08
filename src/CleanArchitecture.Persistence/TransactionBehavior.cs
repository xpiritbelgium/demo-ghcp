using MediatR;

namespace CleanArchitecture.Persistence
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<Unit>
    {
        private readonly CleanArchitectureDbContext _dbContext;

        public TransactionBehavior(CleanArchitectureDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {               
                var response = await next();

                await tx.CommitAsync(cancellationToken);

                return response;
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);

                throw;
            }
        }
    }
}
