using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace starsky.foundation.database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImageFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
	        // xmp: 30 -> 70
	        migrationBuilder.Sql("UPDATE FileIndex SET ImageFormat = 70 WHERE ImageFormat = 30");
	        migrationBuilder.Sql("UPDATE ImportIndex SET ImageFormat = 70 WHERE ImageFormat = 30");

	        // meta_json: 31 -> 71
	        migrationBuilder.Sql("UPDATE FileIndex SET ImageFormat = 71 WHERE ImageFormat = 31");
	        migrationBuilder.Sql("UPDATE ImportIndex SET ImageFormat = 71 WHERE ImageFormat = 31");
        }
        /// <inheritdoc />

        protected override void Down(MigrationBuilder migrationBuilder)
        {
	        // xmp: 70 -> 30
	        migrationBuilder.Sql("UPDATE FileIndex SET ImageFormat = 30 WHERE ImageFormat = 70");
	        migrationBuilder.Sql("UPDATE ImportIndex SET ImageFormat = 30 WHERE ImageFormat = 70");

	        // meta_json: 71 -> 31
	        migrationBuilder.Sql("UPDATE FileIndex SET ImageFormat = 31 WHERE ImageFormat = 71");
	        migrationBuilder.Sql("UPDATE ImportIndex SET ImageFormat = 31 WHERE ImageFormat = 71");
        }
    }
}
