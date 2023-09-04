namespace DDD.Api.Startup;

public static class CacheConfigurationExtension
{
    public static IServiceCollection AddCacheConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            var connectionString = configuration.GetConnectionString("Redis");
            options.Configuration = connectionString;
        });

        return services;
    }
}