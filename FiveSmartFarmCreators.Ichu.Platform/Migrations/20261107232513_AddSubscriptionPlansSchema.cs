using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPlansSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "subscription");

            migrationBuilder.CreateTable(
                name: "plans",
                schema: "subscription",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    device_limit = table.Column<int>(type: "integer", nullable: false),
                    annual_fee_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    annual_fee_currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    device_unit_price_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    device_unit_price_currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                schema: "subscription",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    head_count = table.Column<int>(type: "integer", nullable: false),
                    annual_fee_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    annual_fee_currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    devices_cost_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    devices_cost_currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    total_currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    terms_version = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acquisition_source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    activated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    valid_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plan_features",
                schema: "subscription",
                columns: table => new
                {
                    feature = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_features", x => new { x.plan_id, x.feature });
                    table.ForeignKey(
                        name: "FK_plan_features_plans_plan_id",
                        column: x => x.plan_id,
                        principalSchema: "subscription",
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "subscription",
                table: "plans",
                columns: new[] { "id", "device_limit", "is_published", "name", "tier", "annual_fee_amount", "annual_fee_currency", "device_unit_price_amount", "device_unit_price_currency" },
                values: new object[,]
                {
                    { new Guid("5b1f0d2e-0c11-4a51-9b01-000000000001"), 50, true, "Basic", "Basic", 600m, "PEN", 120m, "PEN" },
                    { new Guid("5b1f0d2e-0c11-4a51-9b01-000000000002"), 200, true, "Standard", "Standard", 1800m, "PEN", 120m, "PEN" },
                    { new Guid("5b1f0d2e-0c11-4a51-9b01-000000000003"), 500, true, "Pro", "Pro", 3900m, "PEN", 120m, "PEN" }
                });

            migrationBuilder.InsertData(
                schema: "subscription",
                table: "plan_features",
                columns: new[] { "feature", "plan_id" },
                values: new object[,]
                {
                    { "Alerts", new Guid("5b1f0d2e-0c11-4a51-9b01-000000000001") },
                    { "Monitoring", new Guid("5b1f0d2e-0c11-4a51-9b01-000000000001") },
                    { "Alerts", new Guid("5b1f0d2e-0c11-4a51-9b01-000000000002") },
                    { "Analytics", new Guid("5b1f0d2e-0c11-4a51-9b01-000000000002") },
                    { "Monitoring", new Guid("5b1f0d2e-0c11-4a51-9b01-000000000002") },
                    { "Planning", new Guid("5b1f0d2e-0c11-4a51-9b01-000000000002") },
                    { "Alerts", new Guid("5b1f0d2e-0c11-4a51-9b01-000000000003") },
                    { "Analytics", new Guid("5b1f0d2e-0c11-4a51-9b01-000000000003") },
                    { "Monitoring", new Guid("5b1f0d2e-0c11-4a51-9b01-000000000003") },
                    { "Planning", new Guid("5b1f0d2e-0c11-4a51-9b01-000000000003") },
                    { "ReportExport", new Guid("5b1f0d2e-0c11-4a51-9b01-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_operation_id",
                schema: "subscription",
                table: "subscriptions",
                column: "operation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_ranch_id",
                schema: "subscription",
                table: "subscriptions",
                column: "ranch_id",
                unique: true,
                filter: "status IN ('Pending', 'Active')");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_ranch_id_status",
                schema: "subscription",
                table: "subscriptions",
                columns: new[] { "ranch_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plan_features",
                schema: "subscription");

            migrationBuilder.DropTable(
                name: "subscriptions",
                schema: "subscription");

            migrationBuilder.DropTable(
                name: "plans",
                schema: "subscription");
        }
    }
}
