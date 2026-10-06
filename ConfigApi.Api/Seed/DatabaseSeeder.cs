using ConfigApi.Context.Factory;
using ConfigApi.Entities.Enums;
using Dapper;

namespace ConfigApi.Api.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IDbConnectionFactory factory)
    {
        using var conn = factory.Create();

        await CreateSchemaAsync(conn);
        await SeedTenantsAsync(conn);
        await SeedUsersAsync(conn);
    }

    private static async Task CreateSchemaAsync(System.Data.IDbConnection conn)
    {
        foreach (var batch in SchemaBatches)
            await conn.ExecuteAsync(batch);
    }

    private static async Task SeedTenantsAsync(System.Data.IDbConnection conn)
    {
        var tenantCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Tenants;");
        if (tenantCount > 0) return;

        var acme = Guid.NewGuid();
        var beta = Guid.NewGuid();

        await conn.ExecuteAsync(@"
            INSERT INTO Tenants (Id, Name, Slug) VALUES (@Id, @Name, @Slug);",
            new[]
            {
                new { Id = acme, Name = "Acme Mağaza", Slug = "acme" },
                new { Id = beta, Name = "Beta Store",  Slug = "beta" }
            });

        await conn.ExecuteAsync(@"
            INSERT INTO TenantDomains (TenantId, Domain, IsPrimary)
            VALUES (@TenantId, @Domain, @IsPrimary);",
            new[]
            {
                new { TenantId = acme, Domain = "acme.localhost", IsPrimary = true },
                new { TenantId = acme, Domain = "x.com",          IsPrimary = false },
                new { TenantId = beta, Domain = "beta.localhost", IsPrimary = true },
                new { TenantId = beta, Domain = "y.com",          IsPrimary = false }
            });

        await conn.ExecuteAsync(@"
            INSERT INTO Configs (TenantId, ConfigKey, ConfigValue)
            VALUES (@TenantId, @ConfigKey, @ConfigValue);",
            new[]
            {
                new { TenantId = acme, ConfigKey = "theme",    ConfigValue = "dark" },
                new { TenantId = acme, ConfigKey = "currency", ConfigValue = "TRY" },
                new { TenantId = beta, ConfigKey = "theme",    ConfigValue = "light" },
                new { TenantId = beta, ConfigKey = "currency", ConfigValue = "USD" }
            });

        await conn.ExecuteAsync(@"
            INSERT INTO Products (TenantId, Name, Description, Price, Stock)
            VALUES (@TenantId, @Name, @Description, @Price, @Stock);",
            new[]
            {
                new { TenantId = acme, Name = "Acme Klavye",   Description = "Mekanik klavye",      Price = 1250.00m, Stock = 20 },
                new { TenantId = acme, Name = "Acme Mouse",    Description = "Kablosuz mouse",      Price = 450.00m,  Stock = 50 },
                new { TenantId = acme, Name = "Acme Monitör",  Description = "27 inç IPS",          Price = 6800.00m, Stock = 8 },
                new { TenantId = beta, Name = "Beta Kulaklık", Description = "Gürültü engelleyici", Price = 2100.00m, Stock = 15 },
                new { TenantId = beta, Name = "Beta Webcam",   Description = "1080p",               Price = 890.00m,  Stock = 30 },
                new { TenantId = beta, Name = "Beta Hoparlör", Description = "Bluetooth",           Price = 1540.00m, Stock = 12 }
            });
    }

    private static async Task SeedUsersAsync(System.Data.IDbConnection conn)
    {
        var userCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users;");
        if (userCount > 0) return;

        var hash = BCrypt.Net.BCrypt.HashPassword("Test1234");

        var acme = await conn.ExecuteScalarAsync<Guid>("SELECT Id FROM Tenants WHERE Slug = 'acme';");
        var beta = await conn.ExecuteScalarAsync<Guid>("SELECT Id FROM Tenants WHERE Slug = 'beta';");

        await conn.ExecuteAsync(@"
            INSERT INTO Users (TenantId, Email, PasswordHash, FullName, Role)
            VALUES (@TenantId, @Email, @Hash, @FullName, @Role);",
            new[]
            {
                new { TenantId = (Guid?)acme, Email = "user@acme.com",   Hash = hash, FullName = "Acme Müşteri",   Role = UserRole.User },
                new { TenantId = (Guid?)acme, Email = "user2@acme.com",  Hash = hash, FullName = "Acme Müşteri 2", Role = UserRole.User },
                new { TenantId = (Guid?)acme, Email = "admin@acme.com",  Hash = hash, FullName = "Acme Yönetici",  Role = UserRole.TenantAdmin },
                new { TenantId = (Guid?)beta, Email = "user@beta.com",   Hash = hash, FullName = "Beta Müşteri",   Role = UserRole.User },
                new { TenantId = (Guid?)beta, Email = "admin@beta.com",  Hash = hash, FullName = "Beta Yönetici",  Role = UserRole.TenantAdmin },
                new { TenantId = (Guid?)null, Email = "root@system.com", Hash = hash, FullName = "Süper Admin",    Role = UserRole.SuperAdmin }
            });
    }

    private static readonly string[] SchemaBatches =
    [
        @"
        IF OBJECT_ID('dbo.Tenants', 'U') IS NULL
        CREATE TABLE Tenants (
            Id         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            Name       NVARCHAR(200)    NOT NULL,
            Slug       NVARCHAR(50)     NOT NULL,
            IsActive   BIT              NOT NULL DEFAULT 1,
            CreatedAt  DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
            CONSTRAINT PK_Tenants PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT UX_Tenants_Slug UNIQUE (Slug)
        );",

        @"
        IF OBJECT_ID('dbo.TenantDomains', 'U') IS NULL
        CREATE TABLE TenantDomains (
            Id         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            TenantId   UNIQUEIDENTIFIER NOT NULL,
            Domain     NVARCHAR(256)    NOT NULL,
            IsPrimary  BIT              NOT NULL DEFAULT 0,
            CONSTRAINT PK_TenantDomains PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_TenantDomains_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
            CONSTRAINT UX_TenantDomains_Domain UNIQUE (Domain)
        );",

        @"
        IF OBJECT_ID('dbo.Users', 'U') IS NULL
        CREATE TABLE Users (
            Id            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            TenantId      UNIQUEIDENTIFIER NULL,
            Email         NVARCHAR(256)    NOT NULL,
            PasswordHash  NVARCHAR(256)    NOT NULL,
            FullName      NVARCHAR(200)    NOT NULL DEFAULT '',
            Role          NVARCHAR(30)     NOT NULL DEFAULT 'User',
            IsActive      BIT              NOT NULL DEFAULT 1,
            CreatedAt     DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
            CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_Users_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
        );",

        @"
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Users_Tenant_Email')
        CREATE UNIQUE INDEX UX_Users_Tenant_Email ON Users(TenantId, Email);",

        @"
        IF OBJECT_ID('dbo.RefreshTokens', 'U') IS NULL
        CREATE TABLE RefreshTokens (
            Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            UserId      UNIQUEIDENTIFIER NOT NULL,
            TokenHash   NVARCHAR(256)    NOT NULL,
            ExpiresAt   DATETIME2        NOT NULL,
            RevokedAt   DATETIME2        NULL,
            CreatedAt   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
            CONSTRAINT PK_RefreshTokens PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
            CONSTRAINT UX_RefreshTokens_Hash UNIQUE (TokenHash)
        );",

        @"
        IF OBJECT_ID('dbo.Products', 'U') IS NULL
        CREATE TABLE Products (
            Id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            TenantId     UNIQUEIDENTIFIER NOT NULL,
            Name         NVARCHAR(250)    NOT NULL,
            Description  NVARCHAR(2000)   NOT NULL DEFAULT '',
            Price        DECIMAL(18,2)    NOT NULL,
            Stock        INT              NOT NULL DEFAULT 0,
            IsActive     BIT              NOT NULL DEFAULT 1,
            CreatedAt    DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
            CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_Products_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
            CONSTRAINT CK_Products_Price CHECK (Price >= 0),
            CONSTRAINT CK_Products_Stock CHECK (Stock >= 0)
        );",

        @"
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Tenant_Active')
        CREATE INDEX IX_Products_Tenant_Active ON Products(TenantId, IsActive) INCLUDE (Name, Price);",

        @"
        IF OBJECT_ID('dbo.Carts', 'U') IS NULL
        CREATE TABLE Carts (
            Id         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            TenantId   UNIQUEIDENTIFIER NOT NULL,
            UserId     UNIQUEIDENTIFIER NOT NULL,
            CreatedAt  DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
            UpdatedAt  DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
            CONSTRAINT PK_Carts PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_Carts_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
            CONSTRAINT FK_Carts_Users   FOREIGN KEY (UserId)   REFERENCES Users(Id),
            CONSTRAINT UX_Carts_User UNIQUE (UserId)
        );",

        @"
        IF OBJECT_ID('dbo.CartItems', 'U') IS NULL
        CREATE TABLE CartItems (
            Id         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            CartId     UNIQUEIDENTIFIER NOT NULL,
            ProductId  UNIQUEIDENTIFIER NOT NULL,
            Quantity   INT              NOT NULL,
            CONSTRAINT PK_CartItems PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_CartItems_Carts    FOREIGN KEY (CartId)    REFERENCES Carts(Id),
            CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
            CONSTRAINT CK_CartItems_Quantity CHECK (Quantity > 0),
            CONSTRAINT UX_CartItems_Cart_Product UNIQUE (CartId, ProductId)
        );",

        @"
        IF OBJECT_ID('dbo.Orders', 'U') IS NULL
        CREATE TABLE Orders (
            Id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            TenantId     UNIQUEIDENTIFIER NOT NULL,
            UserId       UNIQUEIDENTIFIER NOT NULL,
            OrderNumber  NVARCHAR(30)     NOT NULL,
            TotalAmount  DECIMAL(18,2)    NOT NULL,
            Status       TINYINT          NOT NULL DEFAULT 0,
            CreatedAt    DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
            CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_Orders_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
            CONSTRAINT FK_Orders_Users   FOREIGN KEY (UserId)   REFERENCES Users(Id),
            CONSTRAINT UX_Orders_Tenant_Number UNIQUE (TenantId, OrderNumber)
        );",

        @"
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Tenant_Created')
        CREATE INDEX IX_Orders_Tenant_Created ON Orders(TenantId, CreatedAt DESC);",

        @"
        IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
        CREATE TABLE OrderItems (
            Id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            OrderId      UNIQUEIDENTIFIER NOT NULL,
            ProductId    UNIQUEIDENTIFIER NOT NULL,
            ProductName  NVARCHAR(250)    NOT NULL,
            UnitPrice    DECIMAL(18,2)    NOT NULL,
            Quantity     INT              NOT NULL,
            CONSTRAINT PK_OrderItems PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_OrderItems_Orders   FOREIGN KEY (OrderId)   REFERENCES Orders(Id),
            CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
            CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0)
        );",

        @"
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_OrderId')
        CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);",

        @"
        IF OBJECT_ID('dbo.Configs', 'U') IS NULL
        CREATE TABLE Configs (
            Id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            TenantId     UNIQUEIDENTIFIER NOT NULL,
            ConfigKey    NVARCHAR(100)    NOT NULL,
            ConfigValue  NVARCHAR(MAX)    NOT NULL,
            IsActive     BIT              NOT NULL DEFAULT 1,
            CONSTRAINT PK_Configs PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_Configs_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
            CONSTRAINT UX_Configs_Tenant_Key UNIQUE (TenantId, ConfigKey)
        );"
    ];
}
