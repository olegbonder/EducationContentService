using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace EducationContentService.Core.Database
{
    public interface ITransactionManager
    {
        Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
