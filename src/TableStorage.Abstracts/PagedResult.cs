using Microsoft.Extensions.DependencyInjection;

namespace TableStorage.Abstracts;

/// <summary>
/// A single page of data values from a service request
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <param name="Data">The data values in this page</param>
/// <param name="ContinuationToken">The continuation token used to request the next page of data. The continuation token may be null when there are no more pages./// </param>
public record PagedResult<TEntity>(IReadOnlyCollection<TEntity>? Data, string? ContinuationToken = null);
