using System.Linq.Expressions;

using Azure.Data.Tables;

namespace TableStorage.Abstracts;

/// <summary>
/// An <c>interface</c> for common Azure Table storage data operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface ITableRepository<TEntity>
    where TEntity : class, ITableEntity
{
    /// <summary>
    /// Creates new rowkey.
    /// </summary>
    /// <returns>A new rowkey value</returns>
    string NewRowKey();

    /// <summary>
    /// Gets the Azure Table storage <see cref="TableClient"/> to use for data operations.
    /// </summary>
    /// <returns>A <see cref="TableClient"/> instance.</returns>
    Task<TableClient> GetClientAsync();

    /// <summary>
    /// Find an entity with the specified <paramref name="rowKey" /> and <paramref name="partitionKey" />.
    /// </summary>
    /// <param name="rowKey">The entity rowkey identifier to find.</param>
    /// <param name="partitionKey">The entity partition key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An instance of <typeparamref name="TEntity"/> that has the specified identifier if found, otherwise null.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// <paramref name="rowKey"/> or <paramref name="partitionKey"/> is <see langword="null" />.
    /// </exception>
    Task<TEntity?> FindAsync(
        string rowKey,
        string partitionKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find all entities using the specified <paramref name="filter"/> expression.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A list of <typeparamref name="TEntity"/> instances that matches the filter if found, otherwise <see langword="null" />.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="filter"/> is <see langword="null" /></exception>
    Task<IReadOnlyCollection<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find all entities using the specified <paramref name="filter"/> expression.
    /// </summary>
    /// <param name="filter">The OData filter query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A list of <typeparamref name="TEntity"/> instances that matches the filter if found, otherwise <see langword="null" />.
    /// </returns>
    Task<IReadOnlyCollection<TEntity>> FindAllAsync(
        string? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find a page of entities using the specified <paramref name="filter" /> expression.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <param name="continuationToken">A continuation token indicating where to resume paging or <see langword="null" /> to begin paging from the beginning.</param>
    /// <param name="pageSize">The number of items that should be requested. It's not guaranteed that the value will be respected.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="PagedResult{TEntity}"/> of <typeparamref name="TEntity" /> instances that matches the filter.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="filter" /> is <see langword="null" /></exception>
    Task<PagedResult<TEntity>> FindPageAsync(
        Expression<Func<TEntity, bool>> filter,
        string? continuationToken = default,
        int? pageSize = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find a page of entities using the specified <paramref name="filter" /> expression.
    /// </summary>
    /// <param name="filter">The OData filter query.</param>
    /// <param name="continuationToken">A continuation token indicating where to resume paging or <see langword="null" /> to begin paging from the beginning.</param>
    /// <param name="pageSize">The number of items that should be requested. It's not guaranteed that the value will be respected.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="PagedResult{TEntity}"/> of <typeparamref name="TEntity" /> instances that matches the filter.
    /// </returns>
    Task<PagedResult<TEntity>> FindPageAsync(
        string? filter = null,
        string? continuationToken = default,
        int? pageSize = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find the first entity using the specified <paramref name="filter"/> expression.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An instance of <typeparamref name="TEntity"/> that matches the filter if found, otherwise <see langword="null" />.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="filter"/> is <see langword="null" /></exception>
    Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find the first entity using the specified <paramref name="filter"/> expression.
    /// </summary>
    /// <param name="filter">The OData filter query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An instance of <typeparamref name="TEntity"/> that matches the filter if found, otherwise <see langword="null" />.
    /// </returns>
    Task<TEntity?> FindOneAsync(
        string? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the specified <paramref name="entity" /> in the underlying data store by inserting if doesn't exist, or updating if it does.
    /// </summary>
    /// <param name="entity">The entity to be saved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The <typeparamref name="TEntity"/> that was saved.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="entity"/> is <see langword="null" /></exception>
    Task<TEntity> SaveAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts the specified <paramref name="entity" /> to the underlying data store.
    /// </summary>
    /// <param name="entity">The entity to be inserted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The <typeparamref name="TEntity"/> that was inserted.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="entity"/> is <see langword="null" /></exception>
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the specified <paramref name="entity" /> in the underlying data store.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The <typeparamref name="TEntity"/> that was updated.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="entity"/> is <see langword="null" /></exception>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits the specified <paramref name="entities" /> as batch transaction to the service for execution
    /// </summary>
    /// <param name="entities">The entities to process.</param>
    /// <param name="transactionType">Type of the batch transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task BatchAsync(
        IEnumerable<TEntity> entities,
        TableTransactionActionType transactionType = TableTransactionActionType.Add,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified <paramref name="entity" /> from the underlying data store.
    /// </summary>
    /// <param name="entity">The entity to be deleted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="entity"/> is <see langword="null" /></exception>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity with the specified <paramref name="rowKey" /> and <paramref name="partitionKey" /> from the underlying data store.
    /// </summary>
    /// <param name="rowKey">The entity rowkey identifier.</param>
    /// <param name="partitionKey">The partition key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="System.ArgumentNullException">
    /// <paramref name="rowKey"/> or <paramref name="partitionKey"/> is <see langword="null" />.
    /// </exception>
    Task DeleteAsync(
        string rowKey,
        string partitionKey,
        CancellationToken cancellationToken = default);
}
