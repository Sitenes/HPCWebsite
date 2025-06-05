using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class fixRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_BillingInformations_BillingInformationId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerRentalOrders_Payments_PaymentId",
                table: "ServerRentalOrders");

            migrationBuilder.DropIndex(
                name: "IX_ServerRentalOrders_PaymentId",
                table: "ServerRentalOrders");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "ServerRentalOrders",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "BillingInformationId",
                table: "Payments",
                newName: "ShoppingCartId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_BillingInformationId",
                table: "Payments",
                newName: "IX_Payments_ShoppingCartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ShoppingCarts_ShoppingCartId",
                table: "Payments",
                column: "ShoppingCartId",
                principalTable: "ShoppingCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ShoppingCarts_ShoppingCartId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ServerRentalOrders",
                newName: "PaymentId");

            migrationBuilder.RenameColumn(
                name: "ShoppingCartId",
                table: "Payments",
                newName: "BillingInformationId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_ShoppingCartId",
                table: "Payments",
                newName: "IX_Payments_BillingInformationId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerRentalOrders_PaymentId",
                table: "ServerRentalOrders",
                column: "PaymentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_BillingInformations_BillingInformationId",
                table: "Payments",
                column: "BillingInformationId",
                principalTable: "BillingInformations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServerRentalOrders_Payments_PaymentId",
                table: "ServerRentalOrders",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
