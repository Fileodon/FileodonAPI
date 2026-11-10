using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class snake_case : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_organizations_OrganizationId",
                table: "AspNetRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_organization_audit_logs_AspNetUsers_actor_id",
                table: "organization_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_organization_audit_logs_organizations_organization_id",
                table: "organization_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_organizations_AspNetUsers_UserOwnerId",
                table: "organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_AspNetUsers_UserId",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_role_audit_logs_AspNetRoles_role_id",
                table: "role_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_role_audit_logs_AspNetUsers_actor_id",
                table: "role_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_sftp_connection_audit_logs_AspNetUsers_actor_id",
                table: "sftp_connection_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_sftp_connection_audit_logs_sftp_connections_sftp_connection~",
                table: "sftp_connection_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_sftp_connections_organizations_organization_id",
                table: "sftp_connections");

            migrationBuilder.DropForeignKey(
                name: "FK_SFTPConnectionAuthCodes_AspNetUsers_UserId",
                table: "SFTPConnectionAuthCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_SFTPConnectionAuthCodes_organizations_OrganizationId",
                table: "SFTPConnectionAuthCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_user_organization_audit_logs_AspNetUsers_actor_id",
                table: "user_organization_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_user_organization_audit_logs_users_organizations_user_organ~",
                table: "user_organization_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_AspNetRoles_RoleId",
                table: "users_organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_AspNetUsers_users_id",
                table: "users_organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_organizations_organization_id",
                table: "users_organizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users_organizations",
                table: "users_organizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_organization_audit_logs",
                table: "user_organization_audit_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sftp_connections",
                table: "sftp_connections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sftp_connection_audit_logs",
                table: "sftp_connection_audit_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_role_audit_logs",
                table: "role_audit_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_profile",
                table: "profile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_permissions",
                table: "permissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_organizations",
                table: "organizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_organization_audit_logs",
                table: "organization_audit_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SFTPConnectionAuthCodes",
                table: "SFTPConnectionAuthCodes");

            migrationBuilder.RenameTable(
                name: "SFTPConnectionAuthCodes",
                newName: "sftp_connection_auth_codes");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "users_organizations",
                newName: "role_id");

            migrationBuilder.RenameIndex(
                name: "IX_users_organizations_organization_id",
                table: "users_organizations",
                newName: "ix_users_organizations_organization_id");

            migrationBuilder.RenameIndex(
                name: "IX_users_organizations_users_id",
                table: "users_organizations",
                newName: "ix_users_organizations_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_users_organizations_RoleId",
                table: "users_organizations",
                newName: "ix_users_organizations_role_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_organization_audit_logs_user_organization_id",
                table: "user_organization_audit_logs",
                newName: "ix_user_organization_audit_logs_user_organization_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_organization_audit_logs_actor_id",
                table: "user_organization_audit_logs",
                newName: "ix_user_organization_audit_logs_actor_id");

            migrationBuilder.RenameIndex(
                name: "IX_sftp_connections_organization_id",
                table: "sftp_connections",
                newName: "ix_sftp_connections_organization_id");

            migrationBuilder.RenameIndex(
                name: "IX_sftp_connection_audit_logs_sftp_connection_id",
                table: "sftp_connection_audit_logs",
                newName: "ix_sftp_connection_audit_logs_sftp_connection_id");

            migrationBuilder.RenameIndex(
                name: "IX_sftp_connection_audit_logs_actor_id",
                table: "sftp_connection_audit_logs",
                newName: "ix_sftp_connection_audit_logs_actor_id");

            migrationBuilder.RenameIndex(
                name: "IX_role_audit_logs_role_id",
                table: "role_audit_logs",
                newName: "ix_role_audit_logs_role_id");

            migrationBuilder.RenameIndex(
                name: "IX_role_audit_logs_actor_id",
                table: "role_audit_logs",
                newName: "ix_role_audit_logs_actor_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "refresh_tokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "refresh_tokens",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "TokenHash",
                table: "refresh_tokens",
                newName: "token_hash");

            migrationBuilder.RenameColumn(
                name: "RevokedAt",
                table: "refresh_tokens",
                newName: "revoked_at");

            migrationBuilder.RenameColumn(
                name: "ReplacedByTokenHash",
                table: "refresh_tokens",
                newName: "replaced_by_token_hash");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "refresh_tokens",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "refresh_tokens",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_TokenHash",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_token_hash");

            migrationBuilder.RenameColumn(
                name: "WorkspaceURL",
                table: "organizations",
                newName: "workspace_url");

            migrationBuilder.RenameColumn(
                name: "UserOwnerId",
                table: "organizations",
                newName: "user_owner_id");

            migrationBuilder.RenameColumn(
                name: "ContactEmail",
                table: "organizations",
                newName: "contact_email");

            migrationBuilder.RenameIndex(
                name: "IX_organizations_UserOwnerId",
                table: "organizations",
                newName: "ix_organizations_user_owner_id");

            migrationBuilder.RenameIndex(
                name: "IX_organization_audit_logs_organization_id",
                table: "organization_audit_logs",
                newName: "ix_organization_audit_logs_organization_id");

            migrationBuilder.RenameIndex(
                name: "IX_organization_audit_logs_actor_id",
                table: "organization_audit_logs",
                newName: "ix_organization_audit_logs_actor_id");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "AspNetUserTokens",
                newName: "value");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetUserTokens",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                newName: "login_provider");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AspNetUserTokens",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "AspNetUsers",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AspNetUsers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "AspNetUsers",
                newName: "user_name");

            migrationBuilder.RenameColumn(
                name: "TwoFactorEnabled",
                table: "AspNetUsers",
                newName: "two_factor_enabled");

            migrationBuilder.RenameColumn(
                name: "SecurityStamp",
                table: "AspNetUsers",
                newName: "security_stamp");

            migrationBuilder.RenameColumn(
                name: "PhoneNumberConfirmed",
                table: "AspNetUsers",
                newName: "phone_number_confirmed");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "AspNetUsers",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "AspNetUsers",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "NormalizedUserName",
                table: "AspNetUsers",
                newName: "normalized_user_name");

            migrationBuilder.RenameColumn(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                newName: "normalized_email");

            migrationBuilder.RenameColumn(
                name: "LockoutEnd",
                table: "AspNetUsers",
                newName: "lockout_end");

            migrationBuilder.RenameColumn(
                name: "LockoutEnabled",
                table: "AspNetUsers",
                newName: "lockout_enabled");

            migrationBuilder.RenameColumn(
                name: "EmailConfirmed",
                table: "AspNetUsers",
                newName: "email_confirmed");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AspNetUsers",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "AspNetUsers",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "AccessFailedCount",
                table: "AspNetUsers",
                newName: "access_failed_count");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "AspNetUserRoles",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AspNetUserRoles",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                newName: "ix_asp_net_user_roles_role_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AspNetUserLogins",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ProviderDisplayName",
                table: "AspNetUserLogins",
                newName: "provider_display_name");

            migrationBuilder.RenameColumn(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                newName: "provider_key");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                newName: "login_provider");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                newName: "ix_asp_net_user_logins_user_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AspNetUserClaims",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AspNetUserClaims",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "AspNetUserClaims",
                newName: "claim_value");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "AspNetUserClaims",
                newName: "claim_type");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                newName: "ix_asp_net_user_claims_user_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetRoles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AspNetRoles",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PermissionMask",
                table: "AspNetRoles",
                newName: "permission_mask");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "AspNetRoles",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "NormalizedName",
                table: "AspNetRoles",
                newName: "normalized_name");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "AspNetRoles",
                newName: "concurrency_stamp");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoles_OrganizationId",
                table: "AspNetRoles",
                newName: "ix_asp_net_roles_organization_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AspNetRoleClaims",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "AspNetRoleClaims",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "AspNetRoleClaims",
                newName: "claim_value");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "AspNetRoleClaims",
                newName: "claim_type");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                newName: "ix_asp_net_role_claims_role_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "sftp_connection_auth_codes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "VerificationCode",
                table: "sftp_connection_auth_codes",
                newName: "verification_code");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "sftp_connection_auth_codes",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "sftp_connection_auth_codes",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "sftp_connection_auth_codes",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "sftp_connection_auth_codes",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_SFTPConnectionAuthCodes_UserId",
                table: "sftp_connection_auth_codes",
                newName: "ix_sftp_connection_auth_codes_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_SFTPConnectionAuthCodes_OrganizationId",
                table: "sftp_connection_auth_codes",
                newName: "ix_sftp_connection_auth_codes_organization_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users_organizations",
                table: "users_organizations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_organization_audit_logs",
                table: "user_organization_audit_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sftp_connections",
                table: "sftp_connections",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sftp_connection_audit_logs",
                table: "sftp_connection_audit_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_role_audit_logs",
                table: "role_audit_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_refresh_tokens",
                table: "refresh_tokens",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_profile",
                table: "profile",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_permissions",
                table: "permissions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_organizations",
                table: "organizations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_organization_audit_logs",
                table: "organization_audit_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_tokens",
                table: "AspNetUserTokens",
                columns: new[] { "user_id", "login_provider", "name" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_users",
                table: "AspNetUsers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_roles",
                table: "AspNetUserRoles",
                columns: new[] { "user_id", "role_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_logins",
                table: "AspNetUserLogins",
                columns: new[] { "login_provider", "provider_key" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_user_claims",
                table: "AspNetUserClaims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_roles",
                table: "AspNetRoles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asp_net_role_claims",
                table: "AspNetRoleClaims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sftp_connection_auth_codes",
                table: "sftp_connection_auth_codes",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                table: "AspNetRoleClaims",
                column: "role_id",
                principalTable: "AspNetRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_roles_organizations_organization_id",
                table: "AspNetRoles",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_claims_asp_net_users_user_id",
                table: "AspNetUserClaims",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_logins_asp_net_users_user_id",
                table: "AspNetUserLogins",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                table: "AspNetUserRoles",
                column: "role_id",
                principalTable: "AspNetRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_roles_asp_net_users_user_id",
                table: "AspNetUserRoles",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                table: "AspNetUserTokens",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_organization_audit_logs_organizations_organization_id",
                table: "organization_audit_logs",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_organization_audit_logs_users_actor_id",
                table: "organization_audit_logs",
                column: "actor_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_organizations_users_user_owner_id",
                table: "organizations",
                column: "user_owner_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_refresh_tokens_users_user_id",
                table: "refresh_tokens",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_role_audit_logs_asp_net_role_role_id",
                table: "role_audit_logs",
                column: "role_id",
                principalTable: "AspNetRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_role_audit_logs_users_actor_id",
                table: "role_audit_logs",
                column: "actor_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sftp_connection_audit_logs_sftp_connections_sftp_connection",
                table: "sftp_connection_audit_logs",
                column: "sftp_connection_id",
                principalTable: "sftp_connections",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sftp_connection_audit_logs_users_actor_id",
                table: "sftp_connection_audit_logs",
                column: "actor_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sftp_connection_auth_codes_organizations_organization_id",
                table: "sftp_connection_auth_codes",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sftp_connection_auth_codes_users_user_id",
                table: "sftp_connection_auth_codes",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_sftp_connections_organizations_organization_id",
                table: "sftp_connections",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_organization_audit_logs_user_organizations_user_organi",
                table: "user_organization_audit_logs",
                column: "user_organization_id",
                principalTable: "users_organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_organization_audit_logs_users_actor_id",
                table: "user_organization_audit_logs",
                column: "actor_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_users_organizations_asp_net_roles_role_id",
                table: "users_organizations",
                column: "role_id",
                principalTable: "AspNetRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_users_organizations_asp_net_users_user_id",
                table: "users_organizations",
                column: "users_id",
                principalTable: "AspNetUsers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_users_organizations_organizations_organization_id",
                table: "users_organizations",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_roles_organizations_organization_id",
                table: "AspNetRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_claims_asp_net_users_user_id",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_logins_asp_net_users_user_id",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_roles_asp_net_users_user_id",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "fk_organization_audit_logs_organizations_organization_id",
                table: "organization_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_organization_audit_logs_users_actor_id",
                table: "organization_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_organizations_users_user_owner_id",
                table: "organizations");

            migrationBuilder.DropForeignKey(
                name: "fk_refresh_tokens_users_user_id",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "fk_role_audit_logs_asp_net_role_role_id",
                table: "role_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_role_audit_logs_users_actor_id",
                table: "role_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_sftp_connection_audit_logs_sftp_connections_sftp_connection",
                table: "sftp_connection_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_sftp_connection_audit_logs_users_actor_id",
                table: "sftp_connection_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_sftp_connection_auth_codes_organizations_organization_id",
                table: "sftp_connection_auth_codes");

            migrationBuilder.DropForeignKey(
                name: "fk_sftp_connection_auth_codes_users_user_id",
                table: "sftp_connection_auth_codes");

            migrationBuilder.DropForeignKey(
                name: "fk_sftp_connections_organizations_organization_id",
                table: "sftp_connections");

            migrationBuilder.DropForeignKey(
                name: "fk_user_organization_audit_logs_user_organizations_user_organi",
                table: "user_organization_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_user_organization_audit_logs_users_actor_id",
                table: "user_organization_audit_logs");

            migrationBuilder.DropForeignKey(
                name: "fk_users_organizations_asp_net_roles_role_id",
                table: "users_organizations");

            migrationBuilder.DropForeignKey(
                name: "fk_users_organizations_asp_net_users_user_id",
                table: "users_organizations");

            migrationBuilder.DropForeignKey(
                name: "fk_users_organizations_organizations_organization_id",
                table: "users_organizations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users_organizations",
                table: "users_organizations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_organization_audit_logs",
                table: "user_organization_audit_logs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sftp_connections",
                table: "sftp_connections");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sftp_connection_audit_logs",
                table: "sftp_connection_audit_logs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_role_audit_logs",
                table: "role_audit_logs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_profile",
                table: "profile");

            migrationBuilder.DropPrimaryKey(
                name: "pk_permissions",
                table: "permissions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_organizations",
                table: "organizations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_organization_audit_logs",
                table: "organization_audit_logs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_tokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_users",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_roles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_logins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_user_claims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_roles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_asp_net_role_claims",
                table: "AspNetRoleClaims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sftp_connection_auth_codes",
                table: "sftp_connection_auth_codes");

            migrationBuilder.RenameTable(
                name: "sftp_connection_auth_codes",
                newName: "SFTPConnectionAuthCodes");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "users_organizations",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "ix_users_organizations_organization_id",
                table: "users_organizations",
                newName: "IX_users_organizations_organization_id");

            migrationBuilder.RenameIndex(
                name: "ix_users_organizations_user_id",
                table: "users_organizations",
                newName: "IX_users_organizations_users_id");

            migrationBuilder.RenameIndex(
                name: "ix_users_organizations_role_id",
                table: "users_organizations",
                newName: "IX_users_organizations_RoleId");

            migrationBuilder.RenameIndex(
                name: "ix_user_organization_audit_logs_user_organization_id",
                table: "user_organization_audit_logs",
                newName: "IX_user_organization_audit_logs_user_organization_id");

            migrationBuilder.RenameIndex(
                name: "ix_user_organization_audit_logs_actor_id",
                table: "user_organization_audit_logs",
                newName: "IX_user_organization_audit_logs_actor_id");

            migrationBuilder.RenameIndex(
                name: "ix_sftp_connections_organization_id",
                table: "sftp_connections",
                newName: "IX_sftp_connections_organization_id");

            migrationBuilder.RenameIndex(
                name: "ix_sftp_connection_audit_logs_sftp_connection_id",
                table: "sftp_connection_audit_logs",
                newName: "IX_sftp_connection_audit_logs_sftp_connection_id");

            migrationBuilder.RenameIndex(
                name: "ix_sftp_connection_audit_logs_actor_id",
                table: "sftp_connection_audit_logs",
                newName: "IX_sftp_connection_audit_logs_actor_id");

            migrationBuilder.RenameIndex(
                name: "ix_role_audit_logs_role_id",
                table: "role_audit_logs",
                newName: "IX_role_audit_logs_role_id");

            migrationBuilder.RenameIndex(
                name: "ix_role_audit_logs_actor_id",
                table: "role_audit_logs",
                newName: "IX_role_audit_logs_actor_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "refresh_tokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "refresh_tokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "token_hash",
                table: "refresh_tokens",
                newName: "TokenHash");

            migrationBuilder.RenameColumn(
                name: "revoked_at",
                table: "refresh_tokens",
                newName: "RevokedAt");

            migrationBuilder.RenameColumn(
                name: "replaced_by_token_hash",
                table: "refresh_tokens",
                newName: "ReplacedByTokenHash");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "refresh_tokens",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "refresh_tokens",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_TokenHash");

            migrationBuilder.RenameColumn(
                name: "workspace_url",
                table: "organizations",
                newName: "WorkspaceURL");

            migrationBuilder.RenameColumn(
                name: "user_owner_id",
                table: "organizations",
                newName: "UserOwnerId");

            migrationBuilder.RenameColumn(
                name: "contact_email",
                table: "organizations",
                newName: "ContactEmail");

            migrationBuilder.RenameIndex(
                name: "ix_organizations_user_owner_id",
                table: "organizations",
                newName: "IX_organizations_UserOwnerId");

            migrationBuilder.RenameIndex(
                name: "ix_organization_audit_logs_organization_id",
                table: "organization_audit_logs",
                newName: "IX_organization_audit_logs_organization_id");

            migrationBuilder.RenameIndex(
                name: "ix_organization_audit_logs_actor_id",
                table: "organization_audit_logs",
                newName: "IX_organization_audit_logs_actor_id");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "AspNetUserTokens",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "AspNetUserTokens",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "login_provider",
                table: "AspNetUserTokens",
                newName: "LoginProvider");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AspNetUserTokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "AspNetUsers",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetUsers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_name",
                table: "AspNetUsers",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "two_factor_enabled",
                table: "AspNetUsers",
                newName: "TwoFactorEnabled");

            migrationBuilder.RenameColumn(
                name: "security_stamp",
                table: "AspNetUsers",
                newName: "SecurityStamp");

            migrationBuilder.RenameColumn(
                name: "phone_number_confirmed",
                table: "AspNetUsers",
                newName: "PhoneNumberConfirmed");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "AspNetUsers",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "AspNetUsers",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "normalized_user_name",
                table: "AspNetUsers",
                newName: "NormalizedUserName");

            migrationBuilder.RenameColumn(
                name: "normalized_email",
                table: "AspNetUsers",
                newName: "NormalizedEmail");

            migrationBuilder.RenameColumn(
                name: "lockout_end",
                table: "AspNetUsers",
                newName: "LockoutEnd");

            migrationBuilder.RenameColumn(
                name: "lockout_enabled",
                table: "AspNetUsers",
                newName: "LockoutEnabled");

            migrationBuilder.RenameColumn(
                name: "email_confirmed",
                table: "AspNetUsers",
                newName: "EmailConfirmed");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "AspNetUsers",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "AspNetUsers",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "access_failed_count",
                table: "AspNetUsers",
                newName: "AccessFailedCount");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "AspNetUserRoles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AspNetUserRoles",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AspNetUserLogins",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "provider_display_name",
                table: "AspNetUserLogins",
                newName: "ProviderDisplayName");

            migrationBuilder.RenameColumn(
                name: "provider_key",
                table: "AspNetUserLogins",
                newName: "ProviderKey");

            migrationBuilder.RenameColumn(
                name: "login_provider",
                table: "AspNetUserLogins",
                newName: "LoginProvider");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "AspNetUserLogins",
                newName: "IX_AspNetUserLogins_UserId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetUserClaims",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AspNetUserClaims",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "claim_value",
                table: "AspNetUserClaims",
                newName: "ClaimValue");

            migrationBuilder.RenameColumn(
                name: "claim_type",
                table: "AspNetUserClaims",
                newName: "ClaimType");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "AspNetUserClaims",
                newName: "IX_AspNetUserClaims_UserId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "AspNetRoles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetRoles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "permission_mask",
                table: "AspNetRoles",
                newName: "PermissionMask");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "AspNetRoles",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "normalized_name",
                table: "AspNetRoles",
                newName: "NormalizedName");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "AspNetRoles",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_roles_organization_id",
                table: "AspNetRoles",
                newName: "IX_AspNetRoles_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetRoleClaims",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "AspNetRoleClaims",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "claim_value",
                table: "AspNetRoleClaims",
                newName: "ClaimValue");

            migrationBuilder.RenameColumn(
                name: "claim_type",
                table: "AspNetRoleClaims",
                newName: "ClaimType");

            migrationBuilder.RenameIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "AspNetRoleClaims",
                newName: "IX_AspNetRoleClaims_RoleId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SFTPConnectionAuthCodes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "verification_code",
                table: "SFTPConnectionAuthCodes",
                newName: "VerificationCode");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "SFTPConnectionAuthCodes",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "SFTPConnectionAuthCodes",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "SFTPConnectionAuthCodes",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "SFTPConnectionAuthCodes",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_sftp_connection_auth_codes_user_id",
                table: "SFTPConnectionAuthCodes",
                newName: "IX_SFTPConnectionAuthCodes_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_sftp_connection_auth_codes_organization_id",
                table: "SFTPConnectionAuthCodes",
                newName: "IX_SFTPConnectionAuthCodes_OrganizationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users_organizations",
                table: "users_organizations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_organization_audit_logs",
                table: "user_organization_audit_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sftp_connections",
                table: "sftp_connections",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sftp_connection_audit_logs",
                table: "sftp_connection_audit_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_role_audit_logs",
                table: "role_audit_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_profile",
                table: "profile",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_permissions",
                table: "permissions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_organizations",
                table: "organizations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_organization_audit_logs",
                table: "organization_audit_logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SFTPConnectionAuthCodes",
                table: "SFTPConnectionAuthCodes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_organizations_OrganizationId",
                table: "AspNetRoles",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_organization_audit_logs_AspNetUsers_actor_id",
                table: "organization_audit_logs",
                column: "actor_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_organization_audit_logs_organizations_organization_id",
                table: "organization_audit_logs",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_organizations_AspNetUsers_UserOwnerId",
                table: "organizations",
                column: "UserOwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_AspNetUsers_UserId",
                table: "refresh_tokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_role_audit_logs_AspNetRoles_role_id",
                table: "role_audit_logs",
                column: "role_id",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_role_audit_logs_AspNetUsers_actor_id",
                table: "role_audit_logs",
                column: "actor_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sftp_connection_audit_logs_AspNetUsers_actor_id",
                table: "sftp_connection_audit_logs",
                column: "actor_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sftp_connection_audit_logs_sftp_connections_sftp_connection~",
                table: "sftp_connection_audit_logs",
                column: "sftp_connection_id",
                principalTable: "sftp_connections",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sftp_connections_organizations_organization_id",
                table: "sftp_connections",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SFTPConnectionAuthCodes_AspNetUsers_UserId",
                table: "SFTPConnectionAuthCodes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SFTPConnectionAuthCodes_organizations_OrganizationId",
                table: "SFTPConnectionAuthCodes",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_organization_audit_logs_AspNetUsers_actor_id",
                table: "user_organization_audit_logs",
                column: "actor_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_organization_audit_logs_users_organizations_user_organ~",
                table: "user_organization_audit_logs",
                column: "user_organization_id",
                principalTable: "users_organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_AspNetRoles_RoleId",
                table: "users_organizations",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_AspNetUsers_users_id",
                table: "users_organizations",
                column: "users_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_organizations_organization_id",
                table: "users_organizations",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id");
        }
    }
}
