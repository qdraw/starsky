using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Diagnostics.CodeAnalysis;
using starsky.Attributes;

namespace starsky.Migrations
{
    public partial class importdatabase : Migration
    {
        [ExcludeFromCoverage]
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportIndex",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AddToDatabase = table.Column<DateTime>(nullable: false),
                    FileHash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportIndex", x => x.Id);
                });
        }
        
        [ExcludeFromCoverage]
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportIndex");
        }
    }
}
