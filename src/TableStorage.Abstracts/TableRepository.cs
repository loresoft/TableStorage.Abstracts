using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Azure;
using Azure.Data.Tables;

using Microsoft.Extensions.Logging;

using TableStorage.Abstracts.Extensions;

namespace TableStorage.Abstracts;

/// <summary>
/// A repository pattern implementation for Azure Table storage data operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class TableRepository<TEntity> : ITableRepository<TEntity>
    where TEntity : class, ITableEntity
{
    private readonly Lazy<Task<TableClient>> _lazyTableClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="TableRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="logFactory">The log factory.</param>
    /// <param name="tableServiceClient">The table service client.</param>
    /// <exception cref="System.ArgumentNullException">
    /// <paramref name="logFactory"/> or <paramref name="tableServiceClient"/> is <see langword="null" />
    /// </exception>
    public TableRepository(ILoggerFactory logFactory, TableServiceClient tableServiceClient)
    {
        if (logFactory is null)
            throw new ArgumentNullException(nameof(logFactory));

        if (tableServiceClient is null)
            throw new ArgumentNullException(nameof(tableServiceClient));

        Logger = logFactory.CreateLogger(GetType());
        TableServiceClient = tableServiceClient;

        _lazyTableClient = new Lazy<Task<TableClient>>(InitializeTableAsync);
    }

    /// <summary>
    /// Gets the table service client.
    /// </summary>
    /// <value>
    /// The table service client.
    /// </value>
    protected TableServiceClient TableServiceClient { get; }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    /// <value>
    /// The logger.
    /// </value>
    protected ILogger Logger { get; }

    /// <inheritdoc/>
    public virtual string NewRowKey() => Guid.NewGuid().ToString("N");

    /// <inheritdoc/>
    public Task<TableClient> GetClientAsync() => _lazyTableClient.Value;

    /// <inheritdoc/>
    public async Task<TEntity?> FindAsync(
        string rowKey,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        var tableClient = await GetClientAsync().ConfigureAwait(false);

        var response = await tableClient
            .GetEntityIfExistsAsync<TEntity>(partitionKey, rowKey, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        LogResponse(response);

        return response.HasValue ? response.Value : default;
    }


    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        var tableClient = await GetClientAsync().ConfigureAwait(false);

        var pageable = tableClient.QueryAsync(
            filter: filter,
            cancellationToken: cancellationToken);

        if (pageable == null)
            return [];

        var list = new List<TEntity>();

        await foreach (var item in pageable)
            list.Add(item);

        return list;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<TEntity>> FindAllAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var tableClient = await GetClientAsync().ConfigureAwait(false);

        var pageable = tableClient.QueryAsync<TEntity>(
            filter: filter,
            cancellationToken: cancellationToken);

        if (pageable == null)
            return [];

        var list = new List<TEntity>();

        await foreach (var item in pageable)
            list.Add(item);

        return list;
    }


    /// <inheritdoc/>
    public async Task<PagedResult<TEntity>> FindPageAsync(
        Expression<Func<TEntity, bool>> filter,
        string? continuationToken = default,
        int? pageSize = default,
        CancellationToken cancellationToken = default)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        var tableClient = await GetClientAsync().ConfigureAwait(false);

        var pageable = tableClient.QueryAsync(
            filter: filter,
            cancellationToken: cancellationToken);

        if (pageable == null)
            return new PagedResult<TEntity>([]);

        await foreach (var page in pageable.AsPages(continuationToken, pageSize))
            return new PagedResult<TEntity>(page.Values, page.ContinuationToken);

        return new PagedResult<TEntity>([]);
    }

    /// <inheritdoc/>
    public async Task<PagedResult<TEntity>> FindPageAsync(
        string? filter = null,
        string? continuationToken = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var tableClient = await GetClientAsync().ConfigureAwait(false);

        var pageable = tableClient.QueryAsync<TEntity>(
            filter: filter,
            cancellationToken: cancellationToken);

        if (pageable == null)
            return new PagedResult<TEntity>([]);

        await foreach (var page in pageable.AsPages(continuationToken, pageSize))
            return new PagedResult<TEntity>(page.Values, page.ContinuationToken);

        return new PagedResult<TEntity>([]);
    }


    /// <inheritdoc/>
    public async Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>>? filter,
        CancellationToken cancellationToken = default)
    {
        var tableClient = await GetClientAsync().ConfigureAwait(false);

        var pageable = tableClient.QueryAsync(
            filter: filter,
            maxPerPage: 1,
            cancellationToken: cancellationToken);

        if (pageable == null)
            return default;

        await foreach (var item in pageable)
            return item;

        return default;
    }

    /// <inheritdoc/>
    public async Task<TEntity?> FindOneAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var tableClient = await GetClientAsync().ConfigureAwait(false);

        var pageable = tableClient.QueryAsync<TEntity>(
            filter: filter,
            maxPerPage: 1,
            cancellationToken: cancellationToken);

        if (pageable == null)
            return default;

        await foreach (var item in pageable)
            return item;

        return default;
    }


    /// <inheritdoc/>
    public async Task<TEntity> SaveAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var tableClient = await GetClientAsync().ConfigureAwait(false);

        BeforeSave(entity);

        var saveResponse = await tableClient
            .UpsertEntityAsync(entity, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        LogResponse(saveResponse);

        var findResponse = await tableClient
            .GetEntityAsync<TEntity>(entity.PartitionKey, entity.RowKey, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        LogResponse(findResponse);

        var result = findResponse.Value;

        AfterSave(result);

        return result;
    }

    /// <inheritdoc/>
    public async Task<TEntity> CreateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var tableClient = await GetClientAsync().ConfigureAwait(false);

        BeforeSave(entity);

        var response = await tableClient
            .AddEntityAsync(entity, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        LogResponse(response);

        var findResponse = await tableClient
            .GetEntityAsync<TEntity>(entity.PartitionKey, entity.RowKey, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        LogResponse(findResponse);

        var result = findResponse.Value;

        AfterSave(result);

        return result;
    }

    /// <inheritdoc/>
    public Task<TEntity> UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
        => SaveAsync(entity, cancellationToken);


    /// <inheritdoc/>
    public async Task BatchAsync(
        IEnumerable<TEntity> entities,
        TableTransactionActionType transactionType = TableTransactionActionType.Add,
        CancellationToken cancellationToken = default)
    {
        if (entities is null)
            throw new ArgumentNullException(nameof(entities));

        foreach (var entity in entities)
            BeforeSave(entity);

        // write in batches by partition key
        var documentGroups = entities
            .GroupBy(p => p.PartitionKey);

        var tableClient = await GetClientAsync().ConfigureAwait(false);

        foreach (var documentGroup in documentGroups)
        {
            // create table transactions
            var transactionActions = documentGroup
                .Select(tableEntity => new TableTransactionAction(transactionType, tableEntity))
                .ToList();

            // can only send 100 transactions at a time
            foreach (var transactionBatch in transactionActions.Chunk(100))
                await tableClient.SubmitTransactionAsync(transactionBatch, cancellationToken);
        }
    }


    /// <inheritdoc/>
    public async Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await DeleteAsync(entity.RowKey, entity.PartitionKey, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(
        string rowKey,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        var tableClient = await GetClientAsync().ConfigureAwait(false);

        var response = await tableClient.DeleteEntityAsync(partitionKey, rowKey, cancellationToken: cancellationToken);
        LogResponse(response);
    }


    /// <summary>
    /// Called before a saving the specified <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">The entity being saved.</param>
    protected virtual void BeforeSave(TEntity entity)
    {
        if (entity.RowKey.IsNullOrWhiteSpace())
            entity.RowKey = NewRowKey();

        if (entity.PartitionKey.IsNullOrWhiteSpace())
            entity.PartitionKey = entity.RowKey;
    }

    /// <summary>
    /// Called after a saving the specified <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">The entity being saved.</param>
    protected virtual void AfterSave(TEntity entity)
    {
    }


    /// <summary>
    /// Logs the service response.
    /// </summary>
    /// <param name="response">The sevices response message.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="sourceFilePath">The source file path.</param>
    /// <param name="sourceLineNumber">The source line number.</param>
    protected virtual void LogResponse(
        Response response,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        if (response == null)
            return;

        var level = response.IsError ? LogLevel.Error : LogLevel.Debug;

        if (!Logger.IsEnabled(level))
            return;

        Logger.Log(
            level,
            "Response from '{MemberName}'; Status: '{StatusCode}'; Reason: {ReasonPhrase}; File: '{FileName}' ({LineNumber})",
            memberName,
            response.Status,
            response.ReasonPhrase,
            Path.GetFileName(sourceFilePath),
            sourceLineNumber
        );
    }

    /// <summary>
    /// Logs the service response.
    /// </summary>
    /// <typeparam name="T">The type of response data</typeparam>
    /// <param name="response">The sevices response message.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="sourceFilePath">The source file path.</param>
    /// <param name="sourceLineNumber">The source line number.</param>
    protected void LogResponse<T>(
        NullableResponse<T> response,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        if (response == null)
            return;

        var baseResponse = response.GetRawResponse();
        LogResponse(baseResponse, memberName, sourceFilePath, sourceLineNumber);
    }


    /// <summary>
    /// Initializes the <see cref="TableClient"/> on first use. Override to customize how the
    /// <see cref="TableClient"/> is created and how the storage table is initialized.
    /// </summary>
    /// <returns>A <see cref="TableClient"/> instance.</returns>
    protected virtual async Task<TableClient> InitializeTableAsync()
    {
        // one-time initialize
        string tableName = GetTableName();
        var tableClient = TableServiceClient.GetTableClient(tableName);

        var response = await tableClient.CreateIfNotExistsAsync();
        LogResponse(response);

        return tableClient;
    }

    /// <summary>
    /// Gets the name of the storage table to use for this repository. Uses typeof(TEntity).Name by default
    /// </summary>
    /// <returns>The name of the storage table to use for this repository</returns>
    protected virtual string GetTableName() => typeof(TEntity).Name;
}
