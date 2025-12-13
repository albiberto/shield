using Aspire.Shield.Web.Infrastructure;
using static System.Threading.Tasks.Task;

namespace Aspire.Shield.Web.Workers;

public class SourceWorker(ApplicationContext context) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Delay(Timeout.Infinite, stoppingToken);
    }
}