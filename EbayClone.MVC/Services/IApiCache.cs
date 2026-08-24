namespace EbayClone.MVC.Services;

public interface IApiCache
{
    bool TryGet<T>(string key, out T? value);
    void Set<T>(string key, T? value, TimeSpan lifetime);
}
