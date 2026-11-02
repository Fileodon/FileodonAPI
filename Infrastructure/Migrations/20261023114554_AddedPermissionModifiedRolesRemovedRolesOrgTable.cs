using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedPermissionModifiedRolesRemovedRolesOrgTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "roles_organizations");

            migrationBuilder.AddColumn<string>(
                name: "RoleId",
                table: "users_organizations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetRoles",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "AspNetRoles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PermissionMask",
                table: "AspNetRoles",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_organizations_RoleId",
                table: "users_organizations",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_OrganizationId",
                table: "AspNetRoles",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_organizations_OrganizationId",
                table: "AspNetRoles",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_AspNetRoles_RoleId",
                table: "users_organizations",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_organizations_OrganizationId",
                table: "AspNetRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_AspNetRoles_RoleId",
                table: "users_organizations");

            migrationBuilder.DropIndex(
                name: "IX_users_organizations_RoleId",
                table: "users_organizations");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_OrganizationId",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "users_organizations");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "PermissionMask",
                table: "AspNetRoles");

            migrationBuilder.CreateTable(
                name: "roles_organizations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organization_id = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles_organizations", x => x.id);
                    table.ForeignKey(
                        name: "FK_roles_organizations_AspNetRoles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_roles_organizations_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_roles_organizations_organization_id",
                table: "roles_organizations",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_organizations_role_id",
                table: "roles_organizations",
                column: "role_id");
        }
    }
}
