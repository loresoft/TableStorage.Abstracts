#nullable disable

using Microsoft.Extensions.DependencyInjection;

using TableStorage.Abstracts.Tests.Models;

namespace TableStorage.Abstracts.Tests;

public class TemplateRepositoryTest : DatabaseTestBase
{
    public TemplateRepositoryTest(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
    {
    }

    [Fact]
    public async Task FullTest()
    {
        var generator = new Faker<Template>()
            .RuleFor(p => p.RowKey, _ => Ulid.NewUlid().ToString())
            .RuleFor(p => p.PartitionKey, f => f.PickRandom(Constants.Owners))
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.OwnerId, (f, i) => i.PartitionKey);

        var item = generator.Generate();

        var repository = Services.GetRequiredService<ITableRepository<Template>>();
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
        var updatedName = "Big " + readResult.Name;
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
        Assert.True((queryResults.Count) > (0));

        // delete
        await repository.DeleteAsync(readResult);

        var deletedResult = await repository.FindAsync(item.RowKey, item.PartitionKey);
        Assert.Null(deletedResult);
    }
}
