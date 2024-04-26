using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using TableStorage.Abstracts.Tests.Services;

using XUnit.Hosting;

namespace TableStorage.Abstracts.Tests;

public class DatabaseFixture : TestApplicationFixture
{
    public const string StorageConnectionName = "AzureStorage";

    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        var services = builder.Services;

        services.AddTableStorageRepository(StorageConnectionName);

        services.TryAddSingleton<IUserRepository, UserRepository>();
        services.TryAddSingleton<ILogEventRepository, LogEventRepository>();
    }
}
