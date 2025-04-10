using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoBot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuestionsPerDay",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<bool>(
                name: "WordOfTheDayEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionsPerDay",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WordOfTheDayEnabled",
                table: "Users");
        }
    }
}
