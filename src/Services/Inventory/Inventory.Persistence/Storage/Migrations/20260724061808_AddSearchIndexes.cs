using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_product_id",
                table: "inventory_transactions",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_product_variation_id",
                table: "inventory_transactions",
                column: "product_variation_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_type",
                table: "inventory_transactions",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_warehouse_id",
                table: "inventory_transactions",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventories_warehouse_id",
                table: "inventories",
                column: "warehouse_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_inventory_transactions_product_id",
                table: "inventory_transactions");

            migrationBuilder.DropIndex(
                name: "ix_inventory_transactions_product_variation_id",
                table: "inventory_transactions");

            migrationBuilder.DropIndex(
                name: "ix_inventory_transactions_type",
                table: "inventory_transactions");

            migrationBuilder.DropIndex(
                name: "ix_inventory_transactions_warehouse_id",
                table: "inventory_transactions");

            migrationBuilder.DropIndex(
                name: "ix_inventories_warehouse_id",
                table: "inventories");
        }
    }
}
