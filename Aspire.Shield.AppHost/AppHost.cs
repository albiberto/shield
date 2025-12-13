var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddAzureRedis("cache");

var sqlServer = builder.AddSqlServer("sqlserver")
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