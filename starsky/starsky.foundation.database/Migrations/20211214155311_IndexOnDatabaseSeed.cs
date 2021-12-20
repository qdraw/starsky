using Microsoft.EntityFrameworkCore.Migrations;

namespace starsky.foundation.database.Migrations
{
    public partial class IndexOnDatabaseSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ImportIndex_FileHash",
                table: "ImportIndex",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_Id_Identifier",
                table: "Credentials",
                columns: new[] { "Id", "Identifier" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImportIndex_FileHash",
                table: "ImportIndex");

            migrationBuilder.DropIndex(
                name: "IX_Credentials_Id_Identifier",
                table: "Credentials");
        }
    }
}
