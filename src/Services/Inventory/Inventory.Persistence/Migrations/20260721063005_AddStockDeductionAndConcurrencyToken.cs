using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStockDeductionAndConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No AddColumn for "xmin" here (deliberately, unlike what the EF Core scaffold
            // generates by default): xmin is a Postgres system column that already exists on
            // every table - "ALTER TABLE ... ADD COLUMN xmin" is rejected by Postgres ("column
            // name xmin conflicts with a system column name"). InventoryConfig's xmin mapping
            // only needs the model to know the column is there for concurrency-token purposes;
            // nothing needs to create it.
            migrationBuilder.CreateTable(
                name: "stock_deductions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    items_json = table.Column<string>(type: "jsonb", nullable: false),
                    failure_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_deductions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stock_deductions_status",
                table: "stock_deductions",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_deductions");
        }
    }
}
