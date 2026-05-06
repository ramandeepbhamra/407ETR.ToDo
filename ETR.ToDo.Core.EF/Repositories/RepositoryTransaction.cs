using ETR.ToDo.Core.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ETR.ToDo.Core.EF.Repositories;

/// <summary>
/// Wrapper around EF Core's IDbContextTransaction
/// Provides abstraction for repository transaction interface
/// </summary>
internal class RepositoryTransaction : IRepositoryTransaction
{
    private readonly IDbContextTransaction _transaction;

    public RepositoryTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
    }
}
