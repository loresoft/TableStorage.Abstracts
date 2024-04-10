using TableStorage.Abstracts.Tests.Models;

namespace TableStorage.Abstracts.Tests.Services;

public interface ILogEventRepository : ITableRepository<LogEvent>
{
    Task<PagedResult<LogEvent>> QueryByDate(
        DateOnly date,
        string? level = null,
        string? continuationToken = null,
        int? pageSize = 100,
        CancellationToken cancellationToken = default);
}
