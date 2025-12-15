using System.Text;
using System.Text.Json;
using Aspire.Shield.Web.Services;
using Microsoft.Extensions.Caching.Distributed;
using static System.Threading.Tasks.Task;

namespace Aspire.Shield.Web.Workers;

public class CacheWorker(ReactiveService reactive, IDistributedCache cache, ILogger<CacheWorker> logger) : BackgroundService
{
    private const string CacheKey = "LatestSample";
    private IAsyncDisposable? _subscription;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CacheWorker avviato.");

        _subscription = await reactive.Observable.SubscribeAsync(async sample =>
        {
            try
            {
                var json = JsonSerializer.Serialize(sample);
                var bytes = Encoding.UTF8.GetBytes(json);
                await cache.SetAsync(CacheKey, bytes, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Errore durante l'aggiornamento della cache.");
            }
        });

        try
        {
            await Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Stop grazioso
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errore imprevisto in CacheWorker.");
        }
        finally
        {
            logger.LogInformation("CacheWorker arrestato.");
        }
    }

    public override void Dispose()
    {
        _subscription?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        base.Dispose();
    }
}