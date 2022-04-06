using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizyfyAPI.Migrations
{
    public partial class Addverificationemailtokentouser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VerificationToken",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerificationToken",
                table: "Users");
        }
    }
}
