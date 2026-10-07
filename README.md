# ConfigApi — Çok Tenantlı E-Ticaret

ASP.NET Core 8 · Dapper · MSSQL · JWT · Elasticsearch · Redis · Katmanlı mimari

## Projeler

| Proje | Görev |
|---|---|
| ConfigApi.Api | REST API |
| ConfigApi.Mvc | Razor arayüz, API'ye HTTP ile bağlanır |
| ConfigApi.Business | Servisler, tenant context, JWT |
| ConfigApi.Context | Dapper repository'ler, Redis önbellek, Elasticsearch |
| ConfigApi.Entities | Entity, DTO ve indeks belgeleri |

## Kurulum

1. Docker Desktop açıkken solution klasöründe:

```bash
docker compose up -d
docker compose ps
```

Elasticsearch, Kibana ve Redis ayağa kalkar. ES ve Redis `healthy` olmalı.

2. `ConfigApi.Api/appsettings.json` içindeki connection string'i kendi SQL Server'ına göre düzenle.
   Varsayılan LocalDB kullanır.

3. Solution'a sağ tık → Properties → Multiple startup projects → `ConfigApi.Api` ve `ConfigApi.Mvc` ikisini de **Start** yap.

4. Çalıştır. API ilk açılışta şemayı kurar, örnek veriyi ve 2.000 ürünü yazar, Elasticsearch indeksini doldurur.

Docker kullanmak istemezsen `appsettings.json` içinde `Elasticsearch:Enabled` ve `Redis:Enabled` değerlerini `false` yap; proje SQL ile çalışmaya devam eder.

## Adresler

| Uygulama | Adres |
|---|---|
| API | http://localhost:5095 · http://localhost:5095/swagger |
| Acme mağazası | http://localhost:5201 |
| Beta mağazası | http://localhost:5202 |
| Süper admin | http://localhost:5203 |
| Kibana | http://localhost:5601 |
| Elasticsearch | http://localhost:9200 |
| Redis | localhost:6379 |

Tenant, MVC'nin dinlediği porttan belirlenir (`Tenancy:PortHosts`). MVC her API isteğine `X-Tenant-Host` başlığını ekler, API tenantı bu başlıktan çözer. Postman'den test ederken aynı başlığı elle göndermen gerekir:

```
POST http://localhost:5095/api/auth/login
X-Tenant-Host: acme.localhost

{ "email": "user@acme.com", "password": "Test1234" }
```

Gerçek sunucuda bu başlık kaldırılıp `Request.Host` kullanılmalı; orada subdomain'ler gerçek DNS kayıtları olur.

## Test kullanıcıları

Hepsinin şifresi `Test1234`.

| Tenant | E-posta | Rol |
|---|---|---|
| acme.localhost | user@acme.com | User |
| acme.localhost | user2@acme.com | User |
| acme.localhost | admin@acme.com | TenantAdmin |
| beta.localhost | user@beta.com | User |
| beta.localhost | admin@beta.com | TenantAdmin |
| admin.localhost | root@system.com | SuperAdmin |

## Mimari notlar

**Tenant çözümleme.** `TenantResolutionMiddleware` isteğin host'una bakar, `TenantDomains` tablosundan tenantı bulur (sonuç Redis'te 60 dakika önbelleklenir). Login o tenant kapsamında yapılır, tenant kimliği JWT'ye `tenantId` claim'i olarak mühürlenir.

**İzolasyon üç katmanda.**
- SQL: `TenantScopedRepository` türeyen her repository `TenantId`'yi `ITenantProvider`'dan alır, metot parametresi olarak dışarıdan alınmaz. Her sorguda `WHERE TenantId = @TenantId` bulunur.
- Elasticsearch: tek indeks, her belgede `tenantId` alanı. Tüm sorgular `TenantScope()` metodundan başlar ve `filter` içinde `term: tenantId` taşır.
- Redis: her anahtar `t:{tenantId}:` ile başlar.

Çapraz tenant erişiminde 403 değil 404 döner; 403 kaydın varlığını sızdırır.

**Roller.** `User`, `TenantAdmin`, `SuperAdmin`. Süper admin hiçbir tenanta ait değildir (`TenantId IS NULL`) ve `X-Tenant-Id` başlığıyla tenant seçer. Tenant admin bu başlığı gönderse bile yok sayılır.

**Oturum.** Access token 15 dakika, refresh token 7 gün. Refresh token'ın SHA-256 hash'i saklanır, her yenilemede rotasyona uğrar. MVC tarafında access token session'da (Redis), refresh token HttpOnly cookie'de tutulur.

**Arama.** Sıra: Redis önbelleği → Elasticsearch → SQL `LIKE`. Elasticsearch kapalı veya erişilemezse sessizce SQL'e düşülür. Türkçe analiz zinciri `lowercase → tr_stop → tr_stemmer → asciifolding`; yazım hatası toleransı `fuzziness: AUTO` ile, otomatik tamamlama `edge_ngram` ile sağlanır.

**Senkronizasyon.** Ürün eklendiğinde, güncellendiğinde, pasife alındığında ve sipariş sonrası stok düştüğünde `ProductSyncService` indeksi günceller ve yalnızca o tenantın arama önbelleğini temizler.

**Sipariş.** Checkout tek transaction içinde çalışır: `UPDLOCK` ile stok kilitlenir, koşullu `UPDATE` ile düşülür, ürün adı ve fiyatı `OrderItems` satırına kopyalanır.

**Dayanıklılık.** Redis ve Elasticsearch isteğe bağlıdır. İkisi de kapalıyken uygulama açılır ve çalışır; önbellek `NullCacheService`, indeks `NullProductSearchIndex` ile devre dışı kalır.

## Uç noktalar

```
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout

GET    /api/products?search=&minPrice=&maxPrice=&inStockOnly=&sort=&page=&pageSize=
GET    /api/products/suggest?q=
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
POST   /api/admin/reindex                  SuperAdmin
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

`sort`: 0 ilgi · 1 fiyat artan · 2 fiyat azalan · 3 isim · 4 yeni

## Faydalı komutlar

```bash
docker exec configapi-redis redis-cli --scan --pattern 'configapi:*'
docker exec configapi-redis redis-cli FLUSHALL

curl http://localhost:9200/products/_count
curl "http://localhost:9200/products/_mapping?pretty"
```
