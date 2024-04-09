#nullable disable

using Microsoft.Extensions.DependencyInjection;

using TableStorage.Abstracts.Tests.Models;

namespace TableStorage.Abstracts.Tests;

public class CommentRepositoryTest : DatabaseTestBase
{
    public CommentRepositoryTest(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
    {
    }

    [Fact]
    public async Task FullTest()
    {
        var generator = new Faker<Comment>()
            .RuleFor(p => p.RowKey, _ => Guid.NewGuid().ToString("N"))
            .RuleFor(p => p.PartitionKey, f => f.PickRandom(Constants.Owners))
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.OwnerId, (f, i) => i.PartitionKey);

        var item = generator.Generate();

        var repository = Services.GetRequiredService<ITableRepository<Comment>>();
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
}
