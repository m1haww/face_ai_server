using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PhotoAiBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tuneid = table.Column<int>(name: "tune-id", type: "integer", nullable: false),
                    fcmtokenid = table.Column<string>(name: "fcm-token-id", type: "text", nullable: true),
                    gender = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "image-jobs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<Guid>(name: "user-id", type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    systemprompt = table.Column<string>(name: "system-prompt", type: "text", nullable: false),
                    creationdate = table.Column<DateTime>(name: "creation-date", type: "timestamp with time zone", nullable: false),
                    images = table.Column<string>(type: "text", nullable: false),
                    hasshownphotos = table.Column<bool>(name: "has-shown-photos", type: "boolean", nullable: false),
                    presetcategory = table.Column<int>(name: "preset-category", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_image-jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_image-jobs_users_user-id",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_image-jobs_user-id",
                table: "image-jobs",
                column: "user-id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "image-jobs");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
