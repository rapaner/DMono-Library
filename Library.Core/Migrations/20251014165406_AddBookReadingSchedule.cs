using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddBookReadingSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookReadingSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetFinishDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartHour = table.Column<int>(type: "INTEGER", nullable: true),
                    EndHour = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookReadingSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookReadingSchedules_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookReadingSchedules_BookId",
                table: "BookReadingSchedules",
                column: "BookId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookReadingSchedules");
        }
    }
}
