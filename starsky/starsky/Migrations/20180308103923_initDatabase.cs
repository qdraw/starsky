﻿using Microsoft.EntityFrameworkCore.Migrations;
using starsky.Attributes;
using starskycore.Attributes;

namespace starsky.Migrations
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
                    FileName = table.Column<string>(nullable: true),
                    FilePath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileIndex", x => x.Id);
                });
        }
        
        [ExcludeFromCoverage]
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileIndex");
        }
    }
}
