using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSagaAndOrderExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "idempotency_key",
                table: "orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            // No AddColumn for "xmin" here (deliberately, unlike what the EF Core scaffold
            // generates by default): xmin is a Postgres system column that already exists on
            // every table - "ALTER TABLE ... ADD COLUMN xmin" is rejected by Postgres ("column
            // name xmin conflicts with a system column name"). OrderConfig's xmin mapping only
            // needs the model to know the column is there for concurrency-token purposes;
            // nothing needs to create it.

            // Backfill default is "Active", not empty string: every row that already existed before
            // this column was added was synced from a ProductVariationCreatedIntegrationEvent with
            // no notion of disabling - defaulting to "" would silently make pre-existing catalog
            // rows non-orderable until their next Product-side update event refreshes them.
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "order_product_catalogs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.CreateTable(
                name: "saga_execution_records",
                columns: table => new
                {
                    saga_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    saga_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    completed_steps_json = table.Column<string>(type: "jsonb", nullable: false),
                    context_data_json = table.Column<string>(type: "jsonb", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    user_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_saga_execution_records", x => x.saga_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer_id_idempotency_key",
                table: "orders",
                columns: new[] { "customer_id", "idempotency_key" },
                unique: true,
                filter: "\"idempotency_key\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_saga_execution_records_saga_name",
                table: "saga_execution_records",
                column: "saga_name");

            migrationBuilder.CreateIndex(
                name: "ix_saga_execution_records_state",
                table: "saga_execution_records",
                column: "state");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "saga_execution_records");

            migrationBuilder.DropIndex(
                name: "ix_orders_customer_id_idempotency_key",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "idempotency_key",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "status",
                table: "order_product_catalogs");
        }
    }
}
