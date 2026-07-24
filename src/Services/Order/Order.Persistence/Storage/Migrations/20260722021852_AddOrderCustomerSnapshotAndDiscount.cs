using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCustomerSnapshotAndDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customer_name",
                table: "orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "customer_phone",
                table: "orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "customer_phone_reverse",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "customer_phone_search",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "discount",
                table: "order_items",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer_phone_reverse",
                table: "orders",
                column: "customer_phone_reverse");

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer_phone_search",
                table: "orders",
                column: "customer_phone_search");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_orders_customer_phone_reverse",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_customer_phone_search",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customer_name",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customer_phone",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customer_phone_reverse",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customer_phone_search",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "discount",
                table: "order_items");
        }
    }
}
