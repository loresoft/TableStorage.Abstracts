# TableStorage.Abstracts

A .NET library that provides abstract base classes and repository patterns for Azure Table Storage, simplifying data access and operations with a clean, testable interface.

[![Build status](https://github.com/loresoft/TableStorage.Abstracts/workflows/Build/badge.svg)](https://github.com/loresoft/TableStorage.Abstracts/actions)
[![NuGet Version](https://img.shields.io/nuget/v/TableStorage.Abstracts.svg?style=flat-square)](https://www.nuget.org/packages/TableStorage.Abstracts/)
[![Coverage Status](https://coveralls.io/repos/github/loresoft/TableStorage.Abstracts/badge.svg?branch=main)](https://coveralls.io/github/loresoft/TableStorage.Abstracts?branch=main)

## Installation

Install the package via NuGet Package Manager:

```powershell
# Package Manager Console
Install-Package TableStorage.Abstracts
```

```bash
# .NET CLI
dotnet add package TableStorage.Abstracts
```

**NuGet Package:** [TableStorage.Abstracts](https://www.nuget.org/packages/TableStorage.Abstracts/)

## Features

- **Repository Pattern**: Clean abstraction over Azure Table Storage operations
- **Query Operations**: Find single entities, collections, and paginated results
- **CRUD Operations**: Full Create, Read, Update, Delete support with async/await
- **Batch Processing**: Efficient bulk insert, update, and delete operations
- **Dependency Injection**: Built-in support for Microsoft.Extensions.DependencyInjection
- **Auto-Initialization**: Automatic table creation on first use
- **Key Generation**: Automatic RowKey generation using ULID
- **Multi-Framework**: Supports .NET Standard 2.0, .NET 8.0, and .NET 9.0

## Quick Start

### 1. Define Your Entity

Create an entity class that inherits from `TableEntityBase` or implements `ITableEntity`:

```csharp
public class User : TableEntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
```

### 2. Configure Dependency Injection

Register the repository services in your `Program.cs` or `Startup.cs`:

```csharp
// Using connection string from configuration
builder.Services.AddTableStorageRepository("AzureStorage");

// Or with direct connection string
builder.Services.AddTableStorageRepository("UseDevelopmentStorage=true");
```

**Configuration Example** (`appsettings.json`):

```json
{
  "AzureStorage": "UseDevelopmentStorage=true"
}
```

### 3. Use the Repository

Inject and use `ITableRepository<T>` in your services:

```csharp
public class UserService
{
    private readonly ITableRepository<User> _userRepository;

    public UserService(ITableRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> CreateUserAsync(string name, string email)
    {
        var user = new User { Name = name, Email = email };
        return await _userRepository.CreateAsync(user);
    }

    public async Task<User?> GetUserAsync(string rowKey, string partitionKey)
    {
        return await _userRepository.FindAsync(rowKey, partitionKey);
    }
}
```

## Usage Guide

### Alternative Registration Methods

Register with connection string from configuration:

```csharp
services.AddTableStorageRepository("AzureStorage");
```

Register with direct connection string:

```csharp
services.AddTableStorageRepository("UseDevelopmentStorage=true");
```

#### Resolving Dependencies

Resolve `ITableRepository<T>`:

```csharp
var repository = serviceProvider.GetRequiredService<ITableRepository<User>>();
```

Resolve `TableServiceClient`:

```csharp
var tableServiceClient = serviceProvider.GetRequiredService<TableServiceClient>();
```

### Custom Repository Implementation

Create a custom repository by inheriting from `TableRepository<T>`:

```csharp
public class UserRepository : TableRepository<User>
{
    public UserRepository(ILoggerFactory logFactory, TableServiceClient tableServiceClient)
        : base(logFactory, tableServiceClient)
    { }

    protected override void BeforeSave(User entity)
    {
        // Use email as partition key for better data distribution
        entity.PartitionKey = entity.Email;
        base.BeforeSave(entity);
    }

    // Override default table name (uses typeof(TEntity).Name by default)
    protected override string GetTableName() => "UserProfiles";

    // Custom business logic methods
    public async Task<IReadOnlyList<User>> FindActiveUsersAsync()
    {
        return await FindAllAsync(u => u.IsActive);
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await FindOneAsync(u => u.Email == email);
    }
}
```

Register your custom repository:

```csharp
services.AddTableStorageRepository("AzureStorage");
services.AddScoped<UserRepository>();
```

## Understanding RowKey and PartitionKey

Azure Table Storage uses a two-part key system that's fundamental to performance and scalability. Understanding these keys is crucial for designing efficient table storage solutions.

### Key Components

- **RowKey**: Unique identifier within a partition
  - Must be unique within the partition
  - Automatically generated using ULID if not explicitly set
  - Combined with PartitionKey forms the primary key
  - Case-sensitive string (max 1KB)

- **PartitionKey**: Logical grouping for related entities
  - Determines physical storage distribution
  - Entities with the same PartitionKey are stored together
  - Defaults to RowKey value if not explicitly set
  - Case-sensitive string (max 1KB)

### Key Generation Examples

**Automatic key generation** (recommended for simple scenarios):

```csharp
var user = new User 
{ 
    Name = "John Doe",
    Email = "john@example.com"
    // RowKey will be auto-generated using ULID
    // PartitionKey will default to the generated RowKey value
};

await repository.CreateAsync(user);
// Result: RowKey = "01ARZ3NDEKTSV4RRFFQ69G5FAV", PartitionKey = "01ARZ3NDEKTSV4RRFFQ69G5FAV"
```

**Explicit key assignment** for custom partitioning:

```csharp
var user = new User 
{ 
    Name = "John Doe",
    Email = "john@example.com",
    PartitionKey = "Department_Engineering", // Group by department
    RowKey = "user_john_doe"                // Custom identifier
};
```

**Strategic partitioning** for better performance:

```csharp
// Time-based partitioning (good for log data)
var eventTime = DateTimeOffset.UtcNow;
var logEntry = new LogEvent 
{ 
    Message = "User login",
    PartitionKey = KeyGenerator.GeneratePartitionKey(eventTime), // 5-minute intervals with reverse chronological ordering
    RowKey = KeyGenerator.GenerateRowKey(eventTime)              // ULID with reverse chronological ordering
};

// Geographic partitioning
var order = new Order 
{ 
    ProductName = "Widget",
    PartitionKey = "Region_US_West",  // Group by region
    RowKey = $"order_{Guid.NewGuid()}"
};
```

### Performance Implications

**Query Performance:**

- Queries filtering by both PartitionKey and RowKey are fastest (point queries)
- Queries filtering by PartitionKey only are efficient (partition scans)
- Queries without PartitionKey scan the entire table (avoid when possible)

```csharp
// Fastest: Point query (both keys)
var user = await repository.FindAsync("user_001", "Department_Engineering");

// Fast: Partition scan
var deptUsers = await repository.FindAllAsync(u => u.PartitionKey == "Department_Engineering");

// Slow: Table scan (avoid if possible)
var activeUsers = await repository.FindAllAsync(u => u.IsActive);
```

## Query Operations

### Finding Single Entities

Find an entity by row and partition key (both required by Azure Table Storage):

```csharp
var user = await repository.FindAsync(rowKey, partitionKey);
if (user != null)
{
    Console.WriteLine($"Found user: {user.Name}");
}
```

### Finding Multiple Entities

Find all entities matching a filter expression:

```csharp
var activeUsers = await repository.FindAllAsync(u => u.IsActive);
```

Find a single entity by filter (returns first match):

```csharp
var user = await repository.FindOneAsync(u => u.Email == "john@example.com");
```

### Paginated Queries

Azure Table Storage supports forward-only paging using continuation tokens:

```csharp
var pageResult = await repository.FindPageAsync(
    filter: u => u.IsActive,
    pageSize: 20);

Console.WriteLine($"Found {pageResult.Items.Count} users");

// Loop through all pages
while (!string.IsNullOrEmpty(pageResult.ContinuationToken))
{
    pageResult = await repository.FindPageAsync(
        filter: u => u.IsActive,
        continuationToken: pageResult.ContinuationToken,
        pageSize: 20);
    
    Console.WriteLine($"Next page: {pageResult.Items.Count} users");
}
```

## CRUD Operations

### Create Operations

Create a new entity:

```csharp
var user = new User 
{ 
    Name = "John Doe", 
    Email = "john@example.com",
    IsActive = true 
};

var createdUser = await repository.CreateAsync(user);
Console.WriteLine($"Created user with ID: {createdUser.RowKey}");
```

### Update Operations

Update an existing entity:

```csharp
user.Name = "John Smith";
var updatedUser = await repository.UpdateAsync(user);
```

### Upsert Operations

Save (create or update) an entity:

```csharp
var savedUser = await repository.SaveAsync(user);
```

### Delete Operations

Delete an entity:

```csharp
await repository.DeleteAsync(user);

// Or delete by keys
await repository.DeleteAsync(rowKey, partitionKey);
```

## Batch Operations

Perform bulk operations efficiently using either the core `BatchAsync` method or the convenient extension methods:

> **Note**: Azure Table Storage batch operations are limited to 100 entities per batch and all entities must share the same PartitionKey. The batch methods automatically handle these limitations by grouping entities by PartitionKey and chunking them into batches of 100 items.

### Core Batch Method

The fundamental batch operation method with explicit transaction type:

```csharp
var users = new List<User>
{
    new() { Name = "User 1", Email = "user1@example.com" },
    new() { Name = "User 2", Email = "user2@example.com" },
    new() { Name = "User 3", Email = "user3@example.com" }
};

// Insert new entities
await repository.BatchAsync(users, TableTransactionActionType.Add);

// Update existing entities
await repository.BatchAsync(users, TableTransactionActionType.UpdateReplace);

// Merge changes (partial updates)
await repository.BatchAsync(users, TableTransactionActionType.UpdateMerge);

// Delete entities
await repository.BatchAsync(users, TableTransactionActionType.Delete);
```

### Extension Methods for Convenience

Use the convenient extension methods for common batch operations:

#### Bulk Create

```csharp
var users = new List<User>
{
    new() { Name = "User 1", Email = "user1@example.com" },
    new() { Name = "User 2", Email = "user2@example.com" },
    new() { Name = "User 3", Email = "user3@example.com" }
};

// Create multiple entities (equivalent to TableTransactionActionType.Add)
var createdCount = await repository.CreateBatchAsync(users);
Console.WriteLine($"Created {createdCount} users");
```

#### Bulk Update

```csharp
// Update multiple entities (equivalent to TableTransactionActionType.UpdateReplace)
var updatedCount = await repository.UpdateBatchAsync(users);
Console.WriteLine($"Updated {updatedCount} users");
```

#### Bulk Save (Upsert)

```csharp
// Save multiple entities - insert if new, update if exists (equivalent to TableTransactionActionType.UpsertReplace)
var savedCount = await repository.SaveBatchAsync(users);
Console.WriteLine($"Saved {savedCount} users");
```

#### Bulk Delete

```csharp
// Delete multiple entities
var deletedCount = await repository.DeleteBatchAsync(users);
Console.WriteLine($"Deleted {deletedCount} users");
```

#### Advanced Bulk Delete

Delete entities by filter expression with automatic pagination:

```csharp
// Delete all inactive users (processes in pages to limit memory usage)
var deletedCount = await repository.DeleteBatchAsync(u => !u.IsActive);
Console.WriteLine($"Deleted {deletedCount} inactive users");

// Delete using OData filter query
var deletedCount = await repository.DeleteBatchAsync("IsActive eq false");
Console.WriteLine($"Deleted {deletedCount} inactive users");
```

> **Performance Tip**: The filter-based delete methods automatically handle pagination to prevent memory issues when deleting large numbers of entities. Each page is processed using batch operations for optimal performance.

## Advanced Usage

### Custom Key Generation

Override the default ULID key generation:

```csharp
public class CustomRepository : TableRepository<User>
{
    public override string NewRowKey()
    {
        return Guid.NewGuid().ToString();
    }
}
```

### Table Initialization

Tables are automatically created on first use. To manually initialize:

```csharp
var tableClient = await repository.GetClientAsync();
await tableClient.CreateIfNotExistsAsync();
```

### Working with TableServiceClient

Access the underlying Azure Table Storage client:

```csharp
var tableServiceClient = serviceProvider.GetRequiredService<TableServiceClient>();
var tables = tableServiceClient.QueryTablesAsync();
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Links

- [Azure Table Storage Documentation](https://docs.microsoft.com/en-us/azure/storage/tables/)
- [Azure.Data.Tables NuGet Package](https://www.nuget.org/packages/Azure.Data.Tables/)
