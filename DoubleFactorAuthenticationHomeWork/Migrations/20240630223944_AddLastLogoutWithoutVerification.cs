using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoubleFactorAuthenticationHomeWork.Migrations
{
    public partial class AddLastLogoutWithoutVerification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LastLogoutWithoutVerification",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLogoutWithoutVerification",
                table: "AspNetUsers");
        }
    }
}
