using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Genre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: ""),
                    TotalPages = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentPage = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    IsCurrentlyReading = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateFinished = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Rating = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    Notes = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_DateAdded",
                table: "Books",
                column: "DateAdded");

            migrationBuilder.CreateIndex(
                name: "IX_Books_DateFinished",
                table: "Books",
                column: "DateFinished");

            migrationBuilder.CreateIndex(
                name: "IX_Books_IsCurrentlyReading",
                table: "Books",
                column: "IsCurrentlyReading");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
