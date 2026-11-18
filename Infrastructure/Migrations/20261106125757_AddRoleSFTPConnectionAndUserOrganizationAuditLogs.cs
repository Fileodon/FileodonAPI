using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleSFTPConnectionAndUserOrganizationAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "role_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    actor_id = table.Column<string>(type: "text", nullable: false),
                    action = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false),
                    old_data = table.Column<string>(type: "jsonb", nullable: true),
                    new_data = table.Column<string>(type: "jsonb", nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_audit_logs_AspNetRoles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_audit_logs_AspNetUsers_actor_id",
                        column: x => x.actor_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sftp_connection_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sftp_connection_id = table.Column<int>(type: "integer", nullable: false),
                    actor_id = table.Column<string>(type: "text", nullable: false),
                    action = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false),
                    old_data = table.Column<string>(type: "jsonb", nullable: true),
                    new_data = table.Column<string>(type: "jsonb", nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sftp_connection_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_sftp_connection_audit_logs_AspNetUsers_actor_id",
                        column: x => x.actor_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sftp_connection_audit_logs_sftp_connections_sftp_connection~",
                        column: x => x.sftp_connection_id,
                        principalTable: "sftp_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_organization_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_organization_id = table.Column<int>(type: "integer", nullable: false),
                    actor_id = table.Column<string>(type: "text", nullable: false),
                    action = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false),
                    old_data = table.Column<string>(type: "jsonb", nullable: true),
                    new_data = table.Column<string>(type: "jsonb", nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_organization_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_organization_audit_logs_AspNetUsers_actor_id",
                        column: x => x.actor_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_organization_audit_logs_users_organizations_user_organ~",
                        column: x => x.user_organization_id,
                        principalTable: "users_organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_role_audit_logs_actor_id",
                table: "role_audit_logs",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_audit_logs_role_id",
                table: "role_audit_logs",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_sftp_connection_audit_logs_actor_id",
                table: "sftp_connection_audit_logs",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "IX_sftp_connection_audit_logs_sftp_connection_id",
                table: "sftp_connection_audit_logs",
                column: "sftp_connection_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_organization_audit_logs_actor_id",
                table: "user_organization_audit_logs",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_organization_audit_logs_user_organization_id",
                table: "user_organization_audit_logs",
                column: "user_organization_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_audit_logs");

            migrationBuilder.DropTable(
                name: "sftp_connection_audit_logs");

            migrationBuilder.DropTable(
                name: "user_organization_audit_logs");
        }
    }
}
