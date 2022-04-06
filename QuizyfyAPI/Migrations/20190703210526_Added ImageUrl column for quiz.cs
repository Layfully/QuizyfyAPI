using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizyfyAPI.Migrations
{
    public partial class AddedImageUrlcolumnforquiz : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Quizzes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Quizzes");
        }
    }
}
