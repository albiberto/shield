using Aspire.Shield.ServiceDefaults;
using Aspire.Shield.Web.Components;
using Aspire.Shield.Web.DevSpace;
using Aspire.Shield.Web.Infrastructure;
using Aspire.Shield.Web.Services;
using Aspire.Shield.Web.Workers;
using Serilog;
using Serilog.Events;

// Configura Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("🚀 Avvio applicazione Aspire.Shield.Web");

    var builder = WebApplication.CreateBuilder(args);

    // Usa Serilog come provider di logging
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services
        .AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.AddServiceDefaults();

    // Entity Framework & Database Migration Worker
    builder.Services.AddHostedService<DatabaseMigrationWorker>();
    builder.AddSqlServerDbContext<ApplicationContext>("sampledb", settings =>
    {
        settings.DisableRetry = false;
        settings.CommandTimeout = 45;
    });

    // Simulators
    builder.Services.AddSingleton<SimulatorOptions>();
    builder.Services.AddHostedService<UserSimulatorWorker>();
    builder.Services.AddSingleton<HangfireFilterSimulatorWorker>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<HangfireFilterSimulatorWorker>());

    // Application Services
    builder.Services.AddSingleton<ReactiveService>();
    builder.Services.AddHostedService<SourceWorker>();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseAntiforgery();

    app.MapStaticAssets();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    Log.Information("✅ Applicazione avviata con successo");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Applicazione terminata in modo anomalo");
    throw;
}
finally
{
    Log.CloseAndFlush();
}