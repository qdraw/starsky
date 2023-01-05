using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    public partial class epochNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DateTimeEpoch",
                table: "Notifications",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTimeEpoch",
                table: "Notifications");
        }
    }
}
