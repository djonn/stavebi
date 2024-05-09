using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpellingBee.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWordsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Words");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Value);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Words_Value",
                table: "Words",
                column: "Value");
        }
    }
}
