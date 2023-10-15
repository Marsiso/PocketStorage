using CommunityToolkit.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PocketStorage.Application.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static void Configure(this DbContextOptionsBuilder builder, bool isDevelopment, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.IsNotNullOrWhiteSpace(connectionString);

        builder.UseNpgsql(connectionString);
        builder.UseOpenIddict();
        builder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        builder.EnableDetailedErrors(isDevelopment);
        builder.EnableSensitiveDataLogging(isDevelopment);
    }
}
