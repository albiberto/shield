using Aspire.Shield.Web.Infrastructure;
using Aspire.Shield.Web.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using static System.Threading.Tasks.Task;

namespace Aspire.Shield.Web.Workers;

public class DatabaseProducerWorker(IServiceScopeFactory factory) : BackgroundService
{
    private readonly HashSet<string> BusinessUnits =
    [
        "Finance",
        "HR",
        "IT",
        "Marketing",
        "Sales"
    ];
    
    private readonly HashSet<string> Branches =
    [
        "New York",
        "Los Angeles",
        "Chicago",
        "Houston",
        "Phoenix"
    ];
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = factory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                foreach (var branch in Branches)
                {
                    foreach (var unit in BusinessUnits)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;

                        var old = await context.Samples.SingleOrDefaultAsync(
                            s => s.BusinessUnit == unit && s.Branch == branch, 
                            cancellationToken: stoppingToken);

                        if (old is null)
                        {
                            var @new = new Sample(unit, branch, 0);
                            await context.AddAsync(@new, stoppingToken);
                        }
                        else
                        {
                            var @new = old with { Count = Random.Shared.Next(10, 100) };
                            context.Update(@new);
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    
                        try
                        {
                            await Task.Delay(500, stoppingToken);
                        }
                        catch (OperationCanceledException)
                        {
                            // Normale durante lo shutdown
                            return;
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normale durante lo shutdown
        }
    }

}