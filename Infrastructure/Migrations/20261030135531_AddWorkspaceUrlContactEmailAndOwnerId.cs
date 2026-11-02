using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceUrlContactEmailAndOwnerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "organizations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserOwnerId",
                table: "organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkspaceURL",
                table: "organizations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE organizations AS organization
                SET "UserOwnerId" = COALESCE(
                    (
                        SELECT membership.users_id
                        FROM users_organizations AS membership
                        INNER JOIN "AspNetRoles" AS role
                            ON role."Id" = membership."RoleId"
                        WHERE membership.organization_id = organization.id
                          AND role."Name" = 'Owner'
                        ORDER BY membership.id
                        LIMIT 1
                    ),
                    (
                        SELECT membership.users_id
                        FROM users_organizations AS membership
                        INNER JOIN "AspNetRoles" AS role
                            ON role."Id" = membership."RoleId"
                        WHERE membership.organization_id = organization.id
                          AND role."Name" = 'Admin'
                        ORDER BY membership.id
                        LIMIT 1
                    )
                )
                WHERE "UserOwnerId" IS NULL;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM organizations
                        WHERE "UserOwnerId" IS NULL
                    ) THEN
                        RAISE EXCEPTION
                            'Cannot assign owners to all existing organizations. Populate organizations.UserOwnerId before applying this migration.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "UserOwnerId",
                table: "organizations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_organizations_UserOwnerId",
                table: "organizations",
                column: "UserOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_organizations_AspNetUsers_UserOwnerId",
                table: "organizations",
                column: "UserOwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_organizations_AspNetUsers_UserOwnerId",
                table: "organizations");

            migrationBuilder.DropIndex(
                name: "IX_organizations_UserOwnerId",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "UserOwnerId",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "WorkspaceURL",
                table: "organizations");
        }
    }
}
