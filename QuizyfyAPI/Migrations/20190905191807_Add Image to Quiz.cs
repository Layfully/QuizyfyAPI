using Microsoft.EntityFrameworkCore.Migrations;

namespace QuizyfyAPI.Migrations
{
    public partial class AddImagetoQuiz : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Quizzes");

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "Quizzes",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_ImageId",
                table: "Quizzes",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Images_ImageId",
                table: "Quizzes",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Images_ImageId",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_ImageId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Quizzes");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Quizzes",
                nullable: true);
        }
    }
}
