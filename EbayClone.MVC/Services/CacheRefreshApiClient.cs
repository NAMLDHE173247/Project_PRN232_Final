using System.Net.Http.Headers;
using System.Net.Http.Json;
using EbayClone.MVC.Models;

namespace EbayClone.MVC.Services;

public sealed class CacheRefreshApiClient(
    HttpClient httpClient,
    IApiCache apiCache,
    IConfiguration configuration,
    ILogger<CacheRefreshApiClient> logger)
{
    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var email = configuration["CacheRefresh:Email"];
        var password = configuration["CacheRefresh:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("CacheRefresh credentials are not configured.");

        var login = await SendAsync<LoginResponseModel>(
            HttpMethod.Post,
            "api/auth/login",
            new LoginInputModel { Email = email, Password = password },
            null,
            cancellationToken);
        if (login is null || !string.Equals(login.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("CacheRefresh account must have the Admin role.");

        await RefreshAsync<DashboardViewModel>("api/admin/dashboard", login.Token, cancellationToken);
        await RefreshAsync<PagedViewModel<AdminUserViewModel>>("api/admin/users?page=1&pageSize=20", login.Token, cancellationToken);
        await RefreshAsync<PagedViewModel<AdminProductViewModel>>("api/admin/products?page=1&pageSize=20", login.Token, cancellationToken);
        await RefreshAsync<PagedViewModel<OrderViewModel>>("api/admin/orders?page=1&pageSize=20", login.Token, cancellationToken);
        await RefreshAsync<PagedViewModel<DisputeViewModel>>("api/admin/disputes?page=1&pageSize=20", login.Token, cancellationToken);
        await RefreshAsync<PagedViewModel<ReturnRequestViewModel>>("api/admin/return-requests?page=1&pageSize=20", login.Token, cancellationToken);
        await RefreshAsync<PagedViewModel<AdminReviewViewModel>>("api/admin/reviews?page=1&pageSize=20", login.Token, cancellationToken);
        await RefreshAsync<PagedViewModel<AdminFeedbackViewModel>>("api/admin/feedbacks?page=1&pageSize=20", login.Token, cancellationToken);
    }

    private async Task RefreshAsync<T>(string path, string token, CancellationToken cancellationToken)
    {
        var result = await SendAsync<T>(HttpMethod.Get, path, null, token, cancellationToken);
        apiCache.Set($"{CoreApiClient.CacheKeyPrefix}{path}", result, TimeSpan.FromHours(6));
        logger.LogDebug("Refreshed API cache for {Path}", path);
    }

    private async Task<T?> SendAsync<T>(
        HttpMethod method,
        string path,
        object? body,
        string? token,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            request.Content = JsonContent.Create(body);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"{path} returned {(int)response.StatusCode}: {message}");
        }

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }
}
