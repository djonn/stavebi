using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpellingBee.Migrations
{
    /// <inheritdoc />
    public partial class AddNewWordsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Lemma = table.Column<string>(type: "TEXT", nullable: false),
                    Example = table.Column<string>(type: "TEXT", nullable: false),
                    Conjugation = table.Column<string>(type: "TEXT", nullable: false),
                    FullForm = table.Column<string>(type: "TEXT", nullable: false),
                    Standardized = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Words_Id",
                table: "Words",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Words");
        }
    }
}
