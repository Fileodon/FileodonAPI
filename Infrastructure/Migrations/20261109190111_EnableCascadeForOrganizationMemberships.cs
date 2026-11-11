using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnableCascadeForOrganizationMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_organizations_organization_id",
                table: "users_organizations");

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_organizations_organization_id",
                table: "users_organizations",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_organizations_organization_id",
                table: "users_organizations");

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_organizations_organization_id",
                table: "users_organizations",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id");
        }
    }
}
