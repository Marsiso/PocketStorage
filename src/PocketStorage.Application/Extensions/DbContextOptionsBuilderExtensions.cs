using CommunityToolkit.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PocketStorage.Domain.Options;

namespace PocketStorage.Application.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static void Configure(this DbContextOptionsBuilder builder, bool development, Settings settings)
    {
        string connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = settings.Database.Host,
            Port = settings.Database.Port,
            Database = settings.Database.Database,
            Username = settings.Database.Username,
            Password = settings.Database.Password,
            Pooling = settings.Database.Pooling
        }.ConnectionString;

        Guard.IsNotNullOrWhiteSpace(connectionString);

        builder.UseNpgsql(connectionString);
        builder.UseOpenIddict();
        builder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        builder.EnableDetailedErrors(development);
        builder.EnableSensitiveDataLogging(development);
    }
}
