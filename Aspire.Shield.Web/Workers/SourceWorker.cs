using System.Reactive.Linq;
using Aspire.Shield.Web.Infrastructure;
using Aspire.Shield.Web.Model;
using Aspire.Shield.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Aspire.Shield.Web.Workers;

public class SourceWorker(ReactiveService reactive, IServiceProvider services, ILogger<SourceWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SourceWorker avviato.");

        using var subscription = Observable.Interval(TimeSpan.FromSeconds(1))
            .SelectMany(async _ =>
            {
                try
                {
                    await using var scope = services.CreateAsyncScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                    var result = await context.Samples
                        .Select(sample => new SampleModel.WithCount(sample.BusinessUnit, sample.Branch, sample.Count))
                        .ToListAsync(stoppingToken);

                    return result;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Errore durante il recupero dei dati dal DB");
                    return [];
                }
            })
            .Where(list => list.Count > 0) // Evita di emettere liste vuote in caso di errore
            .SelectMany(list => list)
            .Subscribe(reactive.OnNext, ex => logger.LogCritical(ex, "Errore critico nello stream Rx."));

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Stop grazioso
        }
        finally
        {
            logger.LogInformation("SourceWorker arrestato.");
        }
    }
}