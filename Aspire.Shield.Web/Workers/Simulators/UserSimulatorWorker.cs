using Aspire.Shield.Web.Infrastructure;
using Aspire.Shield.Web.Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;

namespace Aspire.Shield.Web.Workers;

public class UserSimulatorWorker(IServiceProvider services, ILogger<UserSimulatorWorker> logger) : BackgroundService
{
    private readonly HashSet<string> _businessUnits = ["Finance", "HR", "IT", "Marketing", "Sales"];
    private readonly HashSet<string> _branches = ["New York", "Los Angeles", "Chicago", "Houston", "Phoenix"];
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DatabaseProducerWorker avviato.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Uso CreateAsyncScope per coerenza e migliore gestione asincrona
                await using var scope = services.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                foreach (var branch in _branches)
                {
                    foreach (var unit in _businessUnits)
                    {
                        if (stoppingToken.IsCancellationRequested) break;

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
                            // Nota: Entity Framework traccia già l'entità, Update non è strettamente necessario se modifichi le proprietà, 
                            // ma va bene lasciarlo per esplicitezza.
                            var updated = old with { Count = Random.Shared.Next(10, 100) };
                            context.Entry(old).CurrentValues.SetValues(updated);
                        }

                        await context.SaveChangesAsync(stoppingToken);
                        await Task.Delay(500, stoppingToken);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Stop grazioso
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errore durante la generazione dei dati.");
        }
        finally
        {
            logger.LogInformation("DatabaseProducerWorker arrestato.");
        }
    }
}