using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.Inventory.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductVariantToVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "product_variant_id",
                table: "inventory_reservations",
                newName: "variant_id");

            migrationBuilder.RenameColumn(
                name: "product_variant_id",
                table: "inventory_document_items",
                newName: "variant_id");

            migrationBuilder.RenameIndex(
                name: "ix_inventory_document_items_product_variant_id",
                table: "inventory_document_items",
                newName: "ix_inventory_document_items_variant_id");

            migrationBuilder.RenameColumn(
                name: "product_variant_id",
                table: "inventory_count_items",
                newName: "variant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "variant_id",
                table: "inventory_reservations",
                newName: "product_variant_id");

            migrationBuilder.RenameColumn(
                name: "variant_id",
                table: "inventory_document_items",
                newName: "product_variant_id");

            migrationBuilder.RenameIndex(
                name: "ix_inventory_document_items_variant_id",
                table: "inventory_document_items",
                newName: "ix_inventory_document_items_product_variant_id");

            migrationBuilder.RenameColumn(
                name: "variant_id",
                table: "inventory_count_items",
                newName: "product_variant_id");
        }
    }
}
