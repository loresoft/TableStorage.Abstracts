using Azure.Data.Tables;

using Microsoft.Extensions.Logging;

using TableStorage.Abstracts.Extensions;
using TableStorage.Abstracts.Tests.Models;

namespace TableStorage.Abstracts.Tests.Services;

public class LogEventRepository : TableRepository<LogEvent>, ILogEventRepository
{
    public LogEventRepository(ILoggerFactory logFactory, TableServiceClient tableServiceClient)
        : base(logFactory, tableServiceClient)
    {
    }

    public async Task<PagedResult<LogEvent>> Query(
        DateOnly date,
        string? level = null,
        string? continuationToken = null,
        int? pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var filter = KeyGenerator.GeneratePartitionKeyQuery(date);

        if (level.HasValue())
            filter += $" and (Level eq '{level}')";

        return await FindPageAsync(filter, continuationToken, pageSize, cancellationToken);
    }

    public override string NewRowKey()
    {
        // store newest log first
        return KeyGenerator.GenerateRowKey(DateTimeOffset.UtcNow);
    }

    protected override void BeforeSave(LogEvent entity)
    {
        if (entity.RowKey.IsNullOrWhiteSpace())
            entity.RowKey = NewRowKey();

        if (entity.PartitionKey.IsNullOrWhiteSpace())
        {
            var timespan = entity.Timestamp ?? DateTimeOffset.UtcNow;
            entity.PartitionKey = KeyGenerator.GeneratePartitionKey(timespan);
        }
    }

    protected override string GetTableName() => "SampleLog";
}
