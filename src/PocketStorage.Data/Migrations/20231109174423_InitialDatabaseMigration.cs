using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PocketStorage.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabaseMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "OpenId");

            migrationBuilder.EnsureSchema(
                name: "Application");

            migrationBuilder.EnsureSchema(
                name: "Files");

            migrationBuilder.CreateTable(
                name: "Applications",
                schema: "OpenId",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    client_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    client_secret = table.Column<string>(type: "text", nullable: true),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    consent_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    display_names = table.Column<string>(type: "text", nullable: true),
                    permissions = table.Column<string>(type: "text", nullable: true),
                    post_logout_redirect_uris = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    redirect_uris = table.Column<string>(type: "text", nullable: true),
                    requirements = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Scopes",
                schema: "OpenId",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    descriptions = table.Column<string>(type: "text", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    display_names = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    resources = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scopes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "Application",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    given_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    family_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    culture = table.Column<string>(type: "text", nullable: false),
                    profile_photo = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_users_user_created_by_id",
                        column: x => x.created_by,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_users_users_user_updated_by_id",
                        column: x => x.updated_by,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Authorizations",
                schema: "OpenId",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    application_id = table.Column<string>(type: "text", nullable: true),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    scopes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authorizations", x => x.id);
                    table.ForeignKey(
                        name: "fk_authorizations_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "OpenId",
                        principalTable: "Applications",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "CodeLists",
                schema: "Application",
                columns: table => new
                {
                    code_list_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_code_lists", x => x.code_list_id);
                    table.ForeignKey(
                        name: "fk_code_lists_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_code_lists_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Application",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    permissions = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_roles_users_user_created_by_id",
                        column: x => x.created_by,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_roles_users_user_updated_by_id",
                        column: x => x.updated_by,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                schema: "Application",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_claims_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                schema: "Application",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_user_logins_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                schema: "Application",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_user_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                schema: "OpenId",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    application_id = table.Column<string>(type: "text", nullable: true),
                    authorization_id = table.Column<string>(type: "text", nullable: true),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payload = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    redemption_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_tokens_open_iddict_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "OpenId",
                        principalTable: "Applications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_tokens_open_iddict_authorizations_authorization_id",
                        column: x => x.authorization_id,
                        principalSchema: "OpenId",
                        principalTable: "Authorizations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "CodeListItems",
                schema: "Application",
                columns: table => new
                {
                    code_list_item_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code_list_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_code_list_items", x => x.code_list_item_id);
                    table.ForeignKey(
                        name: "fk_code_list_items_code_lists_code_list_id",
                        column: x => x.code_list_id,
                        principalSchema: "Application",
                        principalTable: "CodeLists",
                        principalColumn: "code_list_id");
                    table.ForeignKey(
                        name: "fk_code_list_items_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_code_list_items_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                schema: "Application",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_claims_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "Application",
                        principalTable: "Roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "Application",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "Application",
                        principalTable: "Roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                schema: "Files",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    total_size = table.Column<int>(type: "integer", nullable: false),
                    total_count = table.Column<int>(type: "integer", nullable: false),
                    code_list_item_id = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_folders", x => x.id);
                    table.ForeignKey(
                        name: "fk_folders_code_list_items_code_list_item_id",
                        column: x => x.code_list_item_id,
                        principalSchema: "Application",
                        principalTable: "CodeListItems",
                        principalColumn: "code_list_item_id");
                    table.ForeignKey(
                        name: "fk_folders_folders_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "Files",
                        principalTable: "Folders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_folders_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_folders_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_folders_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Files",
                schema: "Files",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    folder_id = table.Column<int>(type: "integer", nullable: false),
                    safe_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    unsafe_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    location = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    extension = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_files", x => x.id);
                    table.ForeignKey(
                        name: "fk_files_folders_folder_id",
                        column: x => x.folder_id,
                        principalSchema: "Files",
                        principalTable: "Folders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_files_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_files_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "Application",
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_applications_client_id",
                schema: "OpenId",
                table: "Applications",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_authorizations_application_id_status_subject_type",
                schema: "OpenId",
                table: "Authorizations",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_code_list_items_code_list_id",
                schema: "Application",
                table: "CodeListItems",
                column: "code_list_id");

            migrationBuilder.CreateIndex(
                name: "ix_code_list_items_created_by",
                schema: "Application",
                table: "CodeListItems",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_code_list_items_is_active",
                schema: "Application",
                table: "CodeListItems",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_code_list_items_updated_by",
                schema: "Application",
                table: "CodeListItems",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_code_list_items_value",
                schema: "Application",
                table: "CodeListItems",
                column: "value");

            migrationBuilder.CreateIndex(
                name: "ix_code_lists_created_by",
                schema: "Application",
                table: "CodeLists",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_code_lists_is_active",
                schema: "Application",
                table: "CodeLists",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_code_lists_name",
                schema: "Application",
                table: "CodeLists",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_code_lists_updated_by",
                schema: "Application",
                table: "CodeLists",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_files_created_by",
                schema: "Files",
                table: "Files",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_files_folder_id",
                schema: "Files",
                table: "Files",
                column: "folder_id");

            migrationBuilder.CreateIndex(
                name: "ix_files_is_active",
                schema: "Files",
                table: "Files",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_files_unsafe_name",
                schema: "Files",
                table: "Files",
                column: "unsafe_name");

            migrationBuilder.CreateIndex(
                name: "ix_files_updated_by",
                schema: "Files",
                table: "Files",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_folders_category_id",
                schema: "Files",
                table: "Folders",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_folders_code_list_item_id",
                schema: "Files",
                table: "Folders",
                column: "code_list_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_folders_created_by",
                schema: "Files",
                table: "Folders",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_folders_is_active",
                schema: "Files",
                table: "Folders",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_folders_parent_id",
                schema: "Files",
                table: "Folders",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_folders_updated_by",
                schema: "Files",
                table: "Folders",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_folders_user_id",
                schema: "Files",
                table: "Folders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_claims_role_id",
                schema: "Application",
                table: "RoleClaims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_roles_created_by",
                schema: "Application",
                table: "Roles",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_roles_is_active",
                schema: "Application",
                table: "Roles",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_roles_updated_by",
                schema: "Application",
                table: "Roles",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "Application",
                table: "Roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scopes_name",
                schema: "OpenId",
                table: "Scopes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tokens_application_id_status_subject_type",
                schema: "OpenId",
                table: "Tokens",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_tokens_authorization_id",
                schema: "OpenId",
                table: "Tokens",
                column: "authorization_id");

            migrationBuilder.CreateIndex(
                name: "ix_tokens_reference_id",
                schema: "OpenId",
                table: "Tokens",
                column: "reference_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_claims_user_id",
                schema: "Application",
                table: "UserClaims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_user_id",
                schema: "Application",
                table: "UserLogins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                schema: "Application",
                table: "UserRoles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "Application",
                table: "Users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "ix_users_created_by",
                schema: "Application",
                table: "Users",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_users_is_active",
                schema: "Application",
                table: "Users",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_users_updated_by",
                schema: "Application",
                table: "Users",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "Application",
                table: "Users",
                column: "normalized_user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files",
                schema: "Files");

            migrationBuilder.DropTable(
                name: "RoleClaims",
                schema: "Application");

            migrationBuilder.DropTable(
                name: "Scopes",
                schema: "OpenId");

            migrationBuilder.DropTable(
                name: "Tokens",
                schema: "OpenId");

            migrationBuilder.DropTable(
                name: "UserClaims",
                schema: "Application");

            migrationBuilder.DropTable(
                name: "UserLogins",
                schema: "Application");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "Application");

            migrationBuilder.DropTable(
                name: "UserTokens",
                schema: "Application");

            migrationBuilder.DropTable(
                name: "Folders",
                schema: "Files");

            migrationBuilder.DropTable(
                name: "Authorizations",
                schema: "OpenId");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Application");

            migrationBuilder.DropTable(
                name: "CodeListItems",
                schema: "Application");

            migrationBuilder.DropTable(
                name: "Applications",
                schema: "OpenId");

            migrationBuilder.DropTable(
                name: "CodeLists",
                schema: "Application");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "Application");
        }
    }
}
