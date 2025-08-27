using Azure.Data.Tables;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using TableStorage.Abstracts.Extensions;

namespace TableStorage.Abstracts;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to register Azure Table Storage services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the required services for Azure Table Storage repository pattern to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="nameOrConnectionString">The Azure Storage connection string, or the name of a connection string located in the application configuration. If null or empty, only the repository interface is registered.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method registers the following services:
    /// </para>
    /// <list type="bullet">
    /// <item><description><see cref="ITableRepository{TEntity}"/> as a singleton open generic type</description></item>
    /// <item><description><see cref="TableServiceClient"/> as a singleton (if connection string is provided)</description></item>
    /// </list>
    /// <para>
    /// If <paramref name="nameOrConnectionString"/> is provided, it will be resolved as either:
    /// </para>
    /// <list type="number">
    /// <item><description>A direct connection string (if it contains ';' or '=' characters)</description></item>
    /// <item><description>A connection string name from the application configuration</description></item>
    /// </list>
    /// </remarks>
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
    /// Adds an Azure Table Service client to the service collection with the specified connection string.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="nameOrConnectionString">The Azure Storage connection string, or the name of a connection string located in the application configuration.</param>
    /// <param name="serviceKey">The service key to register the client with for keyed services. If null, registers as a regular singleton.</param>
    /// <returns>
    /// The same service collection so that multiple calls can be chained.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="nameOrConnectionString"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method allows registration of multiple <see cref="TableServiceClient"/> instances using keyed services,
    /// enabling scenarios where different clients are needed for different storage accounts or configurations.
    /// </para>
    /// <para>
    /// The connection string resolution follows the same logic as <see cref="AddTableStorageRepository"/>:
    /// direct connection strings are used as-is, while names are resolved from application configuration.
    /// </para>
    /// </remarks>
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
    /// Resolves a connection string from the specified name or returns the string as-is if it's already a connection string.
    /// </summary>
    /// <param name="services">The service provider to resolve configuration services from.</param>
    /// <param name="nameOrConnectionString">The connection string or the name of a connection string located in the application configuration.</param>
    /// <returns>The resolved connection string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// The connection string name could not be found in the application configuration.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method uses the following resolution strategy:
    /// </para>
    /// <list type="number">
    /// <item><description>If the input contains ';' or '=' characters, it's treated as a direct connection string and returned as-is</description></item>
    /// <item><description>Otherwise, it's treated as a configuration key and resolved using <see cref="IConfiguration.GetConnectionString(string)"/></description></item>
    /// <item><description>If not found in connection strings, it's searched in the root configuration using <see cref="IConfiguration.this[string]"/></description></item>
    /// <item><description>If still not found, an <see cref="ArgumentException"/> is thrown</description></item>
    /// </list>
    /// <para>
    /// This flexible approach allows for both direct connection strings and configuration-based resolution,
    /// supporting various deployment and configuration scenarios.
    /// </para>
    /// </remarks>
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
