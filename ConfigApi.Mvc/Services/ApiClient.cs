using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ConfigApi.Entities.Dtos;

namespace ConfigApi.Mvc.Services;

public record ApiResponse<T>(bool Success, T? Data, string? Message, HttpStatusCode StatusCode);

public interface IApiClient
{
    Task<T?> GetAsync<T>(string path);
    Task<ApiResponse<T>> PostAsync<T>(string path, object? body = null);
    Task<ApiResponse<T>> PutAsync<T>(string path, object? body = null);
    Task<ApiResponse<object>> PatchAsync(string path);
    Task<ApiResponse<object>> DeleteAsync(string path);
}

public class ApiClient : IApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _accessor;
    private readonly ITokenStore _tokens;
    private readonly ITenantSelection _tenantSelection;
    private readonly IConfiguration _config;

    public ApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor,
                     ITokenStore tokens, ITenantSelection tenantSelection, IConfiguration config)
    {
        _factory = factory;
        _accessor = accessor;
        _tokens = tokens;
        _tenantSelection = tenantSelection;
        _config = config;
    }

    private string BaseUrl
    {
        get
        {
<<<<<<< HEAD
            var scheme = _config["Api:Scheme"] ?? "http";
            var host = _config["Api:Host"] ?? "localhost";
=======
            var host = _accessor.HttpContext!.Request.Host.Host;
            var scheme = _config["Api:Scheme"] ?? "http";
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
            var port = _config["Api:Port"] ?? "5095";
            return $"{scheme}://{host}:{port}";
        }
    }

<<<<<<< HEAD
    private string TenantHost
    {
        get
        {
            var port = _accessor.HttpContext!.Request.Host.Port?.ToString() ?? "";
            return _config[$"Tenancy:PortHosts:{port}"] ?? "localhost";
        }
    }

=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
    private HttpClient CreateRaw()
    {
        var client = _factory.CreateClient("api");
        client.BaseAddress = new Uri(BaseUrl);
<<<<<<< HEAD
        client.DefaultRequestHeaders.Add("X-Tenant-Host", TenantHost);
=======
>>>>>>> bcdf37d9dda12d4a7aca61ef0d9fa06bf7e79f43
        return client;
    }

    private HttpClient Create()
    {
        var client = CreateRaw();

        var token = _tokens.GetAccessToken();
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        var tenantId = _tenantSelection.SelectedTenantId;
        if (tenantId is not null)
            client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.Value.ToString());

        return client;
    }

    public async Task<T?> GetAsync<T>(string path)
    {
        using var response = await SendAsync(() => Create().GetAsync(path));

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<T>()
            : default;
    }

    public Task<ApiResponse<T>> PostAsync<T>(string path, object? body = null)
        => SendWithResultAsync<T>(() => Create().PostAsJsonAsync(path, body ?? new { }));

    public Task<ApiResponse<T>> PutAsync<T>(string path, object? body = null)
        => SendWithResultAsync<T>(() => Create().PutAsJsonAsync(path, body ?? new { }));

    public Task<ApiResponse<object>> PatchAsync(string path)
        => SendWithResultAsync<object>(() => Create().PatchAsync(path, null));

    public Task<ApiResponse<object>> DeleteAsync(string path)
        => SendWithResultAsync<object>(() => Create().DeleteAsync(path));

    private async Task<ApiResponse<T>> SendWithResultAsync<T>(Func<Task<HttpResponseMessage>> send)
    {
        using var response = await SendAsync(send);

        if (response.IsSuccessStatusCode)
        {
            T? data = default;

            if (response.StatusCode != HttpStatusCode.NoContent
                && response.Content.Headers.ContentLength is > 0)
            {
                data = await response.Content.ReadFromJsonAsync<T>();
            }

            return new ApiResponse<T>(true, data, null, response.StatusCode);
        }

        var message = await ReadMessageAsync(response);
        return new ApiResponse<T>(false, default, message, response.StatusCode);
    }

    private async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        var response = await send();

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        if (!await TryRefreshAsync())
            return response;

        response.Dispose();
        return await send();
    }

    private async Task<bool> TryRefreshAsync()
    {
        var refreshToken = _tokens.GetRefreshToken();
        if (string.IsNullOrEmpty(refreshToken)) return false;

        using var client = CreateRaw();

        using var response = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest { RefreshToken = refreshToken });

        if (!response.IsSuccessStatusCode)
        {
            _tokens.Clear();
            return false;
        }

        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if (tokens is null) return false;

        _tokens.Save(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAt);
        return true;
    }

    private static async Task<string> ReadMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorPayload>();
            return error?.Message ?? "İşlem başarısız.";
        }
        catch
        {
            return "İşlem başarısız.";
        }
    }

    private record ErrorPayload(string Message);
}
