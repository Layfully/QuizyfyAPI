using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace QuizyfyAPI.Migrations
{
    public partial class AddedImageUrlcolumnforquiz : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Choices",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Choices",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Choices",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Choices",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1);

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

            migrationBuilder.InsertData(
                table: "Quizzes",
                columns: new[] { "Id", "DateAdded", "Name" },
                values: new object[] { 1, new DateTime(2019, 6, 24, 18, 47, 16, 786, DateTimeKind.Local).AddTicks(1519), "Quizzserser" });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "QuizId", "Text" },
                values: new object[] { 1, 1, "Entity Framework From Scratch" });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "QuizId", "Text" },
                values: new object[] { 2, 1, "Writing Sample Data Made Easy" });

            migrationBuilder.InsertData(
                table: "Choices",
                columns: new[] { "Id", "IsRight", "QuestionId", "Text" },
                values: new object[,]
                {
                    { 1, false, 1, "Entity Framework From Scratch" },
                    { 2, true, 1, "Writing Sample Data Made Easy" },
                    { 3, false, 2, "Writing Sample Data Made Easy" },
                    { 4, true, 2, "ANSWER" }
                });
        }
    }
}
