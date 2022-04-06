using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizyfyAPI.Migrations
{
    public partial class Changeimageurlcolumnname : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Images",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Images",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
