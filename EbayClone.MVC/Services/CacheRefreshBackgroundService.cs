namespace EbayClone.MVC.Services;

public sealed class CacheRefreshBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<CacheRefreshBackgroundService> logger) : BackgroundService
{
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue<bool>("CacheRefresh:Enabled"))
        {
            logger.LogInformation("Cache refresh worker is disabled.");
            return;
        }

        var intervalHours = Math.Max(1, configuration.GetValue("CacheRefresh:IntervalHours", 6));
        await RefreshOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(intervalHours));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await RefreshOnceAsync(stoppingToken);
    }

    private async Task RefreshOnceAsync(CancellationToken cancellationToken)
    {
        if (!await RefreshLock.WaitAsync(TimeSpan.Zero, cancellationToken))
        {
            logger.LogWarning("Cache refresh skipped because another refresh is already running.");
            return;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<CacheRefreshApiClient>()
                .RefreshAsync(cancellationToken);
            logger.LogInformation("API cache refresh completed at {RefreshTimeUtc}.", DateTime.UtcNow);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Application shutdown.
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "API cache refresh failed; existing cache was kept.");
            try
            {
                using var scope = scopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<AdminNotificationService>()
                    .BroadcastAsync("Không thể làm mới dữ liệu cache; hệ thống vẫn giữ dữ liệu gần nhất.", "warning", cancellationToken);
            }
            catch (Exception notificationException)
            {
                logger.LogDebug(notificationException, "Unable to broadcast cache refresh warning.");
            }
        }
        finally
        {
            RefreshLock.Release();
        }
    }
}
