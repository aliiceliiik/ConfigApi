using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ConfigApi.Context.Caching;

public class RedisCacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly string _prefix;

    public RedisCacheService(IConnectionMultiplexer redis,
                             ILogger<RedisCacheService> logger,
                             string instanceName)
    {
        _redis = redis;
        _logger = logger;
        _prefix = instanceName;
    }

    public bool IsEnabled => _redis.IsConnected;

    private string Full(string key) => _prefix + key;

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _redis.GetDatabase().StringGetAsync(Full(key));

            if (value.IsNullOrEmpty) return default;

            return JsonSerializer.Deserialize<T>(value!, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis okuma basarisiz: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await _redis.GetDatabase().StringSetAsync(Full(key), json, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis yazma basarisiz: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _redis.GetDatabase().KeyDeleteAsync(Full(key));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis silme basarisiz: {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            var pattern = Full(prefix) + "*";
            var db = _redis.GetDatabase();

            foreach (var endpoint in _redis.GetEndPoints())
            {
                var server = _redis.GetServer(endpoint);
                if (!server.IsConnected || server.IsReplica) continue;

                await foreach (var key in server.KeysAsync(db.Database, pattern, 250))
                    await db.KeyDeleteAsync(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis toplu silme basarisiz: {Prefix}", prefix);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, TimeSpan ttl, Func<Task<T>> factory)
    {
        var cached = await GetAsync<T>(key);
        if (cached is not null) return cached;

        var value = await factory();

        if (value is not null)
            await SetAsync(key, value, ttl);

        return value;
    }
}
