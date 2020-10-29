using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatBotPrime.Infra.Data.EF.Migrations
{
    public partial class MessagechangeRemoveResponse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Response",
                table: "MessageAlias");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Response",
                table: "MessageAlias",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
