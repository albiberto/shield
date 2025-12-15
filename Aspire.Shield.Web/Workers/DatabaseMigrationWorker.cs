using Aspire.Shield.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Aspire.Shield.Web.Workers;

public sealed class DatabaseMigrationWorker(IServiceProvider services, ILogger<DatabaseMigrationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DatabaseMigrationWorker avviato.");

        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timeSpan, retryCount, _) => logger.LogWarning(exception, "Migrazione DB fallita. Riprovo tra {TimeSpan} secondi (Tentativo {RetryCount}/10)", timeSpan.TotalSeconds, retryCount));

        try
        {
            await policy.ExecuteAsync(async token =>
            {
                await using var scope = services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                await db.Database.MigrateAsync(token);
            }, stoppingToken);

            logger.LogInformation("Migrazione DB completata con successo.");
        }
        catch (OperationCanceledException)
        {
            // Managed Stop
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Database migration failed definitively after 10 retries.");
            throw;
        }
        finally
        {
            logger.LogInformation("DatabaseMigrationWorker terminato.");
        }
    }
}