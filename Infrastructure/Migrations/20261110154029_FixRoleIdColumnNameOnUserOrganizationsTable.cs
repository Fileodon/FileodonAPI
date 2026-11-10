using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRoleIdColumnNameOnUserOrganizationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_AspNetRoles_RoleId",
                table: "users_organizations");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "users_organizations",
                newName: "role_id");

            migrationBuilder.RenameIndex(
                name: "IX_users_organizations_RoleId",
                table: "users_organizations",
                newName: "IX_users_organizations_role_id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_AspNetRoles_role_id",
                table: "users_organizations",
                column: "role_id",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_AspNetRoles_role_id",
                table: "users_organizations");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "users_organizations",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_users_organizations_role_id",
                table: "users_organizations",
                newName: "IX_users_organizations_RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_AspNetRoles_RoleId",
                table: "users_organizations",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
