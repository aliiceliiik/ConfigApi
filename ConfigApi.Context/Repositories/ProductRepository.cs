using ConfigApi.Context.Factory;
using ConfigApi.Context.Repositories.Base;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Entities;
using Dapper;

namespace ConfigApi.Context.Repositories;

public interface IProductRepository
{
<<<<<<< HEAD
    Task<(IEnumerable<Product> items, int total)> SearchAsync(ProductSearchRequest request);
    Task<Product?> GetByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetAllForAdminAsync();
    Task<Product?> GetByIdForAdminAsync(Guid id);
    Task<IEnumerable<Product>> GetAllForIndexingAsync();
    Task<Product> CreateAsync(ProductSaveRequest request);
    Task<Product?> UpdateAsync(Guid id, ProductSaveRequest request);
    Task<Product?> SetActiveAsync(Guid id, bool isActive);
=======
    Task<(IEnumerable<Product> items, int total)> SearchAsync(string? search, int page, int pageSize);
    Task<Product?> GetByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetAllForAdminAsync();
    Task<Product?> GetByIdForAdminAsync(Guid id);
    Task<Guid> CreateAsync(ProductSaveRequest request);
    Task<bool> UpdateAsync(Guid id, ProductSaveRequest request);
    Task<bool> SetActiveAsync(Guid id, bool isActive);
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
}

public class ProductRepository : TenantScopedRepository, IProductRepository
{
    public ProductRepository(IDbConnectionFactory f, ITenantProvider t) : base(f, t) { }

    private const string Columns =
        "Id, TenantId, Name, Description, Price, Stock, IsActive, CreatedAt";

<<<<<<< HEAD
    private const string Inserted =
        "INSERTED.Id, INSERTED.TenantId, INSERTED.Name, INSERTED.Description, " +
        "INSERTED.Price, INSERTED.Stock, INSERTED.IsActive, INSERTED.CreatedAt";

=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
    private const string SearchSql = $@"
        SELECT  {Columns}
        FROM    Products
        WHERE   TenantId = @TenantId
          AND   IsActive = 1
          AND   (@Search IS NULL OR Name LIKE @Like OR Description LIKE @Like)
<<<<<<< HEAD
          AND   (@MinPrice IS NULL OR Price >= @MinPrice)
          AND   (@MaxPrice IS NULL OR Price <= @MaxPrice)
          AND   (@InStockOnly = 0 OR Stock > 0)
        ORDER BY
            CASE WHEN @Sort = 1 THEN Price END ASC,
            CASE WHEN @Sort = 2 THEN Price END DESC,
            CASE WHEN @Sort = 4 THEN CreatedAt END DESC,
            Name ASC
=======
        ORDER BY Name
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

        SELECT  COUNT(*)
        FROM    Products
        WHERE   TenantId = @TenantId
          AND   IsActive = 1
<<<<<<< HEAD
          AND   (@Search IS NULL OR Name LIKE @Like OR Description LIKE @Like)
          AND   (@MinPrice IS NULL OR Price >= @MinPrice)
          AND   (@MaxPrice IS NULL OR Price <= @MaxPrice)
          AND   (@InStockOnly = 0 OR Stock > 0);";

    public async Task<(IEnumerable<Product> items, int total)> SearchAsync(ProductSearchRequest request)
    {
        var term = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
=======
          AND   (@Search IS NULL OR Name LIKE @Like OR Description LIKE @Like);";

    public async Task<(IEnumerable<Product> items, int total)> SearchAsync(string? search, int page, int pageSize)
    {
        var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43

        using var conn = Factory.Create();
        using var multi = await conn.QueryMultipleAsync(SearchSql, new
        {
            TenantId,
            Search = term,
            Like = term is null ? "%" : $"%{term}%",
<<<<<<< HEAD
            request.MinPrice,
            request.MaxPrice,
            InStockOnly = request.InStockOnly ? 1 : 0,
            Sort = (int)request.Sort,
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize
=======
            Skip = (page - 1) * pageSize,
            Take = pageSize
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
        });

        var items = (await multi.ReadAsync<Product>()).ToList();
        var total = await multi.ReadSingleAsync<int>();
<<<<<<< HEAD

=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
        return (items, total);
    }

    private const string GetByIdSql = $@"
        SELECT  {Columns}
        FROM    Products
        WHERE   Id = @Id AND TenantId = @TenantId AND IsActive = 1;";

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        using var conn = Factory.Create();
        return await conn.QuerySingleOrDefaultAsync<Product>(
            GetByIdSql, new { Id = id, TenantId });
    }

