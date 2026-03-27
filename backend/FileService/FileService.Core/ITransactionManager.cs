using System.Data;
using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Core
{
    public interface ITransactionManager
    {
        public Task<Result<int, Error>> SaveChangesAsync(CancellationToken cancellationToken);

        public Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    }
}