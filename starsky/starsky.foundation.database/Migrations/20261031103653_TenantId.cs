using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class TenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Thumbnails",
                table: "Thumbnails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Settings",
                table: "Settings");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Thumbnails",
                type: "TEXT",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Settings",
                type: "TEXT",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "QueueItems",
                type: "TEXT",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Notifications",
                type: "TEXT",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "ImportIndex",
                type: "TEXT",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "FileIndex",
                type: "TEXT",
                maxLength: 63,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Thumbnails",
                table: "Thumbnails",
                columns: new[] { "TenantId", "FileHash" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Settings",
                table: "Settings",
                columns: new[] { "TenantId", "Key" });

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_TenantId_FilePath",
                table: "FileIndex",
                columns: new[] { "TenantId", "FilePath" });

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_TenantId_ParentDirectory_FileName",
                table: "FileIndex",
                columns: new[] { "TenantId", "ParentDirectory", "FileName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Thumbnails",
                table: "Thumbnails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Settings",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_FileIndex_TenantId_FilePath",
                table: "FileIndex");

            migrationBuilder.DropIndex(
                name: "IX_FileIndex_TenantId_ParentDirectory_FileName",
                table: "FileIndex");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Thumbnails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "QueueItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ImportIndex");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FileIndex");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Thumbnails",
                table: "Thumbnails",
                column: "FileHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Settings",
                table: "Settings",
                column: "Key");
        }
    }
}
