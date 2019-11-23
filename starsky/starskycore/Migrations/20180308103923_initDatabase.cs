using Microsoft.EntityFrameworkCore.Migrations;
using starskycore.Attributes;

namespace starskycore.Migrations
{
    public partial class initDatabase : Migration
    {
        [ExcludeFromCoverage]
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileIndex",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(maxLength: 510, nullable: true),
                    FilePath = table.Column<string>(maxLength: 510, nullable: true)
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
