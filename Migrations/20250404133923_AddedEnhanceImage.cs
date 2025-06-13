using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoAiBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddedEnhanceImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "output",
                table: "enhance-jobs");

            migrationBuilder.CreateTable(
                name: "enhance-images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    jobid = table.Column<string>(name: "job-id", type: "text", nullable: false),
                    data = table.Column<byte[]>(type: "bytea", nullable: false),
                    mimetype = table.Column<string>(name: "mime-type", type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enhance-images", x => x.id);
                    table.ForeignKey(
                        name: "FK_enhance-images_enhance-jobs_job-id",
                        column: x => x.jobid,
                        principalTable: "enhance-jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_enhance-images_job-id",
                table: "enhance-images",
                column: "job-id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enhance-images");

            migrationBuilder.AddColumn<string>(
                name: "output",
                table: "enhance-jobs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