    public async Task<IEnumerable<Product>> GetAllForAdminAsync()
    {
        using var conn = Factory.Create();
        return await conn.QueryAsync<Product>($@"
            SELECT  {Columns}
            FROM    Products
            WHERE   TenantId = @TenantId
            ORDER BY IsActive DESC, Name;", new { TenantId });
    }

    public async Task<Product?> GetByIdForAdminAsync(Guid id)
    {
        using var conn = Factory.Create();
        return await conn.QuerySingleOrDefaultAsync<Product>($@"
            SELECT  {Columns}
            FROM    Products
            WHERE   Id = @Id AND TenantId = @TenantId;", new { Id = id, TenantId });
    }

<<<<<<< HEAD
    public async Task<IEnumerable<Product>> GetAllForIndexingAsync()
    {
        using var conn = Factory.Create();
        return await conn.QueryAsync<Product>($@"
            SELECT {Columns} FROM Products ORDER BY CreatedAt;");
    }

    public async Task<Product> CreateAsync(ProductSaveRequest request)
    {
        using var conn = Factory.Create();
        return await conn.QuerySingleAsync<Product>($@"
            INSERT INTO Products (TenantId, Name, Description, Price, Stock, IsActive)
            OUTPUT {Inserted}
=======
    public async Task<Guid> CreateAsync(ProductSaveRequest request)
    {
        using var conn = Factory.Create();
        return await conn.QuerySingleAsync<Guid>(@"
            INSERT INTO Products (TenantId, Name, Description, Price, Stock, IsActive)
            OUTPUT INSERTED.Id
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
            VALUES (@TenantId, @Name, @Description, @Price, @Stock, @IsActive);",
            new
            {
                TenantId,
                request.Name,
                request.Description,
                request.Price,
                request.Stock,
                request.IsActive
            });
    }

<<<<<<< HEAD
    public async Task<Product?> UpdateAsync(Guid id, ProductSaveRequest request)
    {
        using var conn = Factory.Create();
        return await conn.QuerySingleOrDefaultAsync<Product>($@"
            UPDATE Products
            SET    Name = @Name, Description = @Description,
                   Price = @Price, Stock = @Stock, IsActive = @IsActive
            OUTPUT {Inserted}
=======
    public async Task<bool> UpdateAsync(Guid id, ProductSaveRequest request)
    {
        using var conn = Factory.Create();
        var affected = await conn.ExecuteAsync(@"
            UPDATE Products
            SET    Name = @Name, Description = @Description,
                   Price = @Price, Stock = @Stock, IsActive = @IsActive
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
            WHERE  Id = @Id AND TenantId = @TenantId;",
            new
            {
                Id = id,
                TenantId,
                request.Name,
                request.Description,
                request.Price,
                request.Stock,
                request.IsActive
            });
<<<<<<< HEAD
    }

    public async Task<Product?> SetActiveAsync(Guid id, bool isActive)
    {
        using var conn = Factory.Create();
        return await conn.QuerySingleOrDefaultAsync<Product>($@"
            UPDATE Products
            SET    IsActive = @IsActive
            OUTPUT {Inserted}
            WHERE  Id = @Id AND TenantId = @TenantId;",
            new { Id = id, TenantId, IsActive = isActive });
=======

        return affected > 0;
    }

    public async Task<bool> SetActiveAsync(Guid id, bool isActive)
    {
        using var conn = Factory.Create();
        var affected = await conn.ExecuteAsync(
            "UPDATE Products SET IsActive = @IsActive WHERE Id = @Id AND TenantId = @TenantId;",
            new { Id = id, TenantId, IsActive = isActive });

        return affected > 0;
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
    }
}
