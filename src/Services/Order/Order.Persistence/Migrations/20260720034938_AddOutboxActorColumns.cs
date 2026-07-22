using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxActorColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "order_product_catalogs",
                newName: "product_name");

            migrationBuilder.AddColumn<string>(
                name: "actor_id",
                table: "outbox_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "actor_type",
                table: "outbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AddColumn<Guid>(
                name: "product_id",
                table: "order_product_catalogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "sku",
                table: "order_product_catalogs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_order_product_catalogs_product_id",
                table: "order_product_catalogs",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_order_product_catalogs_product_id",
                table: "order_product_catalogs");

            migrationBuilder.DropColumn(
                name: "actor_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "actor_type",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "product_id",
                table: "order_product_catalogs");

            migrationBuilder.DropColumn(
                name: "sku",
                table: "order_product_catalogs");

            migrationBuilder.RenameColumn(
                name: "product_name",
                table: "order_product_catalogs",
                newName: "name");
        }
    }
}
