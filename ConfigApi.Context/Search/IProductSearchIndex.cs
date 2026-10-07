using ConfigApi.Entities.Entities;

namespace ConfigApi.Context.Search;

public interface IProductSearchIndex
{
    bool IsEnabled { get; }

    Task<bool> PingAsync();
    Task EnsureIndexAsync();
    Task IndexAsync(Product product);
    Task IndexManyAsync(IEnumerable<Product> products);
    Task DeleteAsync(Guid productId);
    Task<int> ReindexAllAsync(IEnumerable<Product> products);
}
