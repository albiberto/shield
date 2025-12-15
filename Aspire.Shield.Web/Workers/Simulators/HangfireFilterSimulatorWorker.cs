namespace Aspire.Shield.Web.Workers.Simulators;

public class HangfireFilterSimulatorWorker : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => throw new NotImplementedException();
}