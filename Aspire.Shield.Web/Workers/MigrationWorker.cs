using Aspire.Shield.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Aspire.Shield.Web.Workers;

public sealed class MigrationWorker(IServiceProvider services, ILogger<MigrationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            await db.Database.MigrateAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed");
            throw;
        }
    }
}
