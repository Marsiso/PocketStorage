using Microsoft.EntityFrameworkCore.Migrations;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Constants;
using PocketStorage.Domain.Enums;

#nullable disable

namespace PocketStorage.Data.Migrations
{
    /// <inheritdoc />
    public partial class RoleDataMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                Tables.Roles,
                new string[] { "id", "name", "normalized_name", "permissions", "date_created", "date_updated", "is_active", "concurrency_stamp" },
                new object[,]
                {
                    { Guid.NewGuid().ToString(), RoleDefaults.Administrator, RoleDefaults.Administrator.ToUpperInvariant(), (int)Permission.All, DateTime.UtcNow, DateTime.UtcNow, true, Guid.NewGuid().ToString() },
                    { Guid.NewGuid().ToString(), RoleDefaults.Manager, RoleDefaults.Manager.ToUpperInvariant(), (int)(Permission.ViewUsers | Permission.EditUsers | Permission.ViewFiles | Permission.EditFiles), DateTime.UtcNow, DateTime.UtcNow, true, Guid.NewGuid().ToString() },
                    { Guid.NewGuid().ToString(), RoleDefaults.Default, RoleDefaults.Default.ToUpperInvariant(), (int)Permission.None, DateTime.UtcNow, DateTime.UtcNow, true, Guid.NewGuid().ToString() }
                }, Schemas.Application);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
