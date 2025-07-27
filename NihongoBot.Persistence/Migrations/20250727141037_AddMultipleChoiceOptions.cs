using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NihongoBot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleChoiceOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MultipleChoiceOptions",
                table: "Questions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MultipleChoiceOptions",
                table: "Questions");
        }
    }
}
