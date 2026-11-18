using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteRolesOnDeletedOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_organizations_OrganizationId",
                table: "AspNetRoles");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_organizations_OrganizationId",
                table: "AspNetRoles",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_organizations_OrganizationId",
                table: "AspNetRoles");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_organizations_OrganizationId",
                table: "AspNetRoles",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
