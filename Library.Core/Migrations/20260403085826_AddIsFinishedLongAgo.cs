using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFinishedLongAgo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFinishedLongAgo",
                table: "Books",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFinishedLongAgo",
                table: "Books");
        }
    }
}
