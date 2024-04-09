using Azure;
using Azure.Data.Tables;

namespace TableStorage.Abstracts;

/// <summary>
/// A base class defining the required properties for a table entity model.
/// </summary>
/// <seealso cref="Azure.Data.Tables.ITableEntity" />
public abstract class TableEntityBase : ITableEntity
{
    /// <inheritdoc/>
    public string RowKey { get; set; } = null!;

    /// <inheritdoc/>
    public string PartitionKey { get; set; } = null!;

    /// <inheritdoc/>
    public DateTimeOffset? Timestamp { get; set; }

    /// <inheritdoc/>
    public ETag ETag { get; set; }
}
