using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RedisContainerResource> redis = builder.AddRedisContainer("pocketstorage.redis");

builder.AddProject<PocketStorage_IdentityServer>("pocketstorage.identityserver")
    .WithReference(redis);

builder.AddProject<PocketStorage_ResourceServer>("pocketstorage.resourceserver")
    .WithReference(redis);

builder.Build().Run();
