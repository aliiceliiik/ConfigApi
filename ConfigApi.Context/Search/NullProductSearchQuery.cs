using ConfigApi.Entities.Dtos;

namespace ConfigApi.Context.Search;

public class NullProductSearchQuery : IProductSearchQuery
{
    public bool IsEnabled => false;

    public Task<(IEnumerable<ProductListItemDto> items, int total)> SearchAsync(ProductSearchRequest request)
        => Task.FromResult<(IEnumerable<ProductListItemDto>, int)>((Array.Empty<ProductListItemDto>(), 0));

    public Task<IEnumerable<string>> SuggestAsync(string prefix, int size)
        => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
}
