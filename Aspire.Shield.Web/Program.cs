using Aspire.Shield.Web;
using Aspire.Shield.Web.Components;
using Aspire.Shield.Web.Infrastructure;
using Aspire.Shield.Web.Workers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");
builder.AddRedisDistributedCache("cache");
builder.AddSqlServerDbContext<ApplicationContext>("sqlserver");

builder.Services.AddHostedService<CacheWorker>();
builder.Services.AddHostedService<SourceWorker>();
builder.Services.AddHostedService<MigrationWorker>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
await db.Database.MigrateAsync();

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