using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoAiBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserCredits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "credits",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "credits",
                table: "users");
        }
    }
}
