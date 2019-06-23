using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace QuizyfyAPI.Migrations
{
    public partial class AddChoicestable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_Quizzes_QuizId",
                table: "Question");

            migrationBuilder.AlterColumn<int>(
                name: "QuizId",
                table: "Question",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Choice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Text = table.Column<string>(nullable: true),
                    IsRight = table.Column<bool>(nullable: false),
                    QuestionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Choice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Choice_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Choice",
                columns: new[] { "Id", "IsRight", "QuestionId", "Text" },
                values: new object[,]
                {
                    { 1, false, 1, "Entity Framework From Scratch" },
                    { 2, true, 1, "Writing Sample Data Made Easy" },
                    { 3, false, 2, "Writing Sample Data Made Easy" },
                    { 4, true, 2, "ANSWER" }
                });

            migrationBuilder.UpdateData(
                table: "Question",
                keyColumn: "Id",
                keyValue: 1,
                column: "QuizId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Question",
                keyColumn: "Id",
                keyValue: 2,
                column: "QuizId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2019, 6, 23, 22, 9, 5, 101, DateTimeKind.Local).AddTicks(9527));

            migrationBuilder.CreateIndex(
                name: "IX_Choice_QuestionId",
                table: "Choice",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_Quizzes_QuizId",
                table: "Question",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_Quizzes_QuizId",
                table: "Question");

            migrationBuilder.DropTable(
                name: "Choice");

            migrationBuilder.AlterColumn<int>(
                name: "QuizId",
                table: "Question",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.UpdateData(
                table: "Question",
                keyColumn: "Id",
                keyValue: 1,
                column: "QuizId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Question",
                keyColumn: "Id",
                keyValue: 2,
                column: "QuizId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2019, 6, 23, 21, 27, 14, 764, DateTimeKind.Local).AddTicks(4164));

            migrationBuilder.AddForeignKey(
                name: "FK_Question_Quizzes_QuizId",
                table: "Question",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
