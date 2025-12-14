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
        using var subscription = Observable.Interval(TimeSpan.FromSeconds(5))
            .SelectMany(async _ =>
            {
                try
                {
                    using var scope = services.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                    var result = await context.Samples
                        .Select(sample => new SampleModel(sample.BusinessUnit, sample.Branch, sample.Count))
                        .ToListAsync(stoppingToken);

                    return result;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Errore durante il recupero dei dati dal DB");
                    return [];
                }
            })
            .SelectMany(list => list)
            .Subscribe(
                onNext: reactive.OnNext,
                onError: ex => logger.LogCritical(ex, "Errore critico: lo stream Rx è terminato.") 
            );

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Stop pulito
        }
    }
}
