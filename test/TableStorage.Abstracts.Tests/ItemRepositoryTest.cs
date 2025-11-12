using Microsoft.Extensions.DependencyInjection;

using TableStorage.Abstracts.Extensions;
using TableStorage.Abstracts.Tests.Models;

namespace TableStorage.Abstracts.Tests;

public class ItemRepositoryTest(DatabaseFixture databaseFixture) : DatabaseTestBase(databaseFixture)
{
    [Fact]
    public async Task FullTest()
    {
        var generator = CreateGenerator();

        var item = generator.Generate();

        var repository = Services.GetRequiredService<ITableRepository<Item>>();
        Assert.NotNull(repository);

        // create
        var createResult = await repository.CreateAsync(item);
        Assert.NotNull(createResult);
        Assert.Equal(item.RowKey, createResult.RowKey);

        // read
        var readResult = await repository.FindAsync(item.RowKey, item.PartitionKey);
        Assert.NotNull(readResult);
        Assert.Equal(item.RowKey, readResult.RowKey);
        Assert.Equal(item.OwnerId, readResult.OwnerId);

        // update
        string updatedName = "Big " + readResult.Name;
        readResult.Name = updatedName;

        var updateResult = await repository.UpdateAsync(readResult);
        Assert.NotNull(updateResult);
        Assert.Equal(item.RowKey, updateResult.RowKey);
        Assert.Equal(item.OwnerId, updateResult.OwnerId);

        // query
        var queryResult = await repository.FindOneAsync(r => r.Name == updatedName);
        Assert.NotNull(queryResult);

        var queryResults = await repository.FindAllAsync(r => r.Name == updatedName);
        Assert.NotNull(queryResults);
        Assert.True(queryResults.Count > 0);

        // delete
        await repository.DeleteAsync(readResult);

        var deletedResult = await repository.FindAsync(item.RowKey, item.PartitionKey);
        Assert.Null(deletedResult);
    }


    [Fact]
    public async Task BatchTest()
    {
        var generator = CreateGenerator();
        var items = generator.Generate(1000);

        var repository = Services.GetRequiredService<ITableRepository<Item>>();
        Assert.NotNull(repository);

        await repository.BatchAsync(items);
    }

    [Fact]
    public async Task PagingTest()
    {
        var generator = CreateGenerator();
        var items = generator.Generate(1000);

        var repository = Services.GetRequiredService<ITableRepository<Item>>();
        Assert.NotNull(repository);

        await repository.BatchAsync(items);

        var pageResult = await repository.FindPageAsync(pageSize: 20);
        for (var page = 0; pageResult.ContinuationToken.HasValue() || page < 2; page++)
        {
            pageResult = await repository.FindPageAsync(continuationToken: pageResult.ContinuationToken, pageSize: 20);
        }
    }

    [Fact]
    public async Task LargeResult()
    {
        var generator = CreateGenerator();
        var items = generator.Generate(1000);

        var repository = Services.GetRequiredService<ITableRepository<Item>>();
        Assert.NotNull(repository);

        await repository.BatchAsync(items);

        var queryResults = await repository.FindAllAsync(r => r.OwnerId == Constants.Owners[0]);
        Assert.NotNull(queryResults);
        Assert.True(queryResults.Count > 0);
    }

    [Fact]
    public async Task LargeResultOne()
    {
        var generator = CreateGenerator();
        var items = generator.Generate(1000);

        var repository = Services.GetRequiredService<ITableRepository<Item>>();
        Assert.NotNull(repository);

        await repository.BatchAsync(items);

        var queryResults = await repository.FindOneAsync(r => r.OwnerId == Constants.Owners[0]);
        Assert.NotNull(queryResults);
    }

    private static Faker<Item> CreateGenerator()
    {
        return new Faker<Item>()
            .RuleFor(p => p.RowKey, _ => Ulid.NewUlid().ToString())
            .RuleFor(p => p.PartitionKey, f => f.PickRandom(Constants.Owners))
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.OwnerId, (f, i) => i.PartitionKey);
    }

}
