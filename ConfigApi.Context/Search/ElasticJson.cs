using System.Text.Json;
using System.Text.Json.Serialization;
using ConfigApi.Entities.Search;

namespace ConfigApi.Context.Search;

internal static class ElasticJson
{
    public static readonly JsonSerializerOptions Document = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static readonly JsonSerializerOptions Query = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    internal sealed class SearchResponse
    {
        [JsonPropertyName("hits")]
        public HitsBlock Hits { get; set; } = new();
    }

    internal sealed class HitsBlock
    {
        [JsonPropertyName("total")]
        public TotalBlock Total { get; set; } = new();

        [JsonPropertyName("hits")]
        public List<HitBlock> Items { get; set; } = new();
    }

    internal sealed class TotalBlock
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }

    internal sealed class HitBlock
    {
        [JsonPropertyName("_source")]
        public ProductDocument? Source { get; set; }
    }

    internal sealed class AggregationResponse
    {
        [JsonPropertyName("aggregations")]
        public AggregationBlock Aggregations { get; set; } = new();
    }

    internal sealed class AggregationBlock
    {
        [JsonPropertyName("names")]
        public TermsBlock Names { get; set; } = new();
    }

    internal sealed class TermsBlock
    {
        [JsonPropertyName("buckets")]
        public List<BucketBlock> Buckets { get; set; } = new();
    }

    internal sealed class BucketBlock
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = "";
    }
}
