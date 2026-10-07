using System.Net;
using System.Text;
using System.Text.Json;
using ConfigApi.Entities.Entities;
using ConfigApi.Entities.Search;
using Microsoft.Extensions.Logging;

namespace ConfigApi.Context.Search;

public class ElasticProductSearchIndex : IProductSearchIndex
{
    private const int BulkChunkSize = 500;

    private readonly IHttpClientFactory _factory;
    private readonly ElasticsearchOptions _options;
    private readonly ILogger<ElasticProductSearchIndex> _logger;

    public ElasticProductSearchIndex(IHttpClientFactory factory,
                                     ElasticsearchOptions options,
                                     ILogger<ElasticProductSearchIndex> logger)
    {
        _factory = factory;
        _options = options;
        _logger = logger;
    }

    public bool IsEnabled => _options.Enabled;

    private HttpClient Client() => _factory.CreateClient(ElasticsearchOptions.HttpClientName);

    private string Index => _options.ProductsIndex;

    public async Task<bool> PingAsync()
    {
        try
        {
            using var response = await Client().GetAsync("/");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Elasticsearch erisilemiyor.");
            return false;
        }
    }

    public async Task EnsureIndexAsync()
    {
        try
        {
            var client = Client();

            using var head = new HttpRequestMessage(HttpMethod.Head, $"/{Index}");
            using var exists = await client.SendAsync(head);

            if (exists.StatusCode == HttpStatusCode.OK) return;

            using var content = new StringContent(
                ProductIndexDefinition.Json, Encoding.UTF8, "application/json");

            using var response = await client.PutAsync($"/{Index}", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Elasticsearch indeksi olusturuldu: {Index}", Index);
                return;
            }

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("Indeks olusturulamadi: {Status} {Body}", response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Indeks olusturma basarisiz.");
        }
    }

    public async Task IndexAsync(Product product)
    {
        try
        {
            var json = JsonSerializer.Serialize(Map(product), ElasticJson.Document);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await Client()
                .PutAsync($"/{Index}/_doc/{product.Id}?refresh=wait_for", content);

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Urun indekslenemedi: {Id} {Status}", product.Id, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Urun indekslenemedi: {Id}", product.Id);
        }
    }

    public async Task IndexManyAsync(IEnumerable<Product> products)
    {
        var list = products.ToList();
        if (list.Count == 0) return;

        try
        {
            var client = Client();

            foreach (var chunk in list.Chunk(BulkChunkSize))
            {
                var payload = new StringBuilder();

                foreach (var product in chunk)
                {
                    payload.Append("{\"index\":{\"_id\":\"").Append(product.Id).Append("\"}}\n");
                    payload.Append(JsonSerializer.Serialize(Map(product), ElasticJson.Document)).Append('\n');
                }

                using var content = new StringContent(
                    payload.ToString(), Encoding.UTF8, "application/x-ndjson");

                using var response = await client.PostAsync($"/{Index}/_bulk", content);

                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Toplu indeksleme basarisiz: {Status}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu indeksleme basarisiz.");
        }
    }

    public async Task DeleteAsync(Guid productId)
    {
        try
        {
            using var response = await Client().DeleteAsync($"/{Index}/_doc/{productId}");

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
                _logger.LogWarning("Urun indeksten silinemedi: {Id} {Status}", productId, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Urun indeksten silinemedi: {Id}", productId);
        }
    }

    public async Task<int> ReindexAllAsync(IEnumerable<Product> products)
    {
        var list = products.ToList();
        var client = Client();

        using (var delete = await client.DeleteAsync($"/{Index}"))
        {
            if (!delete.IsSuccessStatusCode && delete.StatusCode != HttpStatusCode.NotFound)
                _logger.LogWarning("Indeks silinemedi: {Status}", delete.StatusCode);
        }

        await EnsureIndexAsync();
        await IndexManyAsync(list);

        using (await client.PostAsync($"/{Index}/_refresh", null)) { }

        return list.Count;
    }

    private static ProductDocument Map(Product p) => new()
    {
        Id = p.Id,
        TenantId = p.TenantId,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        Stock = p.Stock,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt
    };
}
