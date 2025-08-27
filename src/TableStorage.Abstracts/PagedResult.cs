namespace TableStorage.Abstracts;

/// <summary>
/// Represents a single page of data values from a paginated service request.
/// </summary>
/// <typeparam name="TEntity">The type of the entity contained in the page data.</typeparam>
/// <param name="Data">The collection of data values in this page. May be null or empty if no data is available.</param>
/// <param name="ContinuationToken">The continuation token used to request the next page of data. A null value indicates there are no more pages available.</param>
/// <remarks>
/// <para>
/// This record provides a standardized way to handle paginated results from Azure Table Storage queries.
/// It encapsulates both the data for the current page and the token needed to retrieve subsequent pages.
/// </para>
/// <para>
/// The continuation token is an opaque string that should be passed to the next query request
/// to retrieve the following page of results. When the continuation token is null, it indicates
/// that all available data has been retrieved and no more pages exist.
/// </para>
/// </remarks>
public record PagedResult<TEntity>(IReadOnlyCollection<TEntity>? Data, string? ContinuationToken = null);
