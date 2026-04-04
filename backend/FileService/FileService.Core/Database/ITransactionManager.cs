using System.Data.Common;
using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Core.Database
{
    public interface ITransactionManager
    {
        Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);

        Task<UnitResult<Error>> BeginTransactionAsync(CancellationToken cancellationToken);
        
        Task<UnitResult<Error>>  CommitTransactionAsync(CancellationToken cancellationToken);
        
        DbConnection GetDbConnection();
    }
}