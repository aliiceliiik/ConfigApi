using System.Data;
using ConfigApi.Context.Factory;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ConfigApi.Api.Seed;

public static class ProductBulkSeeder
{
    private static readonly string[] Categories =
    [
        "Klavye", "Mouse", "Monitör", "Kulaklık", "Webcam", "Hoparlör",
        "Mikrofon", "Laptop Standı", "USB Hub", "Harici Disk", "SSD", "RAM",
        "Ekran Kartı", "Anakart", "İşlemci", "Güç Kaynağı", "Kasa", "Fan",
        "Mousepad", "Docking Station", "Adaptör", "Kablo", "Şarj Cihazı", "Powerbank"
    ];

    private static readonly string[] Brands =
    [
        "Volta", "Nexora", "Kıvılcım", "Demirsan", "Pulsar", "Aurora",
        "Teknoklas", "Yıldırım", "Mavera", "Zirve", "Lodos", "Pusula"
    ];

    private static readonly string[] Attributes =
    [
        "kablosuz", "RGB aydınlatmalı", "mekanik", "taşınabilir", "sessiz",
        "oyuncu serisi", "profesyonel", "kompakt", "gürültü engelleyici",
        "hızlı şarj destekli", "yüksek çözünürlüklü", "ergonomik",
        "alüminyum gövdeli", "katlanabilir", "su geçirmez"
    ];

    private static readonly string[] Models =
    [
        "X100", "X200", "Pro", "Pro Max", "Lite", "Air", "Ultra",
        "S1", "S2", "S3", "GT", "RS", "Edge", "Core", "Prime"
    ];

    public static async Task SeedAsync(IDbConnectionFactory factory, int targetCount)
    {
        if (targetCount <= 0) return;

        using var conn = factory.Create();

        var existing = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Products;");
        if (existing >= targetCount) return;

        var tenants = (await conn.QueryAsync<(Guid Id, string Slug)>(
            "SELECT Id, Slug FROM Tenants ORDER BY Slug;")).ToList();

        if (tenants.Count == 0) return;

        var toCreate = targetCount - existing;
        var random = new Random(20261007);

        var table = new DataTable();
        table.Columns.Add("TenantId", typeof(Guid));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Description", typeof(string));
        table.Columns.Add("Price", typeof(decimal));
        table.Columns.Add("Stock", typeof(int));
        table.Columns.Add("IsActive", typeof(bool));

        for (var i = 0; i < toCreate; i++)
        {
            var tenant = tenants[i % tenants.Count];
            var category = Categories[random.Next(Categories.Length)];
            var brand = Brands[random.Next(Brands.Length)];
            var model = Models[random.Next(Models.Length)];
            var attribute = Attributes[random.Next(Attributes.Length)];
            var secondAttribute = Attributes[random.Next(Attributes.Length)];

            var name = $"{brand} {category} {model}";
            var description = secondAttribute == attribute
                ? $"{attribute.Substring(0, 1).ToUpper()}{attribute[1..]} {category.ToLower()}."
                : $"{attribute.Substring(0, 1).ToUpper()}{attribute[1..]}, {secondAttribute} {category.ToLower()}.";

            var price = Math.Round((decimal)(random.NextDouble() * 24_500 + 150), 2);
            var stock = random.Next(100) switch
            {
                < 8 => 0,
                < 25 => random.Next(1, 10),
                _ => random.Next(10, 250)
            };

            table.Rows.Add(tenant.Id, name, description, price, stock, random.Next(100) >= 5);
        }

        var sqlConn = (SqlConnection)conn;
        await sqlConn.OpenAsync();

        using var bulk = new SqlBulkCopy(sqlConn)
        {
            DestinationTableName = "Products",
            BatchSize = 500,
            BulkCopyTimeout = 120
        };

        foreach (DataColumn column in table.Columns)
            bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);

        await bulk.WriteToServerAsync(table);
    }
}
