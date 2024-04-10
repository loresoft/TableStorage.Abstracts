#nullable disable

using Bogus;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using TableStorage.Abstracts.Extensions;
using TableStorage.Abstracts.Tests.Models;

using Xunit.Abstractions;

namespace TableStorage.Abstracts.Tests;

public class ItemRepositoryTest : DatabaseTestBase
{
    public ItemRepositoryTest(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
    {
    }

    [Fact]
    public async Task FullTest()
    {
        var generator = CreateGenerator();

        var item = generator.Generate();

        var repository = Services.GetRequiredService<ITableRepository<Item>>();
        repository.Should().NotBeNull();

        // create
        var createResult = await repository.CreateAsync(item);
        createResult.Should().NotBeNull();
        createResult.RowKey.Should().Be(item.RowKey);

        // read
        var readResult = await repository.FindAsync(item.RowKey, item.PartitionKey);
        readResult.Should().NotBeNull();
        readResult.RowKey.Should().Be(item.RowKey);
        readResult.OwnerId.Should().Be(item.OwnerId);

        // update
        string updatedName = "Big " + readResult.Name;
        readResult.Name = updatedName;

        var updateResult = await repository.UpdateAsync(readResult);
        updateResult.Should().NotBeNull();
        updateResult.RowKey.Should().Be(item.RowKey);
        updateResult.OwnerId.Should().Be(item.OwnerId);

        // query
        var queryResult = await repository.FindOneAsync(r => r.Name == updatedName);
        queryResult.Should().NotBeNull();

        var queryResults = await repository.FindAllAsync(r => r.Name == updatedName);
        queryResults.Should().NotBeNull();
        queryResults.Count.Should().BeGreaterThan(0);

        // delete
        await repository.DeleteAsync(readResult);

        var deletedResult = await repository.FindAsync(item.RowKey, item.PartitionKey);
        deletedResult.Should().BeNull();
    }


    [Fact]
    public async Task BatchTest()
    {
        var generator = CreateGenerator();
        var items = generator.Generate(1000);

        var repository = Services.GetRequiredService<ITableRepository<Item>>();
        repository.Should().NotBeNull();

        await repository.BatchAsync(items);
    }

    [Fact]
    public async Task PagingTest()
    {
        var generator = CreateGenerator();
        var items = generator.Generate(1000);

        var repository = Services.GetRequiredService<ITableRepository<Item>>();
        repository.Should().NotBeNull();

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
        repository.Should().NotBeNull();

        await repository.BatchAsync(items);

        var queryResults = await repository.FindAllAsync(r => r.OwnerId == Constants.Owners[0]);
        queryResults.Should().NotBeNull();
        queryResults.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task LargeResultOne()
    {
        var generator = CreateGenerator();
        var items = generator.Generate(1000);

        var repository = Services.GetRequiredService<ITableRepository<Item>>();
        repository.Should().NotBeNull();

        await repository.BatchAsync(items);

        var queryResults = await repository.FindOneAsync(r => r.OwnerId == Constants.Owners[0]);
        queryResults.Should().NotBeNull();
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
