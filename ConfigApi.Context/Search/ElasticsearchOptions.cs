namespace ConfigApi.Context.Search;

public class ElasticsearchOptions
{
    public const string HttpClientName = "elasticsearch";

    public string Uri { get; set; } = "http://localhost:9200";
    public string ProductsIndex { get; set; } = "products";
    public bool Enabled { get; set; }
}
