using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoBot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImageCacheTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImageCache",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Character = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CacheKey = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ImageData = table.Column<byte[]>(type: "bytea", nullable: false),
                    AccessCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastAccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageCache", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageCache_CacheKey",
                table: "ImageCache",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageCache_Character",
                table: "ImageCache",
                column: "Character",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageCache_UpdatedAt",
                table: "ImageCache",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageCache");
        }
    }
}
