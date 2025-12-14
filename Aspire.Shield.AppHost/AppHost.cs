var builder = DistributedApplication.CreateBuilder(args);

// --- REDIS (Questo va bene) ---
var cache = builder
    .AddAzureRedis("cache")
    .RunAsContainer(rb => rb
        .WithRedisInsight()
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataVolume(isReadOnly: false)
        .WithPersistence(TimeSpan.FromMinutes(1), 100));

// --- SQL SERVER ---
// 1. Definisci il container fisico (Il Server)
var sqlServer = builder
    .AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume(); // Volume per il server

// 2. Definisci il database logico collegato a quel server
var sampleDb = sqlServer.AddDatabase("sampledb");

// --- FRONTEND ---
builder.AddProject<Projects.Aspire_Shield_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WithReference(sampleDb) // Riferisci specificamente il DB, non tutto il server
    .WaitFor(cache)
    .WaitFor(sqlServer); // Aspetti che il container SQL sia su

builder.Build().Run();