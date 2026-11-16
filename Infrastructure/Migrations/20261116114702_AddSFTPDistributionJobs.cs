using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSFTPDistributionJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sftp_distribution_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ready_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    total_files = table.Column<int>(type: "integer", nullable: false),
                    total_bytes = table.Column<long>(type: "bigint", nullable: false),
                    files_json = table.Column<string>(type: "text", nullable: false),
                    destinations_json = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sftp_distribution_jobs", x => x.id);
                    table.ForeignKey(
                        name: "fk_sftp_distribution_jobs_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sftp_distribution_jobs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sftp_distribution_jobs_organization_id",
                table: "sftp_distribution_jobs",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_sftp_distribution_jobs_user_id",
                table: "sftp_distribution_jobs",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sftp_distribution_jobs");
        }
    }
}
