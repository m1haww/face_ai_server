using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoAiBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddedEnhanceJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "has-shown-photos",
                table: "image-jobs");

            migrationBuilder.CreateTable(
                name: "enhance-jobs",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    createdat = table.Column<DateTime>(name: "created-at", type: "timestamp with time zone", nullable: false),
                    output = table.Column<string>(type: "text", nullable: false),
                    userid = table.Column<Guid>(name: "user-id", type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enhance-jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_enhance-jobs_users_user-id",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_enhance-jobs_user-id",
                table: "enhance-jobs",
                column: "user-id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enhance-jobs");

            migrationBuilder.AddColumn<bool>(
                name: "has-shown-photos",
                table: "image-jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
