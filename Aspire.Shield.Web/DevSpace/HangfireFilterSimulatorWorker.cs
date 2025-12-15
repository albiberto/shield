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

        // Leggiamo dalla coda
        await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            // MODIFICA CHIAVE:
            // Non usiamo 'await' qui. Lanciamo il task e lo lasciamo andare.
            // Usiamo Task.Run per assicurarci che vada subito su un thread separato.
            _ = Task.Run(() => ProcessJobSafeAsync(job, stoppingToken), stoppingToken);
        }
    }

    // Wrapper per gestire le eccezioni del singolo job parallelo
    private async Task ProcessJobSafeAsync(SampleModel job, CancellationToken token)
    {
        try
        {
            await ProcessJobAsync(job, token);
        }
        catch (Exception ex)
        {
            // Ora il catch è qui, perché il ciclo principale non aspetta questo task
            logger.LogError(ex, "Errore processando il job {Key}", job.Key);
        }
    }

    private async Task ProcessJobAsync(SampleModel job, CancellationToken token)
    {
        // FASE 1: Enqueued
        EmitState(job, SampleModel.StateEnum.Enqueued);
        logger.LogInformation("Job {Key}: ENQUEUED", job.Key);
        // Simuliamo tempo variabile per rendere la concorrenza più evidente
        await Delay(TimeSpan.FromSeconds(2), token);

        // FASE 2: Processing
        EmitState(job, SampleModel.StateEnum.Processing);
        logger.LogInformation("Job {Key}: PROCESSING", job.Key);
        await Delay(TimeSpan.FromSeconds(5), token);

        // FASE 3: Completed
        EmitState(job, SampleModel.StateEnum.Completed);
        logger.LogInformation("Job {Key}: COMPLETED", job.Key);
    }

    private void EmitState(SampleModel baseModel, SampleModel.StateEnum state)
    {
        // Nota sulla Thread-Safety: Se ReactiveService usa Subject semplice, 
        // chiamarlo da thread diversi contemporaneamente potrebbe richiedere un lock
        // lato service o Subject.Synchronize(). 
        // Per questo PoC va bene così, ma in prod controlla ReactiveService.
        lock (reactive) 
        {
            var update = new SampleModel.WithState(baseModel.BusinessUnit, baseModel.Branch, state);
            reactive.OnNext(update);
        }
    }
}