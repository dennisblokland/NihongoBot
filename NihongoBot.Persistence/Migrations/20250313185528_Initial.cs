using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NihongoBot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kanas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Character = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Romaji = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kanas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: true),
                    Streak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KanaVariant",
                columns: table => new
                {
                    Character = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Romaji = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    KanaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KanaVariant", x => x.Character);
                    table.ForeignKey(
                        name: "FK_KanaVariant_Kanas_KanaId",
                        column: x => x.KanaId,
                        principalTable: "Kanas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Kanas",
                columns: new[] { "Id", "Character", "CreatedAt", "Romaji", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "あ", null, "a", 0, null },
                    { 2, "い", null, "i", 0, null },
                    { 3, "う", null, "u", 0, null },
                    { 4, "え", null, "e", 0, null },
                    { 5, "お", null, "o", 0, null },
                    { 6, "か", null, "ka", 0, null },
                    { 7, "き", null, "ki", 0, null },
                    { 8, "く", null, "ku", 0, null },
                    { 9, "け", null, "ke", 0, null },
                    { 10, "こ", null, "ko", 0, null },
                    { 11, "さ", null, "sa", 0, null },
                    { 12, "し", null, "shi", 0, null },
                    { 13, "す", null, "su", 0, null },
                    { 14, "せ", null, "se", 0, null },
                    { 15, "そ", null, "so", 0, null },
                    { 16, "た", null, "ta", 0, null },
                    { 17, "ち", null, "chi", 0, null },
                    { 18, "つ", null, "tsu", 0, null },
                    { 19, "て", null, "te", 0, null },
                    { 20, "と", null, "to", 0, null },
                    { 21, "な", null, "na", 0, null },
                    { 22, "に", null, "ni", 0, null },
                    { 23, "ぬ", null, "nu", 0, null },
                    { 24, "ね", null, "ne", 0, null },
                    { 25, "の", null, "no", 0, null },
                    { 26, "は", null, "ha", 0, null },
                    { 27, "ひ", null, "hi", 0, null },
                    { 28, "ふ", null, "fu", 0, null },
                    { 29, "へ", null, "he", 0, null },
                    { 30, "ほ", null, "ho", 0, null },
                    { 31, "ま", null, "ma", 0, null },
                    { 32, "み", null, "mi", 0, null },
                    { 33, "む", null, "mu", 0, null },
                    { 34, "め", null, "me", 0, null },
                    { 35, "も", null, "mo", 0, null },
                    { 36, "や", null, "ya", 0, null },
                    { 37, "ゆ", null, "yu", 0, null },
                    { 38, "よ", null, "yo", 0, null },
                    { 39, "ら", null, "ra", 0, null },
                    { 40, "り", null, "ri", 0, null },
                    { 41, "る", null, "ru", 0, null },
                    { 42, "れ", null, "re", 0, null },
                    { 43, "ろ", null, "ro", 0, null },
                    { 44, "わ", null, "wa", 0, null },
                    { 45, "を", null, "wo", 0, null },
                    { 46, "ん", null, "n", 0, null },
                    { 47, "きゃ", null, "kya", 0, null },
                    { 48, "きゅ", null, "kyu", 0, null },
                    { 49, "きょ", null, "kyo", 0, null },
                    { 50, "しゃ", null, "sha", 0, null },
                    { 51, "しゅ", null, "shu", 0, null },
                    { 52, "しょ", null, "sho", 0, null },
                    { 53, "ちゃ", null, "cha", 0, null },
                    { 54, "ちゅ", null, "chu", 0, null },
                    { 55, "ちょ", null, "cho", 0, null },
                    { 56, "にゃ", null, "nya", 0, null },
                    { 57, "にゅ", null, "nyu", 0, null },
                    { 58, "にょ", null, "nyo", 0, null },
                    { 59, "ひゃ", null, "hya", 0, null },
                    { 60, "ひゅ", null, "hyu", 0, null },
                    { 61, "ひょ", null, "hyo", 0, null },
                    { 62, "みゃ", null, "mya", 0, null },
                    { 63, "みゅ", null, "myu", 0, null },
                    { 64, "みょ", null, "myo", 0, null },
                    { 65, "りゃ", null, "rya", 0, null },
                    { 66, "りゅ", null, "ryu", 0, null },
                    { 67, "りょ", null, "ryo", 0, null }
                });

            migrationBuilder.InsertData(
                table: "KanaVariant",
                columns: new[] { "Character", "KanaId", "Romaji" },
                values: new object[,]
                {
                    { "ゔ", 3, "vu" },
                    { "が", 6, "ga" },
                    { "ぎ", 7, "gi" },
                    { "ぐ", 8, "gu" },
                    { "げ", 9, "ge" },
                    { "ご", 10, "go" },
                    { "ざ", 11, "za" },
                    { "じ", 12, "ji" },
                    { "ず", 13, "zu" },
                    { "ぜ", 14, "ze" },
                    { "ぞ", 15, "zo" },
                    { "だ", 16, "da" },
                    { "ぢ", 17, "ji" },
                    { "づ", 18, "zu" },
                    { "で", 19, "de" },
                    { "ど", 20, "do" },
                    { "ば", 26, "ba" },
                    { "ぱ", 26, "pa" },
                    { "び", 27, "bi" },
                    { "ぴ", 27, "pi" },
                    { "ぶ", 28, "bu" },
                    { "ぷ", 28, "pu" },
                    { "べ", 29, "be" },
                    { "ぺ", 29, "pe" },
                    { "ぼ", 30, "bo" },
                    { "ぽ", 30, "po" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kanas_Character",
                table: "Kanas",
                column: "Character",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KanaVariant_KanaId",
                table: "KanaVariant",
                column: "KanaId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TelegramId",
                table: "Users",
                column: "TelegramId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KanaVariant");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Kanas");
        }
    }
}
