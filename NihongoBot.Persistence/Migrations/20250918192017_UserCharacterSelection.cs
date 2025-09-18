using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoBot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserCharacterSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnabledCharacters",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnabledCharacters",
                table: "Users");
        }
    }
}
