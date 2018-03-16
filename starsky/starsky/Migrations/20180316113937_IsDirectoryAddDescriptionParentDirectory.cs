using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace starsky.Migrations
{
    public partial class IsDirectoryAddDescriptionParentDirectory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "Folder",
            //    table: "FileIndex");

            migrationBuilder.AddColumn<string>(
                name: "ParentDirectory",
                table: "FileIndex",
                nullable: true);

            // SQLite does not support this migration operation ('RenameColumnOperation'). For more information, see http://go.microsoft.com/fwlink/?LinkId=723262.

            //migrationBuilder.RenameColumn(
            //    name: "Folder",
            //    table: "FileIndex",
            //    newName: "ParentDirectory");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "FileIndex",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDirectory",
                table: "FileIndex",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "IsDirectory",
                table: "FileIndex");

            // SQLite does not support this migration operation ('RenameColumnOperation'). For more information, see http://go.microsoft.com/fwlink/?LinkId=723262.

            migrationBuilder.DropColumn(
                name: "ParentDirectory",
                table: "FileIndex");

            //migrationBuilder.AddColumn<string>(
            //    name: "Folder",
            //    table: "FileIndex",
            //    nullable: true);

            //migrationBuilder.RenameColumn(
            //    name: "ParentDirectory",
            //    table: "FileIndex",
            //    newName: "Folder");
        }
    }
}
