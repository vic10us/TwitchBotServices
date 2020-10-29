using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatBotPrime.Infra.Data.EF.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BasicCommands",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MessageText = table.Column<string>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    Cooldown = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasicCommands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BasicMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MessageTest = table.Column<string>(nullable: true),
                    Response = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasicMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommandAlias",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    commandId = table.Column<Guid>(nullable: true),
                    Word = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandAlias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommandAlias_BasicCommands_commandId",
                        column: x => x.commandId,
                        principalTable: "BasicCommands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageAlias",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MessageId = table.Column<Guid>(nullable: true),
                    Response = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageAlias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageAlias_BasicMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "BasicMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommandAlias_commandId",
                table: "CommandAlias",
                column: "commandId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAlias_MessageId",
                table: "MessageAlias",
                column: "MessageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommandAlias");

            migrationBuilder.DropTable(
                name: "MessageAlias");

            migrationBuilder.DropTable(
                name: "BasicCommands");

            migrationBuilder.DropTable(
                name: "BasicMessages");
        }
    }
}
