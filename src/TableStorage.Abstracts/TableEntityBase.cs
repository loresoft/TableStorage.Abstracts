using Azure;
using Azure.Data.Tables;

namespace TableStorage.Abstracts;

/// <summary>
/// Provides a base class that implements the required properties for an Azure Table Storage entity model.
/// </summary>
/// <remarks>
/// <para>
/// This abstract base class implements <see cref="ITableEntity"/> and provides the four required properties
/// for Azure Table Storage entities: <see cref="RowKey"/>, <see cref="PartitionKey"/>, <see cref="Timestamp"/>, and <see cref="ETag"/>.
/// </para>
/// <para>
/// Inherit from this class to create custom entity models without having to implement the required
/// Azure Table Storage properties manually. The base class handles the infrastructure concerns while
/// derived classes can focus on domain-specific properties and behavior.
/// </para>
/// <para>
/// Example usage:
/// </para>
/// <code>
/// public class Customer : TableEntityBase
/// {
///     public string Name { get; set; } = string.Empty;
///     public string Email { get; set; } = string.Empty;
///     public DateTime CreatedDate { get; set; }
/// }
/// </code>
/// </remarks>
/// <seealso cref="ITableEntity" />
public abstract class TableEntityBase : ITableEntity
{
    /// <summary>
    /// Gets or sets the row key for the table entity.
    /// </summary>
    /// <value>
    /// The row key that uniquely identifies the entity within its partition.
    /// </value>
    /// <remarks>
    /// The row key must be unique within the partition and, combined with the partition key,
    /// forms the primary key for the entity in Azure Table Storage.
    /// </remarks>
    public string RowKey { get; set; } = null!;

    /// <summary>
    /// Gets or sets the partition key for the table entity.
    /// </summary>
    /// <value>
    /// The partition key that determines the logical partition where the entity is stored.
    /// </value>
    /// <remarks>
    /// The partition key is used by Azure Table Storage to distribute entities across multiple
    /// storage nodes for scalability and performance. Entities with the same partition key
    /// are stored together and can be queried efficiently as a group.
    /// </remarks>
    public string PartitionKey { get; set; } = null!;

    /// <summary>
    /// Gets or sets the timestamp for the table entity.
    /// </summary>
    /// <value>
    /// The timestamp indicating when the entity was last modified, or <see langword="null"/> for new entities.
    /// </value>
    /// <remarks>
    /// This property is automatically managed by Azure Table Storage and reflects the last
    /// modification time of the entity. It is set by the service and should not be modified manually.
    /// </remarks>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the ETag for the table entity.
    /// </summary>
    /// <value>
    /// The ETag that represents the entity's version for optimistic concurrency control.
    /// </value>
    /// <remarks>
    /// The ETag is automatically managed by Azure Table Storage and is used for optimistic concurrency
    /// control. It changes each time the entity is modified, allowing for conflict detection
    /// during concurrent updates.
    /// </remarks>
    public ETag ETag { get; set; }
}
