using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatBotPrime.Infra.Data.EF.Migrations
{
    public partial class Commandchange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageText",
                table: "BasicCommands");

            migrationBuilder.AddColumn<string>(
                name: "CommandText",
                table: "BasicCommands",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAllowedToRun",
                table: "BasicCommands",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRun",
                table: "BasicCommands",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommandText",
                table: "BasicCommands");

            migrationBuilder.DropColumn(
                name: "IsAllowedToRun",
                table: "BasicCommands");

            migrationBuilder.DropColumn(
                name: "LastRun",
                table: "BasicCommands");

            migrationBuilder.AddColumn<string>(
                name: "MessageText",
                table: "BasicCommands",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
