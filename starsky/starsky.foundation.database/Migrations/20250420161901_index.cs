using Microsoft.EntityFrameworkCore.Migrations;
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DateTimeEpoch",
                table: "Notifications",
                column: "DateTimeEpoch");

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_FilePath",
                table: "FileIndex",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_FileIndex_ParentDirectory_FileName",
                table: "FileIndex",
                columns: new[] { "ParentDirectory", "FileName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_DateTimeEpoch",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_FileIndex_FilePath",
                table: "FileIndex");

            migrationBuilder.DropIndex(
                name: "IX_FileIndex_ParentDirectory_FileName",
                table: "FileIndex");
        }
    }
}
