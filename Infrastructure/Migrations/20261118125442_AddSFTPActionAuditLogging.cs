using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSFTPActionAuditLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sftp_action_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sftp_connection_id = table.Column<int>(type: "integer", nullable: false),
                    actor_id = table.Column<string>(type: "text", nullable: false),
                    action = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false),
                    old_data = table.Column<string>(type: "text", nullable: true),
                    new_data = table.Column<string>(type: "text", nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sftp_action_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_sftp_action_audit_logs_sftp_connections_sftp_connection_id",
                        column: x => x.sftp_connection_id,
                        principalTable: "sftp_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sftp_action_audit_logs_users_actor_id",
                        column: x => x.actor_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sftp_action_audit_logs_actor_id",
                table: "sftp_action_audit_logs",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_sftp_action_audit_logs_sftp_connection_id",
                table: "sftp_action_audit_logs",
                column: "sftp_connection_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sftp_action_audit_logs");
        }
    }
}
