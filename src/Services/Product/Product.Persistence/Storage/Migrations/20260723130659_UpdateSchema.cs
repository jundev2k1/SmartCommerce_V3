using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Product.Persistence.Storage.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_product_tag_mappings_tag_id",
                table: "product_tag_mappings",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_category_mappings_category_id",
                table: "product_category_mappings",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_category_mappings_product_categories_category_id",
                table: "product_category_mappings",
                column: "category_id",
                principalTable: "product_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_product_tag_mappings_product_tags_tag_id",
                table: "product_tag_mappings",
                column: "tag_id",
                principalTable: "product_tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_category_mappings_product_categories_category_id",
                table: "product_category_mappings");

            migrationBuilder.DropForeignKey(
                name: "fk_product_tag_mappings_product_tags_tag_id",
                table: "product_tag_mappings");

            migrationBuilder.DropIndex(
                name: "ix_product_tag_mappings_tag_id",
                table: "product_tag_mappings");

            migrationBuilder.DropIndex(
                name: "ix_product_category_mappings_category_id",
                table: "product_category_mappings");
        }
    }
}
