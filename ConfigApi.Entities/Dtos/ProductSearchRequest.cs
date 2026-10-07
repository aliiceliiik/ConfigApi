namespace ConfigApi.Entities.Dtos;

public enum ProductSort
{
    Relevance = 0,
    PriceAsc = 1,
    PriceDesc = 2,
    NameAsc = 3,
    Newest = 4
}

public class ProductSearchRequest
{
    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool InStockOnly { get; set; }
    public ProductSort Sort { get; set; } = ProductSort.Relevance;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    public string CacheSignature()
        => string.Join('|',
            Search?.Trim().ToLowerInvariant() ?? "",
            MinPrice?.ToString("0.##") ?? "",
            MaxPrice?.ToString("0.##") ?? "",
            InStockOnly ? "1" : "0",
            ((int)Sort).ToString(),
            Page.ToString(),
            PageSize.ToString());
}
