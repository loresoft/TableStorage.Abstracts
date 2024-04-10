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

    public async Task<PagedResult<LogEvent>> QueryByDate(
        DateOnly date,
        string? level = null,
        string? continuationToken = null,
        int? pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var baseDate = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var upperDate = baseDate.ToReverseChronological();
        var lowwerDate = baseDate.AddDays(1).ToReverseChronological();

        var upper = $"{upperDate.Ticks:D19}";
        var lower = $"{lowwerDate.Ticks:D19}";

        var filter = $"(PartitionKey ge '{lower}') and (PartitionKey lt '{upper}')";

        if (level.HasValue())
            filter += $" and (Level eq '{level}')";

        return await FindPageAsync(filter, continuationToken, pageSize, cancellationToken);
    }

    public override string NewRowKey()
    {
        // store newest log first
        var timestamp = DateTimeOffset.UtcNow.ToReverseChronological();
        return Ulid.NewUlid(timestamp).ToString();
    }

    protected override void BeforeSave(LogEvent entity)
    {
        if (entity.RowKey.IsNullOrWhiteSpace())
            entity.RowKey = NewRowKey();

        if (entity.PartitionKey.IsNullOrWhiteSpace())
        {
            var timespan = entity.Timestamp ?? DateTimeOffset.UtcNow;
            var roundedDate = timespan
                .Round(TimeSpan.FromMinutes(5))
                .ToReverseChronological();

            // create a 19 character String for reverse chronological ordering.
            entity.PartitionKey = $"{roundedDate.Ticks:D19}";
        }
    }

    protected override string GetTableName() => "LogEvent";
}
