using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedUserOrganizationDeleteBehaviourToCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_AspNetUsers_users_id",
                table: "users_organizations");

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_AspNetUsers_users_id",
                table: "users_organizations",
                column: "users_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_AspNetUsers_users_id",
                table: "users_organizations");

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_AspNetUsers_users_id",
                table: "users_organizations",
                column: "users_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
