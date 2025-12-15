namespace Aspire.Shield.Web.DevSpace;

public abstract class BaseSimulatorWorker(TimeSpan interval, ILogger logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workerName = GetType().Name;
        logger.LogInformation("{WorkerName} avviato (Intervallo: {Interval}s).", workerName, interval.TotalSeconds);

        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await DoWorkAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Errore durante il ciclo di lavoro di {WorkerName}.", workerName);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Stop grazioso
        }
        finally
        {
            logger.LogInformation("{WorkerName} arrestato.", workerName);
        }
    }

    protected abstract Task DoWorkAsync(CancellationToken stoppingToken);
}