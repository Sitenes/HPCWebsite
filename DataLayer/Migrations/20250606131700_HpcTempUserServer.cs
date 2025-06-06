using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class HpcTempUserServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "HpcCartItems");

            migrationBuilder.DropColumn(
                name: "ServerName",
                table: "HpcCartItems");

            migrationBuilder.DropColumn(
                name: "ServerSpecs",
                table: "HpcCartItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "HpcCartItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "HpcTempUserServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ServerId = table.Column<int>(type: "int", nullable: false),
                    WorkflowUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HpcTempUserServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HpcTempUserServers_HpcServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "HpcServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HpcTempUserServers_HpcUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "HpcUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HpcCartItems_ServerId",
                table: "HpcCartItems",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_HpcTempUserServers_ServerId",
                table: "HpcTempUserServers",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_HpcTempUserServers_UserId",
                table: "HpcTempUserServers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HpcCartItems_HpcServers_ServerId",
                table: "HpcCartItems",
                column: "ServerId",
                principalTable: "HpcServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HpcCartItems_HpcServers_ServerId",
                table: "HpcCartItems");

            migrationBuilder.DropTable(
                name: "HpcTempUserServers");

            migrationBuilder.DropIndex(
                name: "IX_HpcCartItems_ServerId",
                table: "HpcCartItems");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "HpcCartItems");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "HpcCartItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServerName",
                table: "HpcCartItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServerSpecs",
                table: "HpcCartItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
