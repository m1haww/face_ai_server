using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoAiBackend.Migrations
{
    /// <inheritdoc />
    public partial class ChangedImageData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "data",
                table: "enhance-images");

            migrationBuilder.AddColumn<string>(
                name: "image-url",
                table: "enhance-images",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image-url",
                table: "enhance-images");

            migrationBuilder.AddColumn<byte[]>(
                name: "data",
                table: "enhance-images",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
