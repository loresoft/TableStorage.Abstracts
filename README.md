# TableStorage.Abstracts

Azure Table Storage Abstracts library defines abstract base classes for repository pattern.

[![Build status](https://github.com/loresoft/TableStorage.Abstracts/workflows/Build/badge.svg)](https://github.com/loresoft/TableStorage.Abstracts/actions)

[![NuGet Version](https://img.shields.io/nuget/v/TableStorage.Abstracts.svg?style=flat-square)](https://www.nuget.org/packages/TableStorage.Abstracts/)

[![Coverage Status](https://coveralls.io/repos/github/loresoft/TableStorage.Abstracts/badge.svg?branch=main)](https://coveralls.io/github/loresoft/TableStorage.Abstracts?branch=main)

## Download

The TableStorage.Abstracts library is available on nuget.org via package name `TableStorage.Abstracts`.

To install TableStorage.Abstracts, run the following command in the Package Manager Console

```powershell
Install-Package TableStorage.Abstracts
```

More information about NuGet package available at
<https://nuget.org/packages/TableStorage.Abstracts>

## Features

- find one, many and by paged results
- create, update and delete pattern
- batch processing for bulk insert/update
- table initialization on first use

## Usage

### Dependency Injection

Register services with the Azure storage connection string named 'AzureStorage' loaded from configuration

```c#
services.AddTableStorageRepository("AzureStorage");
```

Example appsettings.json file

```json
{
  "AzureStorage": "UseDevelopmentStorage=true"
}
```

Register with the Azure storage connection string passed in

```c#
services.AddTableStorageRepository("UseDevelopmentStorage=true");
```

Resolve `ITableRepository<T>`

```c#
var repository = serviceProvider.GetRequiredService<ITableRepository<Item>>();
```

Resolve `TableServiceClient`

```c#
var tableServiceClient = serviceProvider.GetRequiredService<TableServiceClient>();
```

### Custom Repository

Create a custom repository instance by inheriting `TableRepository<T>`

```c#
public class UserRepository : TableRepository<User>
{
    public UserRepository(ILoggerFactory logFactory, TableServiceClient tableServiceClient)
        : base(logFactory, tableServiceClient)
    { }

    protected override void BeforeSave(User entity)
    {
        // use email as partition key
        entity.PartitionKey = entity.Email;

        base.BeforeSave(entity);
    }

    // uses typeof(TEntity).Name by default, override with custom table name
    protected override string GetTableName() => "UserMembership";
}
```

### Query Operations

Find an entity by row and partition key.  Note, table storage requires both.

```c#
var repository = serviceProvider.GetRequiredService<ITableRepository<Item>>();
var readResult = await repository.FindAsync(rowKey, partitionKey);
```

Find all by filter expression

```c#
var queryResults = await repository.FindAllAsync(r => r.IsActive);
```

Find all by filter expression

```c#
var itemResult = await repository.FindOneAsync(r => r.Name == itemName);
```

Find a page of items by filter.  Note, Azure Table storage only supports forward paging by Continuation Token.

```c#
var pageResult = await repository.FindPageAsync(
    filter: r => r.IsActive,
    pageSize: 20);

// loop through pages
while (!string.IsNullOrEmpty(pageResult.ContinuationToken))
{
    // get next paging using previous continuation token
    pageResult = await repository.FindPageAsync(
        filter: r => r.IsActive,
        continuationToken: pageResult.ContinuationToken,
        pageSize: 20);
}
```

### Update Operations

Create an item

```c#
 var createdItem = await repository.CreateAsync(item);
```

Update an item

```c#
 var updatedItem = await repository.UpdateAsync(item);
```

Save or Upsert an item

```c#
 var savedItem = await repository.SaveAsync(item);
```

#### RowKey and PartitionKey

Azure Table Storage requires both a `RowKey` and `PartitionKey`

The base repository will set the `RowKey` if it hasn't already been set using the `NewRowKey()` method.  The default implementation is `Guid.NewGuid().ToString("N")`

If `PartitionKey` hasn't been set, `RowKey` will be used.

### Batch Operations

Bulk insert data

```c#
await repository.BatchAsync(items);
```

Batch Merge data

```c#
await repository.BatchAsync(items, TableTransactionActionType.UpdateMerge);
```
