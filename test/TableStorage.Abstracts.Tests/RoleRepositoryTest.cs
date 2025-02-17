#nullable disable

using Microsoft.Extensions.DependencyInjection;

using TableStorage.Abstracts.Tests.Models;

namespace TableStorage.Abstracts.Tests;

public class RoleRepositoryTest : DatabaseTestBase
{
    public RoleRepositoryTest(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
    {
    }

    [Fact]
    public async Task CreateRole()
    {
        var role = new Role
        {
            RowKey = Ulid.NewUlid().ToString(),
            Name = "CreateRole",
            NormalizedName = "createrole",
            Claims = [new Claim { Type = "Test", Value = "testing" }]
        };

        var roleRepo = Services.GetRequiredService<ITableRepository<Role>>();
        Assert.NotNull(roleRepo);

        var result = await roleRepo.CreateAsync(role);
        Assert.NotNull(result);
        Assert.Equal(role.RowKey, result.RowKey);
    }

    [Fact]
    public async Task SaveRole()
    {
        var role = new Role
        {
            RowKey = Ulid.NewUlid().ToString(),
            Name = "SaveRole",
            NormalizedName = "saverole"
        };

        var roleRepo = Services.GetRequiredService<ITableRepository<Role>>();
        Assert.NotNull(roleRepo);

        var result = await roleRepo.SaveAsync(role);
        Assert.NotNull(result);
        Assert.Equal(role.RowKey, result.RowKey);
    }

    [Fact]
    public async Task CreateUpdateRole()
    {
        var role = new Role
        {
            RowKey = Ulid.NewUlid().ToString(),
            Name = "CreateRole",
            NormalizedName = "createrole"
        };

        var roleRepo = Services.GetRequiredService<ITableRepository<Role>>();
        Assert.NotNull(roleRepo);

        var createResult = await roleRepo.CreateAsync(role);
        Assert.NotNull(createResult);
        Assert.Equal(role.RowKey, createResult.RowKey);

        createResult.Name = "CreateUpdateRole";
        createResult.NormalizedName = "createupdaterole";

        var updateResult = await roleRepo.UpdateAsync(createResult);
        Assert.NotNull(updateResult);
        Assert.Equal(role.RowKey, updateResult.RowKey);
    }

    [Fact]
    public async Task CreateReadRole()
    {
        var role = new Role
        {
            RowKey = Ulid.NewUlid().ToString(),
            Name = "CreateReadRole",
            NormalizedName = "createreadrole"
        };

        var roleRepo = Services.GetRequiredService<ITableRepository<Role>>();
        Assert.NotNull(roleRepo);

        var createResult = await roleRepo.CreateAsync(role);
        Assert.NotNull(createResult);
        Assert.Equal(role.RowKey, createResult.RowKey);

        var readResult = await roleRepo.FindAsync(role.RowKey, role.PartitionKey);
        Assert.NotNull(readResult);
        Assert.Equal(role.RowKey, readResult.RowKey);
    }

    [Fact]
    public async Task CreateDeleteRole()
    {
        var role = new Role
        {
            Name = "CreateDeleteRole",
            NormalizedName = "createdeleterole"
        };

        var roleRepo = Services.GetRequiredService<ITableRepository<Role>>();
        Assert.NotNull(roleRepo);

        var createResult = await roleRepo.CreateAsync(role);
        Assert.NotNull(createResult);
        Assert.Equal(role.RowKey, createResult.RowKey);

        await roleRepo.DeleteAsync(role.RowKey, role.PartitionKey);

        var findResult = await roleRepo.FindAsync(role.RowKey, role.PartitionKey);
        Assert.Null(findResult);
    }

    [Fact]
    public async Task FindAllStartsWith()
    {
        var role = new Role
        {
            Name = "CreateRole",
            NormalizedName = "createrole"
        };

        var roleRepo = Services.GetRequiredService<ITableRepository<Role>>();
        Assert.NotNull(roleRepo);

        var createResult = await roleRepo.CreateAsync(role);
        Assert.NotNull(createResult);
        Assert.Equal(role.RowKey, createResult.RowKey);

        var results = await roleRepo.FindAllAsync(r => r.Name == "CreateRole");
        Assert.NotNull(results);
        Assert.True((results.Count) > (0));
    }

    [Fact]
    public async Task FindOneStartsWith()
    {
        var role = new Role
        {
            Name = "CreateRole",
            NormalizedName = "createrole"
        };

        var roleRepo = Services.GetRequiredService<ITableRepository<Role>>();
        Assert.NotNull(roleRepo);

        var createResult = await roleRepo.CreateAsync(role);
        Assert.NotNull(createResult);
        Assert.Equal(role.RowKey, createResult.RowKey);

        var findResult = await roleRepo.FindOneAsync(r => r.Name == "CreateRole");
        Assert.NotNull(findResult);
    }

    [Fact]
    public async Task FindAllEmpty()
    {
        var roleRepo = Services.GetRequiredService<ITableRepository<Role>>();
        Assert.NotNull(roleRepo);

        var results = await roleRepo.FindAllAsync(r => r.Name == "blah" + DateTime.Now.Ticks);
        Assert.NotNull(results);
        Assert.Empty(results);
    }

}
