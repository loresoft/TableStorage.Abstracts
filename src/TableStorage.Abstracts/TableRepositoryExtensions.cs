using System.Linq.Expressions;

using Azure.Data.Tables;

namespace TableStorage.Abstracts;

/// <summary>
/// Provides extension methods for <see cref="ITableRepository{TEntity}"/>.
/// </summary>
public static class TableRepositoryExtensions
{
    /// <summary>
    /// Creates multiple entities in the table using a batch transaction.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity that implements <see cref="ITableEntity"/>.</typeparam>
    /// <param name="repository">The table repository instance.</param>
    /// <param name="entities">The entities to be created.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of entities created in the batch transaction.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method uses the <see cref="TableTransactionActionType.Add"/> operation type.
    /// </remarks>
    public static Task<int> CreateBatchAsync<TEntity>(
        this ITableRepository<TEntity> repository,
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        return repository.BatchAsync(
            entities,
            TableTransactionActionType.Add,
            cancellationToken);
    }

    /// <summary>
    /// Updates multiple entities in the table using a batch transaction with replace operation.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity that implements <see cref="ITableEntity"/>.</typeparam>
    /// <param name="repository">The table repository instance.</param>
    /// <param name="entities">The entities to be updated.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of entities updated in the batch transaction.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method uses the <see cref="TableTransactionActionType.UpdateReplace"/> operation type.
    /// </remarks>
    public static Task<int> UpdateBatchAsync<TEntity>(
        this ITableRepository<TEntity> repository,
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        return repository.BatchAsync(
            entities,
            TableTransactionActionType.UpdateReplace,
            cancellationToken);
    }

    /// <summary>
    /// Saves multiple entities in the table using a batch transaction with upsert operation (insert if not exists, update if exists).
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity that implements <see cref="ITableEntity"/>.</typeparam>
    /// <param name="repository">The table repository instance.</param>
    /// <param name="entities">The entities to be saved.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of entities saved in the batch transaction.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method uses the <see cref="TableTransactionActionType.UpsertReplace"/> operation type.
    /// </remarks>
    public static Task<int> SaveBatchAsync<TEntity>(
        this ITableRepository<TEntity> repository,
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        return repository.BatchAsync(
            entities,
            TableTransactionActionType.UpsertReplace,
            cancellationToken);
    }

    /// <summary>
    /// Deletes multiple entities from the table using a batch transaction.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity that implements <see cref="ITableEntity"/>.</typeparam>
    /// <param name="repository">The table repository instance.</param>
    /// <param name="entities">The entities to be deleted.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of entities deleted in the batch transaction.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method uses the <see cref="TableTransactionActionType.Delete"/> operation type.
    /// </remarks>
    public static Task<int> DeleteBatchAsync<TEntity>(
        this ITableRepository<TEntity> repository,
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        return repository.BatchAsync(
            entities,
            TableTransactionActionType.Delete,
            cancellationToken);

    }

    /// <summary>
    /// Deletes all entities that match the specified filter expression using batch operations with pagination to limit memory usage.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity that implements <see cref="ITableEntity"/>.</typeparam>
    /// <param name="repository">The table repository instance.</param>
    /// <param name="filter">The filter expression to identify entities to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The total number of entities deleted across all batch transactions.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="repository"/> or <paramref name="filter"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method processes entities in pages to limit memory usage and handles continuation tokens automatically.
    /// Each page of entities is deleted using batch operations for better performance.
    /// </remarks>
    public static async Task<int> DeleteBatchAsync<TEntity>(
        this ITableRepository<TEntity> repository,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        var totalDeleted = 0;
        string? continuationToken = null;

        // Process entities in pages to limit memory usage
        do
        {
            var page = await repository
                .FindPageAsync(filter, continuationToken, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (page.Data == null || page.Data.Count == 0)
                break;

            // Delete the current page of entities using batch operation
            var deletedCount = await repository
                .BatchAsync(
                    entities: page.Data,
                    transactionType: TableTransactionActionType.Delete,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);

            totalDeleted += deletedCount;
            continuationToken = page.ContinuationToken;

        } while (!string.IsNullOrEmpty(continuationToken));

        return totalDeleted;
    }

    /// <summary>
    /// Deletes all entities that match the specified OData filter query using batch operations with pagination to limit memory usage.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity that implements <see cref="ITableEntity"/>.</typeparam>
    /// <param name="repository">The table repository instance.</param>
    /// <param name="filter">The OData filter query to identify entities to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The total number of entities deleted across all batch transactions.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="repository"/> or <paramref name="filter"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method processes entities in pages to limit memory usage and handles continuation tokens automatically.
    /// Each page of entities is deleted using batch operations for better performance.
    /// </remarks>
    public static async Task<int> DeleteBatchAsync<TEntity>(
        this ITableRepository<TEntity> repository,
        string filter,
        CancellationToken cancellationToken = default)
        where TEntity : class, ITableEntity
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        var totalDeleted = 0;
        string? continuationToken = null;

        // Process entities in pages to limit memory usage
        do
        {
            var page = await repository
                .FindPageAsync(filter, continuationToken, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (page.Data == null || page.Data.Count == 0)
                break;

            // Delete the current page of entities using batch operation
            var deletedCount = await repository
                .BatchAsync(
                    entities: page.Data,
                    transactionType: TableTransactionActionType.Delete,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);

            totalDeleted += deletedCount;
            continuationToken = page.ContinuationToken;

        } while (!string.IsNullOrEmpty(continuationToken));

        return totalDeleted;
    }
}
