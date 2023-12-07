using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PocketStorage.Data.Migrations
{
    /// <inheritdoc />
    public partial class OpenIdDictTablesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_claims_users_user_id",
                schema: "Application",
                table: "UserClaims");

            migrationBuilder.DropForeignKey(
                name: "fk_user_logins_users_user_id",
                schema: "Application",
                table: "UserLogins");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_roles_role_id",
                schema: "Application",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_users_user_id",
                schema: "Application",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_user_tokens_users_user_id",
                schema: "Application",
                table: "UserTokens");

            migrationBuilder.RenameColumn(
                name: "type",
                schema: "OpenId",
                table: "Applications",
                newName: "client_type");

            migrationBuilder.AddColumn<string>(
                name: "application_type",
                schema: "OpenId",
                table: "Applications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "json_web_key_set",
                schema: "OpenId",
                table: "Applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "settings",
                schema: "OpenId",
                table: "Applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_user_claims_users_user_id",
                schema: "Application",
                table: "UserClaims",
                column: "user_id",
                principalSchema: "Application",
                principalTable: "Users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_logins_users_user_id",
                schema: "Application",
                table: "UserLogins",
                column: "user_id",
                principalSchema: "Application",
                principalTable: "Users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_roles_role_id",
                schema: "Application",
                table: "UserRoles",
                column: "role_id",
                principalSchema: "Application",
                principalTable: "Roles",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_users_user_id",
                schema: "Application",
                table: "UserRoles",
                column: "user_id",
                principalSchema: "Application",
                principalTable: "Users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_tokens_users_user_id",
                schema: "Application",
                table: "UserTokens",
                column: "user_id",
                principalSchema: "Application",
                principalTable: "Users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_claims_users_user_id",
                schema: "Application",
                table: "UserClaims");

            migrationBuilder.DropForeignKey(
                name: "fk_user_logins_users_user_id",
                schema: "Application",
                table: "UserLogins");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_roles_role_id",
                schema: "Application",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_users_user_id",
                schema: "Application",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_user_tokens_users_user_id",
                schema: "Application",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "application_type",
                schema: "OpenId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "json_web_key_set",
                schema: "OpenId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "settings",
                schema: "OpenId",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "client_type",
                schema: "OpenId",
                table: "Applications",
                newName: "type");

            migrationBuilder.AddForeignKey(
                name: "fk_user_claims_users_user_id",
                schema: "Application",
                table: "UserClaims",
                column: "user_id",
                principalSchema: "Application",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_logins_users_user_id",
                schema: "Application",
                table: "UserLogins",
                column: "user_id",
                principalSchema: "Application",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_roles_role_id",
                schema: "Application",
                table: "UserRoles",
                column: "role_id",
                principalSchema: "Application",
                principalTable: "Roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_users_user_id",
                schema: "Application",
                table: "UserRoles",
                column: "user_id",
                principalSchema: "Application",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_tokens_users_user_id",
                schema: "Application",
                table: "UserTokens",
                column: "user_id",
                principalSchema: "Application",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
