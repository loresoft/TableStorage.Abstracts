#nullable disable

using Azure.Data.Tables;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TableStorage.Abstracts.Tests.Models;

namespace TableStorage.Abstracts.Tests;

public class ServiceCollectionTest
{
    [Fact]
    public void ConnectionStringFromRootConfiguration()
    {
        var initialData = new Dictionary<string, string>
        {
            ["AzureStorage"] = "UseDevelopmentStorage=true"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .AddTableStorageRepository("AzureStorage");

        var service = serviceCollection.BuildServiceProvider();

        var tableServiceClient = service.GetRequiredService<TableServiceClient>();
        Assert.NotNull(tableServiceClient);
        Assert.Equal("devstoreaccount1", tableServiceClient.AccountName);

        var roleRepo = service.GetRequiredService<ITableRepository<Role>>();
        Assert.NotNull(roleRepo);
    }

    [Fact]
    public void ConnectionStringFromConfiguration()
    {
        var initialData = new Dictionary<string, string>
        {
            ["ConnectionStrings:AzureStorage"] = "UseDevelopmentStorage=true"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .AddTableStorageRepository("AzureStorage");

        var service = serviceCollection.BuildServiceProvider();

        var tableServiceClient = service.GetRequiredService<TableServiceClient>();
        Assert.NotNull(tableServiceClient);
        Assert.Equal("devstoreaccount1", tableServiceClient.AccountName);

        var roleRepo = service.GetRequiredService<ITableRepository<Role>>();
        Assert.NotNull(roleRepo);
    }

    [Fact]
    public void ConnectionStringPassedIn()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .AddTableStorageRepository("UseDevelopmentStorage=true");

        var service = serviceCollection.BuildServiceProvider();

        var tableServiceClient = service.GetRequiredService<TableServiceClient>();
        Assert.NotNull(tableServiceClient);
        Assert.Equal("devstoreaccount1", tableServiceClient.AccountName);

        var roleRepo = service.GetRequiredService<ITableRepository<Role>>();
        Assert.NotNull(roleRepo);
    }


    [Fact]
    public void ConnectionStringNotSet()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .AddTableStorageRepository();

        var service = serviceCollection.BuildServiceProvider();

        var tableServiceClient = service.GetService<TableServiceClient>();
        Assert.Null(tableServiceClient);
    }

    [Fact]
    public void ConnectionStringNotSetManualRegister()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .AddTableStorageRepository()
            .AddSingleton(sp => new TableServiceClient("UseDevelopmentStorage=true"));

        var service = serviceCollection.BuildServiceProvider();

        var tableServiceClient = service.GetRequiredService<TableServiceClient>();
        Assert.NotNull(tableServiceClient);
        Assert.Equal("devstoreaccount1", tableServiceClient.AccountName);

        var roleRepo = service.GetRequiredService<ITableRepository<Role>>();
        Assert.NotNull(roleRepo);
    }

    [Fact]
    public void ConnectionStringFromConfigurationNotFound()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .AddTableStorageRepository("AzureStorage");

        var service = serviceCollection.BuildServiceProvider();

        var action = service.GetRequiredService<TableServiceClient>;
        Assert.Throws<ArgumentException>(action);
    }
}
