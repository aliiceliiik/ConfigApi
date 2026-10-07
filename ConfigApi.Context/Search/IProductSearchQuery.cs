using ConfigApi.Entities.Dtos;

namespace ConfigApi.Context.Search;

public interface IProductSearchQuery
{
    bool IsEnabled { get; }

    Task<(IEnumerable<ProductListItemDto> items, int total)> SearchAsync(ProductSearchRequest request);
    Task<IEnumerable<string>> SuggestAsync(string prefix, int size);
}
