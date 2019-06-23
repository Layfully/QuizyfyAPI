using System;
using Microsoft.EntityFrameworkCore.Migrations;

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

            migrationBuilder.UpdateData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2019, 6, 23, 21, 7, 30, 475, DateTimeKind.Local).AddTicks(2168));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateAdded",
                table: "Quizzes");
        }
    }
}
