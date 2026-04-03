using System.Data;
using System.Data.Common;
using CSharpFunctionalExtensions;
using FileService.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;
using Wolverine.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres
{
    public class TransactionManager : ITransactionManager
    {
        private readonly IDbContextOutbox<FileServiceDbContext> _outbox;
        private readonly ILogger<TransactionManager> _logger;
        private IDbContextTransaction? _currentTransaction;

        public TransactionManager(
            IDbContextOutbox<FileServiceDbContext> dbContextOutbox,
            ILogger<TransactionManager> logger)
        {
            _outbox = dbContextOutbox;
            _logger = logger;
        }

        public async Task<UnitResult<Error>> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            try
            {
                _currentTransaction = await _outbox.DbContext.Database.BeginTransactionAsync(cancellationToken);
                return UnitResult.Success<Error>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error beginning transaction.");
                return GeneralErrors.Failure("transaction.begin");
            }
        }

        public async Task<UnitResult<Error>> CommitTransactionAsync(CancellationToken cancellationToken)
        {
            if (_currentTransaction == null)
                return GeneralErrors.Failure("transaction.begin");


            try
            {
                await _outbox.DbContext.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
                await _outbox.FlushOutgoingMessagesAsync();
                return UnitResult.Success<Error>();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict while saving changes.");
                await RollbackAsync(cancellationToken);
                return GeneralErrors.Failure("concurrency conflict");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Operation canceled while saving changes.");
                await RollbackAsync(cancellationToken);
                return GeneralErrors.Failure("save.changes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing transaction.");
                await RollbackAsync(cancellationToken);
                return GeneralErrors.Failure("transaction.rollback");
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        private async Task RollbackAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_currentTransaction != null)
                    await _currentTransaction.RollbackAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rollback transaction.");
            }
        }

        public DbConnection GetDbConnection() => _outbox.DbContext.Database.GetDbConnection();

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