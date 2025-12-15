var builder = DistributedApplication.CreateBuilder(args);

// --- REDIS ---
var cache = builder
    .AddAzureRedis("cache")
    .RunAsContainer(rb => rb
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataVolume(isReadOnly: false)
        .WithPersistence(TimeSpan.FromMinutes(1), 100));

// --- SQL SERVER ---
// Usa una sola definizione pulita. 
// Nota: Se usi AddAzureSqlServer ma vuoi testare in locale, RunAsContainer è corretto.
// Tuttavia, spesso si usa semplicemente builder.AddSqlServer("sql") per coerenza locale.
var sqlServer = builder.AddAzureSqlServer("azuresql") 
    .RunAsContainer(config =>
    {
        config.WithLifetime(ContainerLifetime.Persistent); // Ottimo per evitare riavvii lenti
        config.WithDataVolume();
    });

// Aggiungi il database all'istanza del server
var sampleDb = sqlServer.AddDatabase("sampledb");

// --- FRONTEND ---
builder.AddProject<Projects.Aspire_Shield_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    // .WithHttpHealthCheck("/health") // <-- Rimuovi o commenta temporaneamente se fallisce qui
    .WithReference(cache)
    .WithReference(sampleDb)
    .WaitFor(cache)
    .WaitFor(sampleDb); // Questo dice ad Aspire di aspettare che il DB sia creato

builder.Build().Run();