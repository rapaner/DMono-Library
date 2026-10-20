using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PagesReadInDate_BookId_Date",
                table: "PagesReadInDate");

            migrationBuilder.CreateTable(
                name: "ReadingSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinishDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReadingSessions_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<int>(
                name: "ReadingSessionId",
                table: "PagesReadInDate",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO ReadingSessions (BookId, Number, StartDate, FinishDate)
                SELECT
                    p.BookId,
                    1,
                    MIN(p.Date),
                    CASE
                        WHEN b.Status = 1 THEN NULL
                        WHEN b.DateFinished IS NOT NULL THEN b.DateFinished
                        ELSE (SELECT MAX(p2.Date) FROM PagesReadInDate p2 WHERE p2.BookId = p.BookId)
                    END
                FROM PagesReadInDate p
                INNER JOIN Books b ON b.Id = p.BookId
                GROUP BY p.BookId;
                """);

            migrationBuilder.Sql("""
                UPDATE PagesReadInDate
                SET ReadingSessionId = (
                    SELECT rs.Id
                    FROM ReadingSessions rs
                    WHERE rs.BookId = PagesReadInDate.BookId AND rs.Number = 1
                );
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ReadingSessionId",
                table: "PagesReadInDate",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PagesReadInDate_BookId",
                table: "PagesReadInDate",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_PagesReadInDate_SessionId_Date",
                table: "PagesReadInDate",
                columns: new[] { "ReadingSessionId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReadingSessions_BookId_Number",
                table: "ReadingSessions",
                columns: new[] { "BookId", "Number" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PagesReadInDate_ReadingSessions_ReadingSessionId",
                table: "PagesReadInDate",
                column: "ReadingSessionId",
                principalTable: "ReadingSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PagesReadInDate_ReadingSessions_ReadingSessionId",
                table: "PagesReadInDate");

            migrationBuilder.DropTable(
                name: "ReadingSessions");

            migrationBuilder.DropIndex(
                name: "IX_PagesReadInDate_BookId",
                table: "PagesReadInDate");

            migrationBuilder.DropIndex(
                name: "IX_PagesReadInDate_SessionId_Date",
                table: "PagesReadInDate");

            migrationBuilder.DropColumn(
                name: "ReadingSessionId",
                table: "PagesReadInDate");

            migrationBuilder.CreateIndex(
                name: "IX_PagesReadInDate_BookId_Date",
                table: "PagesReadInDate",
                columns: new[] { "BookId", "Date" },
                unique: true);
        }
    }
}
