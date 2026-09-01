using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeakWear.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStripePaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "order_number_seq",
                startValue: 1001L);

            migrationBuilder.AddColumn<DateTime>(
                name: "paid_at_utc",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_payment_intent_id",
                table: "orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "processed_stripe_events",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processed_stripe_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_orders_stripe_payment_intent",
                table: "orders",
                column: "stripe_payment_intent_id",
                unique: true,
                filter: "stripe_payment_intent_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_stripe_events");

            migrationBuilder.DropIndex(
                name: "ix_orders_stripe_payment_intent",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "paid_at_utc",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "stripe_payment_intent_id",
                table: "orders");

            migrationBuilder.DropSequence(
                name: "order_number_seq");
        }
    }
}
