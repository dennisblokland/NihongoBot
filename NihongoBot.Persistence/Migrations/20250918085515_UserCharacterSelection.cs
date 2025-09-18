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
            migrationBuilder.AddColumn<bool>(
                name: "KaEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "KiEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "KuEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "KeEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "KoEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KaEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KiEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KuEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KeEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KoEnabled",
                table: "Users");
        }
    }
}
