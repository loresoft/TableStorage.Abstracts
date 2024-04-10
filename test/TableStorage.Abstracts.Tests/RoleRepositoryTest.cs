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
        roleRepo.Should().NotBeNull();

        var result = await roleRepo.CreateAsync(role);
        result.Should().NotBeNull();
        result.RowKey.Should().Be(role.RowKey);
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
        roleRepo.Should().NotBeNull();

        var result = await roleRepo.SaveAsync(role);
        result.Should().NotBeNull();
        result.RowKey.Should().Be(role.RowKey);
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
        roleRepo.Should().NotBeNull();

        var createResult = await roleRepo.CreateAsync(role);
        createResult.Should().NotBeNull();
        createResult.RowKey.Should().Be(role.RowKey);

        createResult.Name = "CreateUpdateRole";
        createResult.NormalizedName = "createupdaterole";

        var updateResult = await roleRepo.UpdateAsync(createResult);
        updateResult.Should().NotBeNull();
        updateResult.RowKey.Should().Be(role.RowKey);
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
        roleRepo.Should().NotBeNull();

        var createResult = await roleRepo.CreateAsync(role);
        createResult.Should().NotBeNull();
        createResult.RowKey.Should().Be(role.RowKey);

        var readResult = await roleRepo.FindAsync(role.RowKey, role.PartitionKey);
        readResult.Should().NotBeNull();
        readResult.RowKey.Should().Be(role.RowKey);
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
        roleRepo.Should().NotBeNull();

        var createResult = await roleRepo.CreateAsync(role);
        createResult.Should().NotBeNull();
        createResult.RowKey.Should().Be(role.RowKey);

        await roleRepo.DeleteAsync(role.RowKey, role.PartitionKey);

        var findResult = await roleRepo.FindAsync(role.RowKey, role.PartitionKey);
        findResult.Should().BeNull();
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
        roleRepo.Should().NotBeNull();

        var createResult = await roleRepo.CreateAsync(role);
        createResult.Should().NotBeNull();
        createResult.RowKey.Should().Be(role.RowKey);

        var results = await roleRepo.FindAllAsync(r => r.Name == "CreateRole");
        results.Should().NotBeNull();
        results.Count.Should().BeGreaterThan(0);
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
        roleRepo.Should().NotBeNull();

        var createResult = await roleRepo.CreateAsync(role);
        createResult.Should().NotBeNull();
        createResult.RowKey.Should().Be(role.RowKey);

        var findResult = await roleRepo.FindOneAsync(r => r.Name == "CreateRole");
        findResult.Should().NotBeNull();
    }

    [Fact]
    public async Task FindAllEmpty()
    {
        var roleRepo = Services.GetRequiredService<ITableRepository<Role>>();
        roleRepo.Should().NotBeNull();

        var results = await roleRepo.FindAllAsync(r => r.Name == "blah" + DateTime.Now.Ticks);
        results.Should().NotBeNull();
        results.Count.Should().Be(0);
    }

}
