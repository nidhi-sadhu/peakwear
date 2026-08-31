using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeakWear.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_order_items_product_variant_id",
                table: "order_items",
                column: "product_variant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_order_items_product_variants_product_variant_id",
                table: "order_items",
                column: "product_variant_id",
                principalTable: "product_variants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_users_user_id",
                table: "orders",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_order_items_product_variants_product_variant_id",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_users_user_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_order_items_product_variant_id",
                table: "order_items");
        }
    }
}
