# ConfigApi — Çok Tenantlı E-Ticaret

ASP.NET Core 8 · Dapper · MSSQL · JWT · Katmanlı mimari

## Projeler

| Proje | Görev |
|---|---|
| ConfigApi.Api | REST API |
| ConfigApi.Mvc | Razor arayüz, API'ye HTTP ile bağlanır |
| ConfigApi.Business | Servisler, tenant context, JWT |
| ConfigApi.Context | Dapper repository'ler |
| ConfigApi.Entities | Entity ve DTO'lar |

## Kurulum

1. `hosts` dosyasına ekle (`C:\Windows\System32\drivers\etc\hosts`, yönetici olarak):

```
127.0.0.1 acme.localhost
127.0.0.1 beta.localhost
127.0.0.1 admin.localhost
```

2. `ConfigApi.Api/appsettings.json` içindeki connection string'i kendi SQL Server'ına göre düzenle.
   Varsayılan LocalDB kullanır.

3. Solution'a sağ tık → Properties → Multiple startup projects → `ConfigApi.Api` ve `ConfigApi.Mvc` ikisini de **Start** yap.

4. Çalıştır. API ilk açılışta veritabanı şemasını ve örnek veriyi kendisi oluşturur.
   Şemayı elle kurmak istersen `db/schema.sql` dosyasını kullanabilirsin.

## Portlar

| Uygulama | Adres |
|---|---|
| API | http://localhost:5095 · https://localhost:7091/swagger |
| MVC | http://localhost:5200 · https://localhost:7200 |

Tenant arayüzüne host üzerinden girilir: `http://acme.localhost:5200`

## Test kullanıcıları

Hepsinin şifresi `Test1234`.

| Adres | E-posta | Rol |
|---|---|---|
| acme.localhost | user@acme.com | User |
| acme.localhost | user2@acme.com | User |
| acme.localhost | admin@acme.com | TenantAdmin |
| beta.localhost | user@beta.com | User |
| beta.localhost | admin@beta.com | TenantAdmin |
| admin.localhost | root@system.com | SuperAdmin |

## Mimari notlar

**Tenant çözümleme.** `TenantResolutionMiddleware` isteğin host'una bakar, `TenantDomains` tablosundan tenantı bulur. Login o tenant kapsamında yapılır, tenant kimliği JWT'ye `tenantId` claim'i olarak mühürlenir.

**İzolasyon.** `TenantScopedRepository` türeyen her repository `TenantId`'yi `ITenantProvider` üzerinden alır; metot parametresi olarak dışarıdan alınmaz. Her sorguda `WHERE TenantId = @TenantId` bulunur. Çapraz tenant erişiminde 403 değil 404 döner.

**Roller.** `User`, `TenantAdmin`, `SuperAdmin`. Süper admin hiçbir tenanta ait değildir (`TenantId IS NULL`), `admin.localhost` üzerinden girer ve `X-Tenant-Id` başlığıyla tenant seçer. Tenant admin bu başlığı gönderse bile yok sayılır.

**Oturum.** Access token 15 dakika, refresh token 7 gün. Refresh token'ın SHA-256 hash'i saklanır, her yenilemede rotasyona uğrar. MVC tarafında access token session'da, refresh token HttpOnly cookie'de tutulur.

**Sipariş.** Checkout tek transaction içinde çalışır: `UPDLOCK` ile stok kilitlenir, koşullu `UPDATE` ile düşülür, ürün adı ve fiyatı `OrderItems` satırına kopyalanır.

## Uç noktalar

```
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout

GET    /api/products
GET    /api/products/{id}
GET    /api/config

GET    /api/cart
POST   /api/cart/items
PUT    /api/cart/items/{itemId}
DELETE /api/cart/items/{itemId}
DELETE /api/cart

POST   /api/orders/checkout
GET    /api/orders
GET    /api/orders/{id}

GET    /api/tenants                        SuperAdmin
GET    /api/admin/orders                   TenantAdmin · SuperAdmin
GET    /api/admin/orders/{id}
PUT    /api/admin/orders/{id}/status
GET    /api/admin/users
GET    /api/admin/products
GET    /api/admin/products/{id}
POST   /api/admin/products
PUT    /api/admin/products/{id}
PATCH  /api/admin/products/{id}/active?value=true
```
