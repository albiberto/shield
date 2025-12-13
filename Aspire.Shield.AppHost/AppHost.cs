var builder = DistributedApplication.CreateBuilder(args);

var cache = builder
    .AddAzureRedis("cache")
    .RunAsContainer(resourceBuilder =>
        resourceBuilder
            .WithRedisInsight()
            .WithLifetime(ContainerLifetime.Persistent)
            .WithDataVolume(isReadOnly: false)
            .WithPersistence(TimeSpan.FromMinutes(1), 100));

var sqlServer = builder
    .AddSqlServer("sqlserver")
    .WithDataVolume()
    .AddDatabase("sampledb");

builder.AddProject<Projects.Aspire_Shield_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WithReference(sqlServer)
    .WaitFor(cache)
    .WaitFor(sqlServer);

builder.Build().Run();