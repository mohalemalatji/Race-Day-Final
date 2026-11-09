using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.Api.Migrations
{
    public partial class AddImageBlobNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureBlobName",
                table: "USER",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerBlobName",
                table: "EVENT",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureBlobName",
                table: "USER");

            migrationBuilder.DropColumn(
                name: "BannerBlobName",
                table: "EVENT");
        }
    }
}
