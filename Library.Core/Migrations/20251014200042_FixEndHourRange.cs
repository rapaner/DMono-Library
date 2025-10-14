using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Core.Migrations
{
    /// <inheritdoc />
    public partial class FixEndHourRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix any existing records where EndHour is 24 (invalid for DateTime.Hour range 0-23)
            migrationBuilder.Sql(
                @"UPDATE BookReadingSchedules 
                  SET EndHour = 23 
                  WHERE EndHour = 24");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
