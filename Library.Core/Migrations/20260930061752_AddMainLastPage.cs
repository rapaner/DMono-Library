using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMainLastPage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MainLastPage",
                table: "Books",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainLastPage",
                table: "Books");
        }
    }
}
