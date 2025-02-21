using Azure.Data.Tables;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using TableStorage.Abstracts.Extensions;

namespace TableStorage.Abstracts;

/// <summary>
/// Extension methods of <see cref="IServiceCollection"/>
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the required services for table storage repository.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="nameOrConnectionString">The Azure Storage connection string or the name of connection string located in the application config.</param>
    /// <returns>The current service collection for all chaining</returns>
    /// <exception cref="System.ArgumentNullException">services - A service collection is required.</exception>
    public static IServiceCollection AddTableStorageRepository(this IServiceCollection services, string? nameOrConnectionString = default)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services), "A service collection is required.");

        // add ITableRepository as open generic
        services.TryAddSingleton(typeof(ITableRepository<>), typeof(TableRepository<>));

        if (nameOrConnectionString.IsNullOrWhiteSpace())
            return services;

        // only add if name included
        services.TryAddSingleton((serviceProvider) =>
        {
            var connectionString = serviceProvider.ResolveConnectionString(nameOrConnectionString);
            return new TableServiceClient(connectionString);
        });

        return services;
    }

    /// <summary>
    /// Adds the required services for table storage repository with the specified connection string .
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="nameOrConnectionString">The connection string or the name of connection string located in the application configuration.</param>
    /// <param name="serviceKey">The service key to register with if specified.</param>
    /// <returns>
    /// The same service collection so that multiple calls can be chained.
    /// </returns>
    public static IServiceCollection AddTableServiceClient(this IServiceCollection services, string nameOrConnectionString, object? serviceKey = null)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        if (nameOrConnectionString is null)
            throw new ArgumentNullException(nameof(nameOrConnectionString));

        if (serviceKey != null)
        {
            services.TryAddKeyedSingleton(
                serviceKey: serviceKey,
                implementationFactory: (sp, _) =>
                {
                    var connectionString = ResolveConnectionString(sp, nameOrConnectionString);
                    return new TableServiceClient(connectionString);
                }
            );
        }
        else
        {
            services.TryAddSingleton((serviceProvider) =>
            {
                var connectionString = ResolveConnectionString(serviceProvider, nameOrConnectionString);
                return new TableServiceClient(connectionString);
            });
        }

        return services;
    }

    /// <summary>
    /// Resolve the connection string from the specified <paramref name="nameOrConnectionString"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="nameOrConnectionString">The connection string or the name of connection string located in the application config.</param>
    /// <returns>The resolved connection string</returns>
    /// <exception cref="System.ArgumentNullException">The service provider is null.</exception>
    public static string ResolveConnectionString(this IServiceProvider services, string nameOrConnectionString)
    {
        var isConnectionString = nameOrConnectionString.IndexOfAny([';', '=']) > 0;
        if (isConnectionString)
            return nameOrConnectionString;

        var configuration = services.GetRequiredService<IConfiguration>();

        // first try connection strings collection
        var connectionString = configuration.GetConnectionString(nameOrConnectionString);
        if (connectionString.HasValue())
            return connectionString;

        // next try root collection
        connectionString = configuration[nameOrConnectionString];
        if (connectionString.HasValue())
            return connectionString;

        throw new ArgumentException($"Could not find connection string with name '{nameOrConnectionString}' in the application configuration", nameof(nameOrConnectionString));
    }
}
