using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexOptimizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_inventory_transactions_inventory_id",
                table: "inventory_transactions");

            migrationBuilder.CreateIndex(
                name: "idx_outbox_unprocessed_created_at",
                table: "outbox_messages",
                column: "created_at",
                filter: "\"processed_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_inventory_id_created_at",
                table: "inventory_transactions",
                columns: new[] { "inventory_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_inbox_status_created_at",
                table: "inbox_messages",
                columns: new[] { "status", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_outbox_unprocessed_created_at",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "ix_inventory_transactions_inventory_id_created_at",
                table: "inventory_transactions");

            migrationBuilder.DropIndex(
                name: "idx_inbox_status_created_at",
                table: "inbox_messages");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_transactions_inventory_id",
                table: "inventory_transactions",
                column: "inventory_id");
        }
    }
}
