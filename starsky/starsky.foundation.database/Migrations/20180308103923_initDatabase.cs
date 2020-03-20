
using Microsoft.EntityFrameworkCore.Migrations;

namespace starsky.foundation.database.Migrations
{
    public partial class initDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileIndex",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    FileName = table.Column<string>(maxLength: 190, nullable: true),
                    FilePath = table.Column<string>(maxLength: 380, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileIndex", x => x.Id);
                });
        }
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileIndex");
        }
    }
}
