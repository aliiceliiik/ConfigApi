namespace ConfigApi.Context.Caching;

public class NullCacheService : ICacheService
{
    public bool IsEnabled => false;

    public Task<T?> GetAsync<T>(string key) => Task.FromResult<T?>(default);

    public Task SetAsync<T>(string key, T value, TimeSpan ttl) => Task.CompletedTask;

    public Task RemoveAsync(string key) => Task.CompletedTask;

    public Task RemoveByPrefixAsync(string prefix) => Task.CompletedTask;

    public async Task<T> GetOrSetAsync<T>(string key, TimeSpan ttl, Func<Task<T>> factory)
        => await factory();
}
