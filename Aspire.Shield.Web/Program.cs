using Aspire.Shield.Web;
using Aspire.Shield.Web.Components;
using Aspire.Shield.Web.Infrastructure;
using Aspire.Shield.Web.Services;
using Aspire.Shield.Web.Workers;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");
builder.AddRedisDistributedCache("cache");
// SQL SERVER - MODIFICA QUI
// Invece di chiamarlo e basta, configuriamo le opzioni di resilienza
builder.AddSqlServerDbContext<ApplicationContext>("sampledb", settings =>
{
    // Abilita i tentativi automatici in caso di fallimento temporaneo
    settings.DisableRetry = false; 
    
    // Aumenta il timeout dei comandi (utile all'avvio quando il PC è sotto carico)
    settings.CommandTimeout = 45; 
});

builder.Services.AddSingleton<ReactiveService>();
builder.Services.AddHostedService<CacheWorker>();
builder.Services.AddHostedService<SourceWorker>();
builder.Services.AddHostedService<DatabaseMigrationWorker>();
builder.Services.AddHostedService<UserSimulatorWorker>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
