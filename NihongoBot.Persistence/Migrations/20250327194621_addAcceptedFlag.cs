using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoBot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addAcceptedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "Questions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "Questions");
        }
    }
}
