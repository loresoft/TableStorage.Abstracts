using System.Linq.Expressions;

using Azure.Data.Tables;

namespace TableStorage.Abstracts;

/// <summary>
/// Provides an interface for common Azure Table storage data operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity that implements <see cref="ITableEntity"/>.</typeparam>
public interface ITableRepository<TEntity>
    where TEntity : class, ITableEntity
{
    /// <summary>
    /// Creates a new row key value.
    /// </summary>
    /// <returns>A new row key value.</returns>
    string NewRowKey();

    /// <summary>
    /// Gets the Azure Table storage <see cref="TableClient"/> to use for data operations.
    /// </summary>
    /// <returns>A <see cref="TableClient"/> instance.</returns>
    Task<TableClient> GetClientAsync();

    /// <summary>
    /// Finds an entity with the specified row key and partition key.
    /// </summary>
    /// <param name="rowKey">The entity row key identifier to find.</param>
    /// <param name="partitionKey">The entity partition key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An instance of <typeparamref name="TEntity"/> that has the specified identifier if found; otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="rowKey"/> or <paramref name="partitionKey"/> is <see langword="null" />.
    /// </exception>
    Task<TEntity?> FindAsync(
        string rowKey,
        string partitionKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds all entities that match the specified filter expression.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A read-only collection of <typeparamref name="TEntity"/> instances that match the filter.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null" />.</exception>
    Task<IReadOnlyCollection<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds all entities that match the specified OData filter query.
    /// </summary>
    /// <param name="filter">The OData filter query. If <see langword="null"/> or empty, all entities are returned.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A read-only collection of <typeparamref name="TEntity"/> instances that match the filter.
    /// </returns>
    Task<IReadOnlyCollection<TEntity>> FindAllAsync(
        string? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a page of entities that match the specified filter expression.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <param name="continuationToken">A continuation token indicating where to resume paging, or <see langword="null" /> to begin paging from the beginning.</param>
    /// <param name="pageSize">The maximum number of items that should be returned in a single page. The actual number returned may be less.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="PagedResult{TEntity}"/> containing <typeparamref name="TEntity" /> instances that match the filter.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="filter" /> is <see langword="null" />.</exception>
    Task<PagedResult<TEntity>> FindPageAsync(
        Expression<Func<TEntity, bool>> filter,
        string? continuationToken = default,
        int? pageSize = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a page of entities that match the specified OData filter query.
    /// </summary>
    /// <param name="filter">The OData filter query. If <see langword="null"/> or empty, all entities are considered.</param>
    /// <param name="continuationToken">A continuation token indicating where to resume paging, or <see langword="null" /> to begin paging from the beginning.</param>
    /// <param name="pageSize">The maximum number of items that should be returned in a single page. The actual number returned may be less.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="PagedResult{TEntity}"/> containing <typeparamref name="TEntity" /> instances that match the filter.
    /// </returns>
    Task<PagedResult<TEntity>> FindPageAsync(
        string? filter = null,
        string? continuationToken = default,
        int? pageSize = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the first entity that matches the specified filter expression.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An instance of <typeparamref name="TEntity"/> that matches the filter if found; otherwise, <see langword="null" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null" />.</exception>
    Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the first entity that matches the specified OData filter query.
    /// </summary>
    /// <param name="filter">The OData filter query. If <see langword="null"/> or empty, the first entity is returned.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An instance of <typeparamref name="TEntity"/> that matches the filter if found; otherwise, <see langword="null" />.
    /// </returns>
    Task<TEntity?> FindOneAsync(
        string? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the specified entity in the underlying data store by inserting if it doesn't exist, or updating if it does (upsert operation).
    /// </summary>
    /// <param name="entity">The entity to be saved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The <typeparamref name="TEntity"/> that was saved.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null" />.</exception>
    Task<TEntity> SaveAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts the specified entity into the underlying data store.
    /// </summary>
    /// <param name="entity">The entity to be inserted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The <typeparamref name="TEntity"/> that was inserted.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null" />.</exception>
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the specified entity in the underlying data store.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The <typeparamref name="TEntity"/> that was updated.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null" />.</exception>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits the specified entities as a batch transaction to the service for execution.
    /// </summary>
    /// <param name="entities">The entities to process in the batch transaction.</param>
    /// <param name="transactionType">The type of batch transaction to perform on all entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of entities processed in the batch transaction.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entities"/> is <see langword="null" />.</exception>
    Task<int> BatchAsync(
        IEnumerable<TEntity> entities,
        TableTransactionActionType transactionType = TableTransactionActionType.Add,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified entity from the underlying data store.
    /// </summary>
    /// <param name="entity">The entity to be deleted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null" />.</exception>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity with the specified row key and partition key from the underlying data store.
    /// </summary>
    /// <param name="rowKey">The entity row key identifier.</param>
    /// <param name="partitionKey">The partition key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="rowKey"/> or <paramref name="partitionKey"/> is <see langword="null" />.
    /// </exception>
    Task DeleteAsync(
        string rowKey,
        string partitionKey,
        CancellationToken cancellationToken = default);
}
