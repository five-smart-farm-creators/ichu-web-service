using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddCattleInformationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cattle");

            migrationBuilder.CreateTable(
                name: "cattle",
                schema: "cattle",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ear_tag = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    breed = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    sex = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    current_stage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    exit_reason = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    exit_date = table.Column<DateOnly>(type: "date", nullable: true),
                    version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cattle", x => x.id);
                    table.UniqueConstraint("ak_cattle_ranch_id_id", x => new { x.ranch_id, x.id });
                });

            migrationBuilder.CreateTable(
                name: "lots",
                schema: "cattle",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    criterion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lots", x => x.id);
                    table.UniqueConstraint("ak_lots_ranch_id_id", x => new { x.ranch_id, x.id });
                });

            migrationBuilder.CreateTable(
                name: "stage_periods",
                schema: "cattle",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cattle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    life_stage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stage_periods", x => x.id);
                    table.ForeignKey(
                        name: "FK_stage_periods_cattle_cattle_id",
                        column: x => x.cattle_id,
                        principalSchema: "cattle",
                        principalTable: "cattle",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lot_memberships",
                schema: "cattle",
                columns: table => new
                {
                    lot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cattle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lot_memberships", x => new { x.cattle_id, x.lot_id });
                    table.ForeignKey(
                        name: "FK_lot_memberships_cattle_ranch_id_cattle_id",
                        columns: x => new { x.ranch_id, x.cattle_id },
                        principalSchema: "cattle",
                        principalTable: "cattle",
                        principalColumns: new[] { "ranch_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lot_memberships_lots_ranch_id_lot_id",
                        columns: x => new { x.ranch_id, x.lot_id },
                        principalSchema: "cattle",
                        principalTable: "lots",
                        principalColumns: new[] { "ranch_id", "id" });
                });

            migrationBuilder.CreateIndex(
                name: "uq_cattle_ranch_ear_tag",
                schema: "cattle",
                table: "cattle",
                columns: new[] { "ranch_id", "ear_tag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lot_memberships_ranch_id_cattle_id",
                schema: "cattle",
                table: "lot_memberships",
                columns: new[] { "ranch_id", "cattle_id" });

            migrationBuilder.CreateIndex(
                name: "IX_lot_memberships_ranch_id_lot_id",
                schema: "cattle",
                table: "lot_memberships",
                columns: new[] { "ranch_id", "lot_id" });

            migrationBuilder.CreateIndex(
                name: "IX_stage_periods_cattle_id_valid_from",
                schema: "cattle",
                table: "stage_periods",
                columns: new[] { "cattle_id", "valid_from" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lot_memberships",
                schema: "cattle");

            migrationBuilder.DropTable(
                name: "stage_periods",
                schema: "cattle");

            migrationBuilder.DropTable(
                name: "lots",
                schema: "cattle");

            migrationBuilder.DropTable(
                name: "cattle",
                schema: "cattle");
        }
    }
}
