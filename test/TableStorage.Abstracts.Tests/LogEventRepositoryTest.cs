using Azure.Data.Tables;

using Microsoft.Extensions.DependencyInjection;

using TableStorage.Abstracts.Tests.Models;
using TableStorage.Abstracts.Tests.Services;

namespace TableStorage.Abstracts.Tests;

public class LogEventRepositoryTest(DatabaseFixture databaseFixture) : DatabaseTestBase(databaseFixture)
{

    [Fact]
    public async Task QueryTest()
    {
        var repository = Services.GetRequiredService<ILogEventRepository>();
        Assert.NotNull(repository);

        var today = DateOnly.FromDateTime(DateTime.Now);

        var result = await repository.Query(today);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateBatchAsync_Should_Create_Multiple_LogEvents()
    {
        // Arrange
        var repository = Services.GetRequiredService<ITableRepository<LogEvent>>();
        var logEvents = CreateTestLogEvents(5, "TestPartition1");

        // Act
        var createdCount = await repository.CreateBatchAsync(logEvents);

        // Assert
        Assert.Equal(5, createdCount);

        // Verify entities were created
        foreach (var logEvent in logEvents)
        {
            var retrieved = await repository.FindAsync(logEvent.RowKey, logEvent.PartitionKey);
            Assert.NotNull(retrieved);
            Assert.Equal(logEvent.Level, retrieved.Level);
            Assert.Equal(logEvent.MessageTemplate, retrieved.MessageTemplate);
        }
    }

    [Fact]
    public async Task UpdateBatchAsync_Should_Update_Multiple_LogEvents()
    {
        // Arrange
        var repository = Services.GetRequiredService<ITableRepository<LogEvent>>();
        var logEvents = CreateTestLogEvents(3, "TestPartition2");

        // Create entities first
        await repository.CreateBatchAsync(logEvents);

        // Modify entities
        foreach (var logEvent in logEvents)
        {
            logEvent.Level = "UpdatedLevel";
            logEvent.MessageTemplate = "Updated: " + logEvent.MessageTemplate;
        }

        // Act
        var updatedCount = await repository.UpdateBatchAsync(logEvents);

        // Assert
        Assert.Equal(3, updatedCount);

        // Verify entities were updated
        foreach (var logEvent in logEvents)
        {
            var retrieved = await repository.FindAsync(logEvent.RowKey, logEvent.PartitionKey);
            Assert.NotNull(retrieved);
            Assert.Equal("UpdatedLevel", retrieved.Level);
            Assert.StartsWith("Updated: ", retrieved.MessageTemplate);
        }
    }

    [Fact]
    public async Task SaveBatchAsync_Should_Upsert_Multiple_LogEvents()
    {
        // Arrange
        var repository = Services.GetRequiredService<ITableRepository<LogEvent>>();
        var newLogEvents = CreateTestLogEvents(2, "TestPartition3");
        var existingLogEvents = CreateTestLogEvents(2, "TestPartition3");

        // Create some existing entities
        await repository.CreateBatchAsync(existingLogEvents);

        // Modify existing entities
        foreach (var logEvent in existingLogEvents)
        {
            logEvent.Level = "ModifiedLevel";
        }

        // Combine new and existing entities
        var allLogEvents = new List<LogEvent>();
        allLogEvents.AddRange(newLogEvents);
        allLogEvents.AddRange(existingLogEvents);

        // Act
        var savedCount = await repository.SaveBatchAsync(allLogEvents);

        // Assert
        Assert.Equal(4, savedCount);

        // Verify new entities were created
        foreach (var logEvent in newLogEvents)
        {
            var retrieved = await repository.FindAsync(logEvent.RowKey, logEvent.PartitionKey);
            Assert.NotNull(retrieved);
            Assert.Equal(logEvent.Level, retrieved.Level);
        }

        // Verify existing entities were updated
        foreach (var logEvent in existingLogEvents)
        {
            var retrieved = await repository.FindAsync(logEvent.RowKey, logEvent.PartitionKey);
            Assert.NotNull(retrieved);
            Assert.Equal("ModifiedLevel", retrieved.Level);
        }
    }

    [Fact]
    public async Task DeleteBatchAsync_Should_Delete_Multiple_LogEvents()
    {
        // Arrange
        var repository = Services.GetRequiredService<ITableRepository<LogEvent>>();
        var logEvents = CreateTestLogEvents(4, "TestPartition4");

        // Create entities first
        await repository.CreateBatchAsync(logEvents);

        // Verify entities exist
        foreach (var logEvent in logEvents)
        {
            var retrieved = await repository.FindAsync(logEvent.RowKey, logEvent.PartitionKey);
            Assert.NotNull(retrieved);
        }

        // Act
        var deletedCount = await repository.DeleteBatchAsync(logEvents);

        // Assert
        Assert.Equal(4, deletedCount);

        // Verify entities were deleted
        foreach (var logEvent in logEvents)
        {
            var retrieved = await repository.FindAsync(logEvent.RowKey, logEvent.PartitionKey);
            Assert.Null(retrieved);
        }
    }

    [Fact]
    public async Task DeleteBatchAsync_ByFilter_Should_Delete_Matching_LogEvents()
    {
        // Arrange
        var repository = Services.GetRequiredService<ITableRepository<LogEvent>>();
        var partitionKey = "TestPartition5";
        var logEventsToDelete = CreateTestLogEvents(3, partitionKey, "ERROR");
        var logEventsToKeep = CreateTestLogEvents(2, partitionKey, "INFO");

        var allLogEvents = new List<LogEvent>();
        allLogEvents.AddRange(logEventsToDelete);
        allLogEvents.AddRange(logEventsToKeep);

        // Create all entities
        await repository.CreateBatchAsync(allLogEvents);

        // Act - Delete only ERROR level events
        var deletedCount = await repository.DeleteBatchAsync(e => e.Level == "ERROR");

        // Assert
        Assert.Equal(3, deletedCount);

        // Verify ERROR events were deleted
        foreach (var logEvent in logEventsToDelete)
        {
            var retrieved = await repository.FindAsync(logEvent.RowKey, logEvent.PartitionKey);
            Assert.Null(retrieved);
        }

        // Verify INFO events were kept
        foreach (var logEvent in logEventsToKeep)
        {
            var retrieved = await repository.FindAsync(logEvent.RowKey, logEvent.PartitionKey);
            Assert.NotNull(retrieved);
        }
    }

    [Fact]
    public async Task DeleteBatchAsync_ByODataFilter_Should_Delete_Matching_LogEvents()
    {
        // Arrange
        var repository = Services.GetRequiredService<ITableRepository<LogEvent>>();
        var partitionKey = "TestPartition6";
        var logEventsToDelete = CreateTestLogEvents(2, partitionKey, "WARNING");
        var logEventsToKeep = CreateTestLogEvents(3, partitionKey, "DEBUG");

        var allLogEvents = new List<LogEvent>();
        allLogEvents.AddRange(logEventsToDelete);
        allLogEvents.AddRange(logEventsToKeep);

        // Create all entities
        await repository.CreateBatchAsync(allLogEvents);

        // Act - Delete only WARNING level events using OData filter
        var deletedCount = await repository.DeleteBatchAsync("Level eq 'WARNING'");

        // Assert
        Assert.Equal(2, deletedCount);

        // Verify WARNING events were deleted
        foreach (var logEvent in logEventsToDelete)
        {
            var retrieved = await repository.FindAsync(logEvent.RowKey, logEvent.PartitionKey);
            Assert.Null(retrieved);
        }

        // Verify DEBUG events were kept
        foreach (var logEvent in logEventsToKeep)
        {
            var retrieved = await repository.FindAsync(logEvent.RowKey, logEvent.PartitionKey);
            Assert.NotNull(retrieved);
        }
    }

    [Fact]
    public async Task BatchAsync_Core_Should_Handle_All_Transaction_Types()
    {
        // Arrange
        var repository = Services.GetRequiredService<ITableRepository<LogEvent>>();
        var partitionKey = "TestPartition7";

        // Test Add operation
        var addEvents = CreateTestLogEvents(2, partitionKey);
        var addedCount = await repository.BatchAsync(addEvents, TableTransactionActionType.Add);
        Assert.Equal(2, addedCount);

        // Test UpdateReplace operation
        foreach (var logEvent in addEvents)
        {
            logEvent.Level = "UPDATED";
        }
        var updatedCount = await repository.BatchAsync(addEvents, TableTransactionActionType.UpdateReplace);
        Assert.Equal(2, updatedCount);

        // Test UpsertReplace operation (insert new and update existing)
        var upsertEvents = CreateTestLogEvents(1, partitionKey);
        upsertEvents.AddRange(addEvents.Take(1)); // Include one existing
        foreach (var existingEvent in upsertEvents.Where(e => addEvents.Any(a => a.RowKey == e.RowKey)))
        {
            existingEvent.Level = "UPSERTED";
        }
        var upsertedCount = await repository.BatchAsync(upsertEvents, TableTransactionActionType.UpsertReplace);
        Assert.Equal(2, upsertedCount);

        // Test Delete operation
        var deleteCount = await repository.BatchAsync(addEvents, TableTransactionActionType.Delete);
        Assert.Equal(2, deleteCount);

        // Verify entities are deleted
        foreach (var logEvent in addEvents)
        {
            var retrieved = await repository.FindAsync(logEvent.RowKey, logEvent.PartitionKey);
            Assert.Null(retrieved);
        }
    }

    [Fact]
    public async Task BatchAsync_Should_Handle_Large_Batches()
    {
        // Arrange
        var repository = Services.GetRequiredService<ITableRepository<LogEvent>>();
        var partitionKey = "TestPartition8";

        // Create 250 entities (more than the 100 limit per batch)
        var logEvents = CreateTestLogEvents(250, partitionKey);

        // Act
        var createdCount = await repository.CreateBatchAsync(logEvents);

        // Assert
        Assert.Equal(250, createdCount);

        // Verify a sample of entities were created
        var sampleEvents = logEvents.Take(10);
        foreach (var logEvent in sampleEvents)
        {
            var retrieved = await repository.FindAsync(logEvent.RowKey, logEvent.PartitionKey);
            Assert.NotNull(retrieved);
        }
    }

    [Fact]
    public async Task BatchAsync_Should_Handle_Multiple_Partitions()
    {
        // Arrange
        var repository = Services.GetRequiredService<ITableRepository<LogEvent>>();
        var logEvents = new List<LogEvent>();

        // Create entities across multiple partitions
        logEvents.AddRange(CreateTestLogEvents(5, "Partition1"));
        logEvents.AddRange(CreateTestLogEvents(5, "Partition2"));
        logEvents.AddRange(CreateTestLogEvents(5, "Partition3"));

        // Act
        var createdCount = await repository.CreateBatchAsync(logEvents);

        // Assert
        Assert.Equal(15, createdCount);

        // Verify entities in each partition
        var partition1Events = await repository.FindAllAsync(e => e.PartitionKey == "Partition1");
        var partition2Events = await repository.FindAllAsync(e => e.PartitionKey == "Partition2");
        var partition3Events = await repository.FindAllAsync(e => e.PartitionKey == "Partition3");

        Assert.True(partition1Events.Count >= 5);
        Assert.True(partition2Events.Count >= 5);
        Assert.True(partition3Events.Count >= 5);
    }

    [Fact]
    public async Task BatchAsync_Should_Return_Zero_For_Empty_Collection()
    {
        // Arrange
        var repository = Services.GetRequiredService<ITableRepository<LogEvent>>();
        var emptyList = new List<LogEvent>();

        // Act & Assert
        var createCount = await repository.CreateBatchAsync(emptyList);
        Assert.Equal(0, createCount);

        var updateCount = await repository.UpdateBatchAsync(emptyList);
        Assert.Equal(0, updateCount);

        var saveCount = await repository.SaveBatchAsync(emptyList);
        Assert.Equal(0, saveCount);

        var deleteCount = await repository.DeleteBatchAsync(emptyList);
        Assert.Equal(0, deleteCount);
    }

    private static List<LogEvent> CreateTestLogEvents(int count, string partitionKey, string level = "INFO")
    {
        var logEvents = new List<LogEvent>();
        var faker = new Bogus.Faker();

        for (int i = 0; i < count; i++)
        {
            logEvents.Add(new LogEvent
            {
                RowKey = Ulid.NewUlid().ToString(),
                PartitionKey = partitionKey,
                Level = level,
                MessageTemplate = faker.Lorem.Sentence(),
                RenderedMessage = faker.Lorem.Paragraph(),
                Exception = faker.Random.Bool(0.1f) ? faker.System.Exception().ToString() : null,
                Data = faker.Random.Bool(0.3f) ? faker.System.Random.Hash() : null
            });
        }

        return logEvents;
    }
}
