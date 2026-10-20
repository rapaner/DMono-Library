using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddBookStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Books",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE Books
                SET Status = CASE
                    WHEN IsCurrentlyReading = 1 THEN 1
                    WHEN DateFinished IS NOT NULL THEN 2
                    WHEN IsFinishedLongAgo = 1 THEN 3
                    ELSE 0
                END;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Books_IsCurrentlyReading",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "IsCurrentlyReading",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "IsFinishedLongAgo",
                table: "Books");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Status",
                table: "Books",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_Status",
                table: "Books");

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrentlyReading",
                table: "Books",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinishedLongAgo",
                table: "Books",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE Books
                SET IsCurrentlyReading = CASE WHEN Status = 1 THEN 1 ELSE 0 END,
                    IsFinishedLongAgo = CASE WHEN Status = 3 THEN 1 ELSE 0 END;
                """);

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Books");

            migrationBuilder.CreateIndex(
                name: "IX_Books_IsCurrentlyReading",
                table: "Books",
                column: "IsCurrentlyReading");
        }
    }
}
