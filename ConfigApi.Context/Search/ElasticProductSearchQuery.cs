using System.Text;
using System.Text.Json;
using ConfigApi.Context.Repositories;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Search;
using Microsoft.Extensions.Logging;

namespace ConfigApi.Context.Search;

public class ElasticProductSearchQuery : IProductSearchQuery
{
    private readonly IHttpClientFactory _factory;
    private readonly ElasticsearchOptions _options;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<ElasticProductSearchQuery> _logger;

    public ElasticProductSearchQuery(IHttpClientFactory factory,
                                     ElasticsearchOptions options,
                                     ITenantProvider tenantProvider,
                                     ILogger<ElasticProductSearchQuery> logger)
    {
        _factory = factory;
        _options = options;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public bool IsEnabled => _options.Enabled;

    private HttpClient Client() => _factory.CreateClient(ElasticsearchOptions.HttpClientName);

    private string Index => _options.ProductsIndex;

    private List<object> TenantScope(ProductSearchRequest? request = null)
    {
        var filters = new List<object>
        {
            new Dictionary<string, object>
            {
                ["term"] = new Dictionary<string, object>
                {
                    ["tenantId"] = _tenantProvider.TenantId.ToString()
                }
            },
            new Dictionary<string, object>
            {
                ["term"] = new Dictionary<string, object> { ["isActive"] = true }
            }
        };

        if (request is null) return filters;

        if (request.InStockOnly)
            filters.Add(new Dictionary<string, object>
            {
                ["range"] = new Dictionary<string, object>
                {
                    ["stock"] = new Dictionary<string, object> { ["gt"] = 0 }
                }
            });

        if (request.MinPrice is not null || request.MaxPrice is not null)
        {
            var bounds = new Dictionary<string, object>();

            if (request.MinPrice is not null) bounds["gte"] = request.MinPrice.Value;
            if (request.MaxPrice is not null) bounds["lte"] = request.MaxPrice.Value;

            filters.Add(new Dictionary<string, object>
            {
                ["range"] = new Dictionary<string, object> { ["price"] = bounds }
            });
        }

        return filters;
    }

    private static object TextMatch(string term) => new Dictionary<string, object>
    {
        ["bool"] = new Dictionary<string, object>
        {
            ["minimum_should_match"] = 1,
            ["should"] = new object[]
            {
                new Dictionary<string, object>
                {
                    ["match_phrase"] = new Dictionary<string, object>
                    {
                        ["name"] = new Dictionary<string, object>
                        {
                            ["query"] = term,
                            ["boost"] = 10
                        }
                    }
                },
                new Dictionary<string, object>
                {
                    ["match"] = new Dictionary<string, object>
                    {
                        ["name"] = new Dictionary<string, object>
                        {
                            ["query"] = term,
                            ["boost"] = 5,
                            ["fuzziness"] = "AUTO",
                            ["operator"] = "and"
                        }
                    }
                },
                new Dictionary<string, object>
                {
                    ["match"] = new Dictionary<string, object>
                    {
                        ["description"] = new Dictionary<string, object>
                        {
                            ["query"] = term,
                            ["boost"] = 1,
                            ["fuzziness"] = "AUTO"
                        }
                    }
                }
            }
        }
    };

    private object BuildQuery(ProductSearchRequest request)
    {
        var inner = new Dictionary<string, object>
        {
            ["filter"] = TenantScope(request)
        };

        inner["must"] = string.IsNullOrWhiteSpace(request.Search)
            ? new object[] { new Dictionary<string, object> { ["match_all"] = new Dictionary<string, object>() } }
            : new[] { TextMatch(request.Search.Trim()) };

        return new Dictionary<string, object> { ["bool"] = inner };
    }

    private static object[] BuildSort(ProductSearchRequest request)
    {
        static object Field(string name, string order) => new Dictionary<string, object>
        {
            [name] = new Dictionary<string, object> { ["order"] = order }
        };

        return request.Sort switch
        {
            ProductSort.PriceAsc => new[] { Field("price", "asc") },
            ProductSort.PriceDesc => new[] { Field("price", "desc") },
            ProductSort.NameAsc => new[] { Field("name.keyword", "asc") },
            ProductSort.Newest => new[] { Field("createdAt", "desc") },
            _ => string.IsNullOrWhiteSpace(request.Search)
                ? new[] { Field("name.keyword", "asc") }
                : new[] { Field("_score", "desc") }
        };
    }

    public async Task<(IEnumerable<ProductListItemDto> items, int total)> SearchAsync(
        ProductSearchRequest request)
    {
        var body = new Dictionary<string, object>
        {
            ["from"] = (request.Page - 1) * request.PageSize,
            ["size"] = request.PageSize,
            ["track_total_hits"] = true,
            ["query"] = BuildQuery(request),
            ["sort"] = BuildSort(request)
        };

        var json = await SendAsync(body);
        var parsed = JsonSerializer.Deserialize<ElasticJson.SearchResponse>(json, ElasticJson.Query);

        if (parsed is null)
            throw new SearchUnavailableException("Elasticsearch yaniti cozumlenemedi.");

        var items = parsed.Hits.Items
            .Where(h => h.Source is not null)
            .Select(h => Map(h.Source!))
            .ToList();

        return (items, parsed.Hits.Total.Value);
    }

    public async Task<IEnumerable<string>> SuggestAsync(string prefix, int size)
    {
        if (string.IsNullOrWhiteSpace(prefix)) return Array.Empty<string>();

        var body = new Dictionary<string, object>
        {
            ["size"] = 0,
            ["query"] = new Dictionary<string, object>
            {
                ["bool"] = new Dictionary<string, object>
                {
                    ["filter"] = TenantScope(),
                    ["must"] = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["match"] = new Dictionary<string, object>
                            {
                                ["name.autocomplete"] = prefix.Trim()
                            }
                        }
                    }
                }
            },
            ["aggs"] = new Dictionary<string, object>
            {
                ["names"] = new Dictionary<string, object>
                {
                    ["terms"] = new Dictionary<string, object>
                    {
                        ["field"] = "name.keyword",
                        ["size"] = size
                    }
                }
            }
        };

        try
        {
            var json = await SendAsync(body);
            var parsed = JsonSerializer.Deserialize<ElasticJson.AggregationResponse>(json, ElasticJson.Query);

            return parsed?.Aggregations.Names.Buckets.Select(b => b.Key).ToList()
                   ?? (IEnumerable<string>)Array.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Oneri sorgusu basarisiz.");
            return Array.Empty<string>();
        }
    }

    private async Task<string> SendAsync(Dictionary<string, object> body)
    {
        var payload = JsonSerializer.Serialize(body, ElasticJson.Query);

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        HttpResponseMessage response;

        try
        {
            response = await Client().PostAsync($"/{Index}/_search", content);
        }
        catch (Exception ex)
        {
            throw new SearchUnavailableException("Elasticsearch erisilemiyor.", ex);
        }

        using (response)
        {
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode) return json;

            _logger.LogWarning("Elasticsearch sorgusu basarisiz: {Status} {Body}",
                response.StatusCode, json);

            throw new SearchUnavailableException("Elasticsearch sorgusu basarisiz.");
        }
    }

    private static ProductListItemDto Map(ProductDocument d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Description = d.Description,
        Price = d.Price,
        Stock = d.Stock
    };
}
