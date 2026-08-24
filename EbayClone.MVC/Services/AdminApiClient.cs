using EbayClone.MVC.Models;

namespace EbayClone.MVC.Services;

public sealed class AdminApiClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    IApiCache apiCache)
    : CoreApiClient(httpClient, httpContextAccessor, apiCache)
{
    public Task<LoginResponseModel?> LoginAsync(LoginInputModel input, CancellationToken cancellationToken) =>
        SendAsync<LoginResponseModel>(HttpMethod.Post, "api/auth/login", input, false, cancellationToken);
}

public class AdminApiException(int statusCode, string message, Exception? innerException = null) : Exception(message, innerException)
{
    public int StatusCode { get; } = statusCode;
}
