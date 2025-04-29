using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddItemsToServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessingSpeed",
                table: "Servers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SuitableFor",
                table: "Servers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ServerRentalOrders_ServerId",
                table: "ServerRentalOrders",
                column: "ServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerRentalOrders_Servers_ServerId",
                table: "ServerRentalOrders",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerRentalOrders_Servers_ServerId",
                table: "ServerRentalOrders");

            migrationBuilder.DropIndex(
                name: "IX_ServerRentalOrders_ServerId",
                table: "ServerRentalOrders");

            migrationBuilder.DropColumn(
                name: "ProcessingSpeed",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "SuitableFor",
                table: "Servers");
        }
    }
}
