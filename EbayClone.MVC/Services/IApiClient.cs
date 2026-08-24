namespace EbayClone.MVC.Services;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken);
    Task<T?> PutAsync<T>(string path, object? body, CancellationToken cancellationToken);
    Task<byte[]> GetFileAsync(string path, CancellationToken cancellationToken);
}
