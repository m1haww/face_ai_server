using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoAiBackend.Migrations
{
    /// <inheritdoc />
    public partial class RunwayJobsStandalone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_runway-image-jobs_image-jobs_image-job-id",
                table: "runway-image-jobs");

            migrationBuilder.DropIndex(
                name: "IX_runway-image-jobs_image-job-id",
                table: "runway-image-jobs");

            migrationBuilder.AlterColumn<int>(
                name: "image-job-id",
                table: "runway-image-jobs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "credit-cost",
                table: "runway-image-jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "output-urls",
                table: "runway-image-jobs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "prompt",
                table: "runway-image-jobs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "user-id",
                table: "runway-image-jobs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_runway-image-jobs_user-id",
                table: "runway-image-jobs",
                column: "user-id");

            migrationBuilder.AddForeignKey(
                name: "FK_runway-image-jobs_users_user-id",
                table: "runway-image-jobs",
                column: "user-id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_runway-image-jobs_users_user-id",
                table: "runway-image-jobs");

            migrationBuilder.DropIndex(
                name: "IX_runway-image-jobs_user-id",
                table: "runway-image-jobs");

            migrationBuilder.DropColumn(
                name: "credit-cost",
                table: "runway-image-jobs");

            migrationBuilder.DropColumn(
                name: "output-urls",
                table: "runway-image-jobs");

            migrationBuilder.DropColumn(
                name: "prompt",
                table: "runway-image-jobs");

            migrationBuilder.DropColumn(
                name: "user-id",
                table: "runway-image-jobs");

            migrationBuilder.AlterColumn<int>(
                name: "image-job-id",
                table: "runway-image-jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_runway-image-jobs_image-job-id",
                table: "runway-image-jobs",
                column: "image-job-id");

            migrationBuilder.AddForeignKey(
                name: "FK_runway-image-jobs_image-jobs_image-job-id",
                table: "runway-image-jobs",
                column: "image-job-id",
                principalTable: "image-jobs",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
