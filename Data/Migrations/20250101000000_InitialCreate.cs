using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Data.Migrations
{
    /// <summary>
    /// Начальная миграция для создания базы данных библиотеки
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Создание таблицы Authors
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            // Создание таблицы Books
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SeriesTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SeriesNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    TotalPages = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCurrentlyReading = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DateAdded = table.Column<string>(type: "TEXT", nullable: false),
                    DateFinished = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            // Создание таблицы PagesReadInDate
            migrationBuilder.CreateTable(
                name: "PagesReadInDate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<string>(type: "TEXT", nullable: false),
                    PagesRead = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagesReadInDate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagesReadInDate_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Создание таблицы многие-ко-многим BookAuthors
            migrationBuilder.CreateTable(
                name: "BookAuthors",
                columns: table => new
                {
                    AuthorsId = table.Column<int>(type: "INTEGER", nullable: false),
                    BooksId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookAuthors", x => new { x.AuthorsId, x.BooksId });
                    table.ForeignKey(
                        name: "FK_BookAuthors_Authors_AuthorsId",
                        column: x => x.AuthorsId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookAuthors_Books_BooksId",
                        column: x => x.BooksId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Создание индексов для Authors
            migrationBuilder.CreateIndex(
                name: "IX_Authors_Name",
                table: "Authors",
                column: "Name");

            // Создание индексов для Books
            migrationBuilder.CreateIndex(
                name: "IX_Books_Title",
                table: "Books",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Books_IsCurrentlyReading",
                table: "Books",
                column: "IsCurrentlyReading");

            migrationBuilder.CreateIndex(
                name: "IX_Books_DateAdded",
                table: "Books",
                column: "DateAdded");

            migrationBuilder.CreateIndex(
                name: "IX_Books_DateFinished",
                table: "Books",
                column: "DateFinished");

            migrationBuilder.CreateIndex(
                name: "IX_Books_SeriesTitle",
                table: "Books",
                column: "SeriesTitle");

            // Создание индексов для PagesReadInDate
            migrationBuilder.CreateIndex(
                name: "IX_PagesReadInDate_BookId_Date",
                table: "PagesReadInDate",
                columns: new[] { "BookId", "Date" },
                unique: true);

            // Создание индексов для BookAuthors
            migrationBuilder.CreateIndex(
                name: "IX_BookAuthors_BooksId",
                table: "BookAuthors",
                column: "BooksId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Удаление таблиц в обратном порядке (учитывая внешние ключи)
            migrationBuilder.DropTable(
                name: "BookAuthors");

            migrationBuilder.DropTable(
                name: "PagesReadInDate");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Authors");
        }
    }
}

