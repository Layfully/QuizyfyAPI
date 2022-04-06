using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizyfyAPI.Migrations
{
    public partial class AddDateAddedcolumntoquizzestable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateAdded",
                table: "Quizzes",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateAdded",
                table: "Quizzes");
        }
    }
}
