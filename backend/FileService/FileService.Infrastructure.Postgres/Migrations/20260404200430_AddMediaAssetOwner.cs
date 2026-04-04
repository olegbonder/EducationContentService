using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaAssetOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "video_processing",
                newName: "video_processing",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "processing_steps",
                newName: "processing_steps",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "media_assets",
                newName: "media_assets",
                newSchema: "public");

            migrationBuilder.AddColumn<string>(
                name: "meta_data",
                schema: "public",
                table: "video_processing",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "owner",
                schema: "public",
                table: "media_assets",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "meta_data",
                schema: "public",
                table: "video_processing");

            migrationBuilder.DropColumn(
                name: "owner",
                schema: "public",
                table: "media_assets");

            migrationBuilder.RenameTable(
                name: "video_processing",
                schema: "public",
                newName: "video_processing");

            migrationBuilder.RenameTable(
                name: "processing_steps",
                schema: "public",
                newName: "processing_steps");

            migrationBuilder.RenameTable(
                name: "media_assets",
                schema: "public",
                newName: "media_assets");
        }
    }
}
