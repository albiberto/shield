using System.Threading.Channels;
using Aspire.Shield.Web.Model;
using Aspire.Shield.Web.Services;
using static System.Threading.Tasks.Task;

namespace Aspire.Shield.Web.DevSpace;

public class HangfireFilterSimulatorWorker(ReactiveService reactive, ILogger<HangfireFilterSimulatorWorker> logger) : BackgroundService
{
    private readonly Channel<SampleModel> _queue = Channel.CreateUnbounded<SampleModel>();

    public void EnqueueJob(SampleModel model)
    {
        _queue.Writer.TryWrite(model);
        logger.LogInformation("Job accodato per {Key}", model.Key);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("JobProcessingWorker avviato e in attesa di messaggi...");

        await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessJobAsync(job, stoppingToken);
                await Delay(2000, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore processando il job {Key}", job.Key);
            }
        }
    }

    private async Task ProcessJobAsync(SampleModel job, CancellationToken token)
    {
        // FASE 1: Enqueued (Appena prelevato dalla coda)
        EmitState(job, SampleModel.StateEnum.Enqueued);
        logger.LogInformation("Job {Key}: ENQUEUED", job.Key);
        await Delay(TimeSpan.FromSeconds(5), token);

        // FASE 2: Processing (Dopo 5 secondi)
        EmitState(job, SampleModel.StateEnum.Processing);
        logger.LogInformation("Job {Key}: PROCESSING", job.Key);
        await Delay(TimeSpan.FromSeconds(5), token);

        // FASE 3: Completed (Dopo altri 5 secondi)
        EmitState(job, SampleModel.StateEnum.Completed);
        logger.LogInformation("Job {Key}: COMPLETED", job.Key);
        
        // Non serve delay qui, il lavoro è finito
    }

    private void EmitState(SampleModel baseModel, SampleModel.StateEnum state)
    {
        var update = new SampleModel.WithState(baseModel.BusinessUnit, baseModel.Branch, state);
        reactive.OnNext(update);
    }
}