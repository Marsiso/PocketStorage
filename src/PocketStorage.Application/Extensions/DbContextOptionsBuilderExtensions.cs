using CommunityToolkit.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PocketStorage.Domain.Options;

namespace PocketStorage.Application.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder Configure(this DbContextOptionsBuilder builder, bool development, ApplicationSettings settings)
    {
        string connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = settings.Postgresql.Host,
            Port = settings.Postgresql.Port,
            Database = settings.Postgresql.Database,
            Username = settings.Postgresql.Username,
            Password = settings.Postgresql.Password,
            Pooling = settings.Postgresql.Pooling
        }.ConnectionString;

        Guard.IsNotNullOrWhiteSpace(connectionString);

        builder.UseNpgsql(connectionString);
        builder.UseOpenIddict();
        builder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        builder.EnableDetailedErrors(development);
        builder.EnableSensitiveDataLogging(development);

        return builder;
    }
}
