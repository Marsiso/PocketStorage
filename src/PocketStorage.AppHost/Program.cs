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

IResourceBuilder<RedisContainerResource> redis = builder.AddRedisContainer("redis-cache");
IResourceBuilder<PostgresContainerResource> postgres = builder.AddPostgresContainer("postgresql", applicationSettings.Postgresql.Port, applicationSettings.Postgresql.Password);

postgres.WithEnvironment("POSTGRES_DB", applicationSettings.Postgresql.Database);
postgres.WithEnvironment("POSTGRES_USER", applicationSettings.Postgresql.Username);
postgres.WithEnvironment("POSTGRES_PASSWORD", applicationSettings.Postgresql.Password);

IResourceBuilder<ContainerResource> grafana = builder.AddContainer("grafana", "grafana/grafana")
    .WithVolumeMount("../../grafana/config", "/etc/grafana")
    .WithVolumeMount("../../grafana/dashboards", "/var/lib/grafana/dashboards")
    .WithServiceBinding(3000, 3000, name: "grafana-http", scheme: "http");

IResourceBuilder<ContainerResource> prometheus = builder.AddContainer("prometheus", "prom/prometheus")
    .WithVolumeMount("../../prometheus", "/etc/prometheus")
    .WithServiceBinding(9090, 9090);

IResourceBuilder<ProjectResource> identityServer = builder.AddProject<PocketStorage_IdentityServer>("identity-server")
    .WithReference(redis);

IResourceBuilder<ProjectResource> resourceServer = builder.AddProject<PocketStorage_ResourceServer>("resource-server")
    .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("grafana-http"))
    .WithReference(redis);

await using DistributedApplication application = builder.Build();
await application.RunAsync();
