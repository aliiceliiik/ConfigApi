namespace ConfigApi.Entities.Dtos;

<<<<<<< HEAD
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
=======
public class ProductSearchRequest
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
}
