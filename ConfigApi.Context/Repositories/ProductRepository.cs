using ConfigApi.Context.Factory;
using ConfigApi.Context.Repositories.Base;
using ConfigApi.Entities.Dtos;
using ConfigApi.Entities.Entities;
using Dapper;

namespace ConfigApi.Context.Repositories;

public interface IProductRepository
{
    Task<(IEnumerable<Product> items, int total)> SearchAsync(string? search, int page, int pageSize);
    Task<Product?> GetByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetAllForAdminAsync();
    Task<Product?> GetByIdForAdminAsync(Guid id);
    Task<Guid> CreateAsync(ProductSaveRequest request);
    Task<bool> UpdateAsync(Guid id, ProductSaveRequest request);
    Task<bool> SetActiveAsync(Guid id, bool isActive);
}

public class ProductRepository : TenantScopedRepository, IProductRepository
{
    public ProductRepository(IDbConnectionFactory f, ITenantProvider t) : base(f, t) { }

    private const string Columns =
        "Id, TenantId, Name, Description, Price, Stock, IsActive, CreatedAt";

    private const string SearchSql = $@"
        SELECT  {Columns}
        FROM    Products
        WHERE   TenantId = @TenantId
          AND   IsActive = 1
          AND   (@Search IS NULL OR Name LIKE @Like OR Description LIKE @Like)
        ORDER BY Name
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

        SELECT  COUNT(*)
        FROM    Products
        WHERE   TenantId = @TenantId
          AND   IsActive = 1
          AND   (@Search IS NULL OR Name LIKE @Like OR Description LIKE @Like);";

    public async Task<(IEnumerable<Product> items, int total)> SearchAsync(string? search, int page, int pageSize)
    {
        var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        using var conn = Factory.Create();
        using var multi = await conn.QueryMultipleAsync(SearchSql, new
        {
            TenantId,
            Search = term,
            Like = term is null ? "%" : $"%{term}%",
            Skip = (page - 1) * pageSize,
            Take = pageSize
        });

        var items = (await multi.ReadAsync<Product>()).ToList();
        var total = await multi.ReadSingleAsync<int>();
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

    public async Task<Guid> CreateAsync(ProductSaveRequest request)
    {
        using var conn = Factory.Create();
        return await conn.QuerySingleAsync<Guid>(@"
            INSERT INTO Products (TenantId, Name, Description, Price, Stock, IsActive)
            OUTPUT INSERTED.Id
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

    public async Task<bool> UpdateAsync(Guid id, ProductSaveRequest request)
    {
        using var conn = Factory.Create();
        var affected = await conn.ExecuteAsync(@"
            UPDATE Products
            SET    Name = @Name, Description = @Description,
                   Price = @Price, Stock = @Stock, IsActive = @IsActive
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

        return affected > 0;
    }

    public async Task<bool> SetActiveAsync(Guid id, bool isActive)
    {
        using var conn = Factory.Create();
        var affected = await conn.ExecuteAsync(
            "UPDATE Products SET IsActive = @IsActive WHERE Id = @Id AND TenantId = @TenantId;",
            new { Id = id, TenantId, IsActive = isActive });

        return affected > 0;
    }
}
