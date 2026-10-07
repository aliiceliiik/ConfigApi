using ConfigApi.Entities.Entities;

namespace ConfigApi.Context.Search;

public class NullProductSearchIndex : IProductSearchIndex
{
    public bool IsEnabled => false;

    public Task<bool> PingAsync() => Task.FromResult(false);
    public Task EnsureIndexAsync() => Task.CompletedTask;
    public Task IndexAsync(Product product) => Task.CompletedTask;
    public Task IndexManyAsync(IEnumerable<Product> products) => Task.CompletedTask;
    public Task DeleteAsync(Guid productId) => Task.CompletedTask;
    public Task<int> ReindexAllAsync(IEnumerable<Product> products) => Task.FromResult(0);
}
