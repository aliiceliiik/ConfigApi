using System.Security.Claims;
using System.Text;
using ConfigApi.Api.Filters;
using ConfigApi.Api.Middleware;
using ConfigApi.Api.Seed;
using ConfigApi.Business;
using ConfigApi.Context.Factory;
<<<<<<< HEAD
using ConfigApi.Context.Repositories;
using ConfigApi.Context.Search;
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Token'ını gir."
    });

    options.AddSecurityDefinition(ResolveAdminScopeAttribute.TenantHeader, new OpenApiSecurityScheme
    {
        Name = ResolveAdminScopeAttribute.TenantHeader,
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Yalnızca süper admin için: yönetilecek tenant Guid'i."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>(),
            [new OpenApiSecuritySchemeReference(ResolveAdminScopeAttribute.TenantHeader, document)] = new List<string>()
        });
});

<<<<<<< HEAD
builder.Services.AddBusinessServices(builder.Configuration);
=======
builder.Services.AddBusinessServices();
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43

var jwt = builder.Configuration.GetSection("Jwt");

var jwtKey = jwt["Key"]
    ?? throw new InvalidOperationException(
        "appsettings.json içinde Jwt:Key bulunamadı. Dosya ConfigApi.Api altında mı ve 'Copy if newer' işaretli mi?");

if (jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key en az 32 karakter olmalı.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = "userId",

            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Clients", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("Clients");
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
<<<<<<< HEAD
    var sp = scope.ServiceProvider;

    var factory = sp.GetRequiredService<IDbConnectionFactory>();
    var bulkCount = builder.Configuration.GetValue<int>("Seed:BulkProductCount");

    await DatabaseSeeder.SeedAsync(factory, bulkCount);

    await SearchIndexSeeder.SeedAsync(
        sp.GetRequiredService<IProductSearchIndex>(),
        sp.GetRequiredService<IProductRepository>(),
        sp.GetRequiredService<ILogger<Program>>());
=======
    var factory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    await DatabaseSeeder.SeedAsync(factory);
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
}

app.Run();
