using Aspire.Shield.Web.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using static System.Threading.Tasks.Task;

namespace Aspire.Shield.Web.Workers;

public class CacheWorker(ReactiveService reactive, IDistributedCache cache, ILogger<CacheWorker> logger) : BackgroundService
{
    private const string CacheKey = "LatestSample";

    private IAsyncDisposable? _subscription;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _subscription = await reactive.Observable.SubscribeAsync(async sample =>
        {
            var json = System.Text.Json.JsonSerializer.Serialize(sample);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            await cache.SetAsync(CacheKey, bytes, token: stoppingToken);
        });

        try
        {
            await Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Stop Pulito
        }
    }

    public override void Dispose()
    {
        _subscription?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        base.Dispose();
    }
}