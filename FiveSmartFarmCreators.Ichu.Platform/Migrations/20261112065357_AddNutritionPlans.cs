using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nutrition_plans",
                schema: "planning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true),
                    target_daily_gain_kilograms = table.Column<decimal>(type: "numeric(5,3)", nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nutrition_plans", x => x.id);
                    table.CheckConstraint("ck_nutrition_plans_validity", "valid_to IS NULL OR valid_to >= valid_from");
                });

            migrationBuilder.CreateTable(
                name: "nutrition_plan_items",
                schema: "planning",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    feed = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    kilograms_per_head_per_day = table.Column<decimal>(type: "numeric(7,3)", nullable: false),
                    nutrition_plan_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nutrition_plan_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_nutrition_plan_items_nutrition_plans_nutrition_plan_id",
                        column: x => x.nutrition_plan_id,
                        principalSchema: "planning",
                        principalTable: "nutrition_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nutrition_plan_items_nutrition_plan_id",
                schema: "planning",
                table: "nutrition_plan_items",
                column: "nutrition_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_nutrition_plans_lot_id_valid_from",
                schema: "planning",
                table: "nutrition_plans",
                columns: new[] { "lot_id", "valid_from" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nutrition_plan_items",
                schema: "planning");

            migrationBuilder.DropTable(
                name: "nutrition_plans",
                schema: "planning");
        }
    }
}
