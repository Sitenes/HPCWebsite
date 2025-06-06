using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class HpcTemp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HpcTempUserServers_HpcUsers_UserId",
                table: "HpcTempUserServers");

            migrationBuilder.AddForeignKey(
                name: "FK_HpcTempUserServers_User_UserId",
                table: "HpcTempUserServers",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HpcTempUserServers_User_UserId",
                table: "HpcTempUserServers");

            migrationBuilder.AddForeignKey(
                name: "FK_HpcTempUserServers_HpcUsers_UserId",
                table: "HpcTempUserServers",
                column: "UserId",
                principalTable: "HpcUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
