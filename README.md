# EF Core 3.1.x Second Level Cache Interceptor

<p align="left">
  <a href="https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor">
     <img alt="GitHub Actions status" src="https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/workflows/.NET%20Core%20Build/badge.svg">
  </a>
</p>

## Entity Framework Core 3.1.x Second Level Caching Library

Second level caching is a query cache. The results of EF commands will be stored in the cache, so that the same EF commands will retrieve their data from the cache rather than executing them against the database again.

## Install via NuGet

To install EFCoreSecondLevelCacheInterceptor, run the following command in the Package Manager Console:

[![Nuget](https://img.shields.io/nuget/v/EFCoreSecondLevelCacheInterceptor)](https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor)

```powershell
PM> Install-Package EFCoreSecondLevelCacheInterceptor
```

You can also view the [package page](http://www.nuget.org/packages/EFCoreSecondLevelCacheInterceptor/) on NuGet.

## Usage

### 1- [Register a preferred cache provider](/src/Tests/EFCoreSecondLevelCacheInterceptor.AspNetCoreSample/Startup.cs):

#### Using the built-in In-Memory cache provider

```csharp
namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample
{
    public class Startup
    {
        private readonly string _contentRootPath;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _contentRootPath = env.ContentRootPath;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEFSecondLevelCache(options =>
                options.UseMemoryCacheProvider().DisableLogging(true)

            // Please use the `CacheManager.Core` or `EasyCaching.Redis` for the Redis cache provider.
            );

            var connectionString = Configuration["ConnectionStrings:ApplicationDbContextConnection"];
            if (connectionString.Contains("%CONTENTROOTPATH%"))
            {
                connectionString = connectionString.Replace("%CONTENTROOTPATH%", _contentRootPath);
            }
            services.AddConfiguredMsSqlDbContext(connectionString);

            services.AddControllersWithViews();
        }
    }
}
```

### Using EasyCaching.Core as the cache provider

Here you can use the [EasyCaching.Core](https://github.com/dotnetcore/EasyCaching), as a highly configurable cache manager too.
To use its in-memory caching mechanism, add this entry to the `.csproj` file:

```xml
  <ItemGroup>
    <PackageReference Include="EasyCaching.InMemory" Version="0.8.8" />
  </ItemGroup>
```

Then register its required services:

```csharp
namespace EFSecondLevelCache.Core.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            const string providerName1 = "InMemory1";
            services.AddEFSecondLevelCache(options =>
                    options.UseEasyCachingCoreProvider(providerName1).DisableLogging(true)
            );

            // Add an in-memory cache service provider
            // More info: https://easycaching.readthedocs.io/en/latest/In-Memory/
            services.AddEasyCaching(options =>
            {
                // use memory cache with your own configuration
                options.UseInMemory(config =>
                {
                    config.DBConfig = new InMemoryCachingOptions
                    {
                        // scan time, default value is 60s
                        ExpirationScanFrequency = 60,
                        // total count of cache items, default value is 10000
                        SizeLimit = 100,

                        // enable deep clone when reading object from cache or not, default value is true.
                        EnableReadDeepClone = false,
                        // enable deep clone when writing object to cache or not, default value is false.
                        EnableWriteDeepClone = false,
                    };
                    // the max random second will be added to cache's expiration, default value is 120
                    config.MaxRdSecond = 120;
                    // whether enable logging, default is false
                    config.EnableLogging = false;
                    // mutex key's alive time(ms), default is 5000
                    config.LockMs = 5000;
                    // when mutex key alive, it will sleep some time, default is 300
                    config.SleepMs = 300;
                }, providerName);
            });
        }
    }
}
```

If you want to use the Redis as the preferred cache provider with `EasyCaching.Core`, first install the following package:

```xml
  <ItemGroup>
    <PackageReference Include="EasyCaching.Redis" Version="0.8.8" />
  </ItemGroup>
```

And then register its required services:

```csharp
namespace EFSecondLevelCache.Core.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            const string providerName1 = "Redis1";
            services.AddEFSecondLevelCache(options =>
                    options.UseEasyCachingCoreProvider(providerName1).DisableLogging(true)
            );

            // More info: https://easycaching.readthedocs.io/en/latest/Redis/
            services.AddEasyCaching(option =>
            {
                option.UseRedis(config =>
                {
                    config.DBConfig.AllowAdmin = true;
                    config.DBConfig.Endpoints.Add(new EasyCaching.Core.Configurations.ServerEndPoint("127.0.0.1", 6379));
                }, providerName);
            });
        }
    }
}
```

### Using CacheManager.Core as the cache provider [It's not actively maintained]

Also here you can use the [CacheManager.Core](https://github.com/MichaCo/CacheManager), as a highly configurable cache manager too.
To use its in-memory caching mechanism, add these entries to the `.csproj` file:

```xml
  <ItemGroup>
    <PackageReference Include="CacheManager.Core" Version="1.2.0" />
    <PackageReference Include="CacheManager.Microsoft.Extensions.Caching.Memory" Version="1.2.0" />
    <PackageReference Include="CacheManager.Serialization.Json" Version="1.2.0" />
  </ItemGroup>
```

Then register its required services:

```csharp
namespace EFSecondLevelCache.Core.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEFSecondLevelCache(options =>
                options.UseCacheManagerCoreProvider().DisableLogging(true)
            );

            // Add an in-memory cache service provider
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            services.AddSingleton(typeof(ICacheManagerConfiguration),
                new CacheManager.Core.ConfigurationBuilder()
                        .WithJsonSerializer()
                        .WithMicrosoftMemoryCacheHandle(instanceName: "MemoryCache1")
                        .Build());
        }
    }
}
```

If you want to use the Redis as the preferred cache provider with `CacheManager.Core`, first install the `CacheManager.StackExchange.Redis` package and then register its required services:

```csharp
// Add Redis cache service provider
var jss = new JsonSerializerSettings
{
    NullValueHandling = NullValueHandling.Ignore,
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    TypeNameHandling = TypeNameHandling.Auto,
    Converters = { new SpecialTypesConverter() }
};

const string redisConfigurationKey = "redis";
services.AddSingleton(typeof(ICacheManagerConfiguration),
    new CacheManager.Core.ConfigurationBuilder()
        .WithJsonSerializer(serializationSettings: jss, deserializationSettings: jss)
        .WithUpdateMode(CacheUpdateMode.Up)
        .WithRedisConfiguration(redisConfigurationKey, config =>
        {
            config.WithAllowAdmin()
                .WithDatabase(0)
                .WithEndpoint("localhost", 6379)
                // Enables keyspace notifications to react on eviction/expiration of items.
                // Make sure that all servers are configured correctly and 'notify-keyspace-events' is at least set to 'Exe', otherwise CacheManager will not retrieve any events.
                // You can try 'Egx' or 'eA' value for the `notify-keyspace-events` too.
                // See https://redis.io/topics/notifications#configuration for configuration details.
                .EnableKeyspaceEvents();
        })
        .WithMaxRetries(100)
        .WithRetryTimeout(50)
        .WithRedisCacheHandle(redisConfigurationKey)
        .Build());
services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));

services.AddEFSecondLevelCache(options =>
    options.UseCacheManagerCoreProvider().DisableLogging(true)
);
```

[Here is](/src/Tests/EFCoreSecondLevelCacheInterceptor.Tests/Settings/EFServiceProvider.cs#L21) the definition of the SpecialTypesConverter.

### Using a custom cache provider

If you don't want to use the above cache providers, implement your custom `IEFCacheServiceProvider` and then introduce it using the `options.UseCustomCacheProvider<T>()` method.

### 2- [Add SecondLevelCacheInterceptor](/src/Tests/EFCoreSecondLevelCacheInterceptor.Tests.DataLayer/MsSqlServiceCollectionExtensions.cs) to your `DbContextOptionsBuilder` pipeline:

```csharp
    public static class MsSqlServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguredMsSqlDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContextPool<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
                    optionsBuilder
                        .UseSqlServer(
                            connectionString,
                            sqlServerOptionsBuilder =>
                            {
                                sqlServerOptionsBuilder
                                    .CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds)
                                    .EnableRetryOnFailure()
                                    .MigrationsAssembly(typeof(MsSqlServiceCollectionExtensions).Assembly.FullName);
                            })
                        .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>()));
            return services;
        }
    }
```

### 3- Setting up the cache invalidation:

This library doesn't need any settings for the cache invalidation. It watches for all of the CRUD operations using its interceptor and then invalidates the related cache entries automatically.
But if you want to invalidate the whole cache manually, inject the `IEFCacheServiceProvider` service and then call its `ClearAllCachedEntries()` method.

### 4- To cache the results of the normal queries like:

```csharp
var post1 = context.Posts
                   .Where(x => x.Id > 0)
                   .OrderBy(x => x.Id)
                   .FirstOrDefault();
```

We can use the new `Cacheable()` extension method:

```csharp
var post1 = context.Posts
                   .Where(x => x.Id > 0)
                   .OrderBy(x => x.Id)
                   .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                   .FirstOrDefault();  // Async methods are supported too.
```

NOTE: It doesn't matter where the `Cacheable` method is located in this expression tree. [It just adds](/src/EFCoreSecondLevelCacheInterceptor/EFCachedQueryExtensions.cs) the standard `TagWith` method to mark this query as `Cacheable`. Later `SecondLevelCacheInterceptor` will use this tag to identify the `Cacheable` queries.

Also it's possible to set the `Cacheable()` method's settings globally:

```csharp
services.AddEFSecondLevelCache(options => options.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(5)).DisableLogging(true));
```

In this case the above query will become:

```csharp
var post1 = context.Posts
                   .Where(x => x.Id > 0)
                   .OrderBy(x => x.Id)
                   .Cacheable()
                   .FirstOrDefault();  // Async methods are supported too.
```

If you specify the settings of the `Cacheable()` method explicitly such as `Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))`, its setting will override the global setting.

## Caching all of the queries

To cache all of the system's queries, just set the `CacheAllQueries()` method:

```csharp
namespace EFCoreSecondLevelCacheInterceptor.AspNetCoreSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEFSecondLevelCache(options =>
            {
                options.UseMemoryCacheProvider().DisableLogging(true);
                options.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30));
            });

            // ...
```

This will put the whole system's queries in cache. In this case calling the `Cacheable()` methods won't be necessary. If you specify the `Cacheable()` method, its setting will override this global setting. If you want to exclude some of the queries from this global cache, apply the `NotCacheable()` method to them.

## Samples

- [Console App Sample](/src/Tests/EFCoreSecondLevelCacheInterceptor.ConsoleSample/)
- [ASP.NET Core App Sample](/src/Tests/EFCoreSecondLevelCacheInterceptor.AspNetCoreSample/)
- [Tests](/src/Tests/EFCoreSecondLevelCacheInterceptor.Tests/)

## Guidance

### When to use

Good candidates for query caching are global site settings and public data, such as infrequently changing articles or comments. It can also be beneficial to cache data specific to a user so long as the cache expires frequently enough relative to the size of the user base that memory consumption remains acceptable. Small, per-user data that frequently exceeds the cache's lifetime, such as a user's photo path, is better held in user claims, which are stored in cookies, than in this cache.

### Scope

This cache is scoped to the application, not the current user. It does not use session variables. Accordingly, when retrieving cached per-user data, be sure queries in include code such as `.Where(x => .... && x.UserId == id)`.

### Invalidation

This cache is updated when an entity is changed (insert, update, or delete) via a DbContext that uses this library. If the database is updated through some other means, such as a stored procedure or trigger, the cache becomes stale.
