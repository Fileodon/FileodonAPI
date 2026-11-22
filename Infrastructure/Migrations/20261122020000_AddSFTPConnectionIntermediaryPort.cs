using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

public partial class AddSFTPConnectionIntermediaryPort : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "intermediary_port",
            table: "sftp_connections",
            type: "integer",
            nullable: false,
            defaultValue: 5096);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "intermediary_port",
            table: "sftp_connections");
    }
}
