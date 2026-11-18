using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSFTPDistributionProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "error",
                table: "sftp_distribution_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "progress_json",
                table: "sftp_distribution_jobs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "error",
                table: "sftp_distribution_jobs");

            migrationBuilder.DropColumn(
                name: "progress_json",
                table: "sftp_distribution_jobs");
        }
    }
}
