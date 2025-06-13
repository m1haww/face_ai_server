using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PhotoAiBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRunwayImageJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "runway-image-jobs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    imagejobid = table.Column<int>(name: "image-job-id", type: "integer", nullable: false),
                    runwaytaskid = table.Column<string>(name: "runway-task-id", type: "text", nullable: false),
                    tasktype = table.Column<string>(name: "task-type", type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    failurereason = table.Column<string>(name: "failure-reason", type: "text", nullable: true),
                    failurecode = table.Column<string>(name: "failure-code", type: "text", nullable: true),
                    createdat = table.Column<DateTime>(name: "created-at", type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(name: "updated-at", type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runway-image-jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_runway-image-jobs_image-jobs_image-job-id",
                        column: x => x.imagejobid,
                        principalTable: "image-jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_runway-image-jobs_image-job-id",
                table: "runway-image-jobs",
                column: "image-job-id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "runway-image-jobs");
        }
    }
}
