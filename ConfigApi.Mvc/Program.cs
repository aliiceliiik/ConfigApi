using ConfigApi.Mvc.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("api");

builder.Services.AddScoped<ITokenStore, TokenStore>();
builder.Services.AddScoped<ITenantSelection, TenantSelection>();
builder.Services.AddScoped<IApiClient, ApiClient>();

<<<<<<< HEAD
var redisConfig = builder.Configuration["Redis:Configuration"];
var redisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled");

if (redisEnabled && !string.IsNullOrWhiteSpace(redisConfig))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConfig;
        options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "configapi:mvc:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}
=======
builder.Services.AddDistributedMemoryCache();
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
