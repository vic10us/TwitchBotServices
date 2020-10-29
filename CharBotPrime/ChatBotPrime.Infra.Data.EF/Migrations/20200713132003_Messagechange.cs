using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatBotPrime.Infra.Data.EF.Migrations
{
    public partial class Messagechange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageTest",
                table: "BasicMessages");

            migrationBuilder.AddColumn<string>(
                name: "Word",
                table: "MessageAlias",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageText",
                table: "BasicMessages",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Word",
                table: "MessageAlias");

            migrationBuilder.DropColumn(
                name: "MessageText",
                table: "BasicMessages");

            migrationBuilder.AddColumn<string>(
                name: "MessageTest",
                table: "BasicMessages",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
