using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCore.Order.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexOptimizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_saga_execution_records_saga_name",
                table: "saga_execution_records");

            migrationBuilder.DropIndex(
                name: "ix_saga_execution_records_state",
                table: "saga_execution_records");

            migrationBuilder.DropIndex(
                name: "ix_orders_status",
                table: "orders");

            migrationBuilder.CreateIndex(
                name: "ix_saga_execution_records_saga_name_started_at",
                table: "saga_execution_records",
                columns: new[] { "saga_name", "started_at" });

            migrationBuilder.CreateIndex(
                name: "ix_saga_execution_records_state_started_at",
                table: "saga_execution_records",
                columns: new[] { "state", "started_at" });

            migrationBuilder.CreateIndex(
                name: "idx_outbox_unprocessed_created_at",
                table: "outbox_messages",
                column: "created_at",
                filter: "\"processed_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_orders_created_at",
                table: "orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_orders_status_created_at",
                table: "orders",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_order_owners_customer_name",
                table: "order_owners",
                column: "customer_name");

            migrationBuilder.CreateIndex(
                name: "idx_inbox_status_created_at",
                table: "inbox_messages",
                columns: new[] { "status", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_saga_execution_records_saga_name_started_at",
                table: "saga_execution_records");

            migrationBuilder.DropIndex(
                name: "ix_saga_execution_records_state_started_at",
                table: "saga_execution_records");

            migrationBuilder.DropIndex(
                name: "idx_outbox_unprocessed_created_at",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "ix_orders_created_at",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_status_created_at",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_order_owners_customer_name",
                table: "order_owners");

            migrationBuilder.DropIndex(
                name: "idx_inbox_status_created_at",
                table: "inbox_messages");

            migrationBuilder.CreateIndex(
                name: "ix_saga_execution_records_saga_name",
                table: "saga_execution_records",
                column: "saga_name");

            migrationBuilder.CreateIndex(
                name: "ix_saga_execution_records_state",
                table: "saga_execution_records",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "ix_orders_status",
                table: "orders",
                column: "status");
        }
    }
}
