using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class AddConnectIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConnectBlockInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 190, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 380, nullable: false),
                    Offset = table.Column<long>(type: "INTEGER", nullable: false),
                    Size = table.Column<int>(type: "INTEGER", nullable: false),
                    Hash = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectBlockInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConnectDeviceIndexes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 190, nullable: false),
                    DeviceId = table.Column<byte[]>(type: "BLOB", maxLength: 32, nullable: false),
                    MaxSequence = table.Column<long>(type: "INTEGER", nullable: false),
                    IndexId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectDeviceIndexes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConnectFileMetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 190, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 380, nullable: false),
                    Version = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Sequence = table.Column<long>(type: "INTEGER", nullable: false),
                    BlockSize = table.Column<int>(type: "INTEGER", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Invalid = table.Column<bool>(type: "INTEGER", nullable: false),
                    NoPermissions = table.Column<bool>(type: "INTEGER", nullable: false),
                    ModifiedBy = table.Column<long>(type: "INTEGER", nullable: false),
                    SymlinkTarget = table.Column<string>(type: "TEXT", maxLength: 380, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectFileMetas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConnectFolderMetas",
                columns: table => new
                {
                    Folder = table.Column<string>(type: "TEXT", maxLength: 190, nullable: false),
                    IndexId = table.Column<long>(type: "INTEGER", nullable: false),
                    Sequence = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectFolderMetas", x => x.Folder);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectBlockInfos_Folder_Name_Offset",
                table: "ConnectBlockInfos",
                columns: new[] { "Folder", "Name", "Offset" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConnectDeviceIndexes_Folder_DeviceId",
                table: "ConnectDeviceIndexes",
                columns: new[] { "Folder", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectFileMetas_Folder_Name",
                table: "ConnectFileMetas",
                columns: new[] { "Folder", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectBlockInfos");

            migrationBuilder.DropTable(
                name: "ConnectDeviceIndexes");

            migrationBuilder.DropTable(
                name: "ConnectFileMetas");

            migrationBuilder.DropTable(
                name: "ConnectFolderMetas");
        }
    }
}
