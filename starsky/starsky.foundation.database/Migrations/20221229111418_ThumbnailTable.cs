using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    public partial class ThumbnailTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Thumbnails",
                columns: table => new
                {
                    FileHash = table.Column<string>(type: "varchar(190)", maxLength: 190, nullable: false),
                    TinyMeta = table.Column<bool>(type: "INTEGER", nullable: true),
                    Small = table.Column<bool>(type: "INTEGER", nullable: true),
                    Large = table.Column<bool>(type: "INTEGER", nullable: true),
                    ExtraLarge = table.Column<bool>(type: "INTEGER", nullable: true),
                    Reasons = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Thumbnails", x => x.FileHash);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Thumbnails");
        }
    }
}
