using Microsoft.EntityFrameworkCore.Migrations;
using Library.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Library.Data.Migrations
{
    /// <summary>
    /// Миграция для добавления функциональности расписания чтения по часам
    /// </summary>
    [DbContext(typeof(LibraryDbContext))]
    [Migration("20250112000000_AddReadingSchedule")]
    public partial class AddReadingSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Создание таблицы BookReadingSchedules
            migrationBuilder.CreateTable(
                name: "BookReadingSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetFinishDate = table.Column<string>(type: "TEXT", nullable: false),
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

            // Создание индекса для BookReadingSchedules
            migrationBuilder.CreateIndex(
                name: "IX_BookReadingSchedules_BookId",
                table: "BookReadingSchedules",
                column: "BookId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Удаление таблиц в обратном порядке (учитывая внешние ключи)
            migrationBuilder.DropTable(
                name: "BookReadingSchedules");
        }
    }
}

