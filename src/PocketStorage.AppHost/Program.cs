using Microsoft.Extensions.Configuration;
using PocketStorage.Domain.Options;
using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

string? solutionLocation = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.FullName ?? throw new InvalidOperationException();
IConfigurationRoot globalSettings = new ConfigurationBuilder()
    .SetBasePath(solutionLocation)
    .AddJsonFile("appsettings.json")
    .Build();

ApplicationSettings applicationSettings = globalSettings.GetSection(ApplicationSettings.SectionName).Get<ApplicationSettings>() ?? throw new InvalidOperationException();

IResourceBuilder<RedisContainerResource> redis = builder.AddRedisContainer("pocketstorage.redis");
IResourceBuilder<PostgresContainerResource> postgres = builder.AddPostgresContainer("pocketstorage.database", applicationSettings.Postgresql.Port, applicationSettings.Postgresql.Password);

postgres.WithEnvironment("POSTGRES_DB", applicationSettings.Postgresql.Database);
postgres.WithEnvironment("POSTGRES_USER", applicationSettings.Postgresql.Username);
postgres.WithEnvironment("POSTGRES_PASSWORD", applicationSettings.Postgresql.Password);

IResourceBuilder<ProjectResource> identityServer = builder.AddProject<PocketStorage_IdentityServer>("pocketstorage.identityserver")
    .WithReference(redis);

IResourceBuilder<ProjectResource> resourceServer = builder.AddProject<PocketStorage_ResourceServer>("pocketstorage.resourceserver")
    .WithReference(redis);

builder.Build().Run();
