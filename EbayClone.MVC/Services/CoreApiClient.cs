using System.Net.Http.Headers;
using System.Net.Http.Json;
using EbayClone.MVC.Models;

namespace EbayClone.MVC.Services;

public class CoreApiClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    IApiCache apiCache) : IApiClient
{
    public const string CacheKeyPrefix = "admin-api-cache:";

    public Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken) =>
        SendAsync<T>(HttpMethod.Get, path, null, true, cancellationToken);

    public Task<T?> PutAsync<T>(string path, object? body, CancellationToken cancellationToken) =>
        SendAsync<T>(HttpMethod.Put, path, body, true, cancellationToken);

    public async Task<byte[]> GetFileAsync(string path, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Get, path, true);
        try
        {
            using var response = await SendWithErrorHandlingAsync(request, cancellationToken);
            var result = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            MarkOnline();
            return result;
        }
        catch (AdminApiException exception) when (exception.StatusCode == 503)
        {
            MarkOffline();
            throw;
        }
        catch (HttpRequestException exception)
        {
            MarkOffline();
            throw new AdminApiException(503, "Không thể kết nối Admin API.", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            MarkOffline();
            throw new AdminApiException(503, "Admin API phản hồi quá lâu.", exception);
        }
    }

    protected async Task<T?> SendAsync<T>(
        HttpMethod method,
        string path,
        object? body,
        bool authorize,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, path, authorize);
        if (body is not null) request.Content = JsonContent.Create(body);

        try
        {
            using var response = await SendWithErrorHandlingAsync(request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            if (method == HttpMethod.Get && authorize)
                apiCache.Set(GetCacheKey(path), result, TimeSpan.FromMinutes(30));
            MarkOnline();
            return result;
        }
        catch (HttpRequestException exception)
        {
            MarkOffline();
            if (method == HttpMethod.Get && authorize && apiCache.TryGet(GetCacheKey(path), out T? cached)) return cached;
            throw new AdminApiException(503, "Không thể kết nối Admin API.", exception);
        }
        catch (AdminApiException exception) when (exception.StatusCode is 502 or 503 or 504)
        {
            MarkOffline();
            if (method == HttpMethod.Get && authorize && apiCache.TryGet(GetCacheKey(path), out T? cached)) return cached;
            throw;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            MarkOffline();
            if (method == HttpMethod.Get && authorize && apiCache.TryGet(GetCacheKey(path), out T? cached)) return cached;
            throw new AdminApiException(503, "Admin API phản hồi quá lâu.", exception);
        }
    }

    protected HttpRequestMessage CreateRequest(HttpMethod method, string path, bool authorize)
    {
        var request = new HttpRequestMessage(method, path);
        if (!authorize) return request;

        var token = httpContextAccessor.HttpContext?.Session.GetString("AdminToken");
        if (string.IsNullOrWhiteSpace(token))
            throw new AdminApiException(401, "Phiên đăng nhập đã hết hạn.");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private async Task<HttpResponseMessage> SendWithErrorHandlingAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            var statusCode = (int)response.StatusCode;
            response.Dispose();
            throw new AdminApiException(statusCode, message);
        }
        return response;
    }

    protected string GetCacheKey(string path) => $"{CacheKeyPrefix}{path}";
    protected void MarkOffline() => httpContextAccessor.HttpContext?.Session.SetString("OfflineMode", "true");
    protected void MarkOnline() => httpContextAccessor.HttpContext?.Session.Remove("OfflineMode");
}
