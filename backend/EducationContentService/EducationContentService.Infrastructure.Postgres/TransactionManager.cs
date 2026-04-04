using CSharpFunctionalExtensions;
using EducationContentService.Core.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;
using Wolverine.EntityFrameworkCore;

namespace EducationContentService.Infrastructure.Postgres
{
    public class TransactionManager : ITransactionManager
    {
        private readonly IDbContextOutbox<EducationDbContext> _outbox;
        private readonly ILogger<TransactionManager> _logger;
        private IDbContextTransaction? _currentTransaction;

        public TransactionManager(
            IDbContextOutbox<EducationDbContext> dbContextOutbox,
            ILogger<TransactionManager> logger)
        {
            _outbox = dbContextOutbox;
            _logger = logger;
        }
        public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _outbox.DbContext.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    await _outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
                }

                return UnitResult.Success<Error>();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict while saving changes.");
                return Error.Conflict("concurrency.error", "Concurrency conflict while saving changes.");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Operation canceled while saving changes.");
                return GeneralErrors.Failure("save.changes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database.");
                return GeneralErrors.Failure("save.changes");
            }
        }
    }
}
