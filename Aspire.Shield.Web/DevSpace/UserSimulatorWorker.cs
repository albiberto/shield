using Aspire.Shield.Web.Infrastructure;
using Aspire.Shield.Web.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;

namespace Aspire.Shield.Web.DevSpace;

public class UserSimulatorWorker(IServiceProvider services, SimulatorOptions options, ILogger<UserSimulatorWorker> logger) : BaseSimulatorWorker(TimeSpan.FromSeconds(1), logger)
{
    protected override async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        foreach (var branch in options.Branches)
        {
            foreach (var unit in options.BusinessUnits)
            {
                if (stoppingToken.IsCancellationRequested) return;

                var item = await context.Samples.SingleOrDefaultAsync(s => s.BusinessUnit == unit && s.Branch == branch, stoppingToken);

                if (item is null)
                {
                    var newItem = new Sample(unit, branch, 0);
                    await context.AddAsync(newItem, stoppingToken);
                }
                else
                {
                    var updatedItem = item with { Count = Random.Shared.Next(10, 100) };
                    context.Entry(item).CurrentValues.SetValues(updatedItem);
                }
                
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}