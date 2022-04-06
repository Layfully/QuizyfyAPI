using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizyfyAPI.Migrations
{
    public partial class Addimagetoquestion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "Questions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ImageId",
                table: "Questions",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Images_ImageId",
                table: "Questions",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Images_ImageId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_ImageId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Questions");
        }
    }
}
