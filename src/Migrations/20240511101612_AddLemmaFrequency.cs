using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpellingBee.Migrations
{
    /// <inheritdoc />
    public partial class AddLemmaFrequency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "LemmaFrequency",
                table: "Words",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LemmaFrequency",
                table: "Words");
        }
    }
}
