using ConfigApi.Context.Repositories;
using ConfigApi.Context.Search;

namespace ConfigApi.Api.Seed;

public static class SearchIndexSeeder
{
    public static async Task SeedAsync(IProductSearchIndex index,
                                       IProductRepository products,
                                       ILogger logger)
    {
        if (!index.IsEnabled) return;

        if (!await index.PingAsync())
        {
            logger.LogWarning("Elasticsearch erisilemiyor, indeksleme atlandi.");
            return;
        }

        await index.EnsureIndexAsync();

        var all = (await products.GetAllForIndexingAsync()).ToList();
        var count = await index.ReindexAllAsync(all);

        logger.LogInformation("Elasticsearch indekslendi: {Count} urun", count);
    }
}
