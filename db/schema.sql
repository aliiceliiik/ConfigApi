IF DB_ID('ConfigApiDb') IS NULL
    CREATE DATABASE ConfigApiDb;
GO

USE ConfigApiDb;
GO

IF OBJECT_ID('dbo.Tenants', 'U') IS NULL
CREATE TABLE Tenants (
    Id         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Name       NVARCHAR(200)    NOT NULL,
    Slug       NVARCHAR(50)     NOT NULL,
    IsActive   BIT              NOT NULL DEFAULT 1,
    CreatedAt  DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Tenants PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UX_Tenants_Slug UNIQUE (Slug)
);
GO

IF OBJECT_ID('dbo.TenantDomains', 'U') IS NULL
CREATE TABLE TenantDomains (
    Id         UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId   UNIQUEIDENTIFIER NOT NULL,
    Domain     NVARCHAR(256)    NOT NULL,
    IsPrimary  BIT              NOT NULL DEFAULT 0,
    CONSTRAINT PK_TenantDomains PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_TenantDomains_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT UX_TenantDomains_Domain UNIQUE (Domain)
);
GO

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
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Users_Tenant_Email')
CREATE UNIQUE INDEX UX_Users_Tenant_Email ON Users(TenantId, Email);
GO

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
);
GO

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
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Tenant_Active')
CREATE INDEX IX_Products_Tenant_Active ON Products(TenantId, IsActive) INCLUDE (Name, Price);
GO

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
);
GO

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
);
GO

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
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Tenant_Created')
CREATE INDEX IX_Orders_Tenant_Created ON Orders(TenantId, CreatedAt DESC);
GO

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
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_OrderId')
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
GO

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
);
GO
