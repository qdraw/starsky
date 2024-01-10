using System;
using Microsoft.EntityFrameworkCore.Migrations;
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.

namespace starsky.foundation.database.Migrations
{
    public partial class importdatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportIndex",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    AddToDatabase = table.Column<DateTime>(nullable: false),
                    FileHash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportIndex", x => x.Id);
                });
        }
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportIndex");
        }
    }
}
