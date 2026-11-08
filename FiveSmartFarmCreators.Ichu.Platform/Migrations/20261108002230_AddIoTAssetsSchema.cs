using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddIoTAssetsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "iot");

            migrationBuilder.CreateTable(
                name: "devices",
                schema: "iot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    serial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    device_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    firmware_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    last_communication_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_communication_received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    battery_percentage = table.Column<short>(type: "smallint", nullable: true),
                    battery_recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.id);
                    table.UniqueConstraint("AK_devices_id_ranch_id", x => new { x.id, x.ranch_id });
                    table.CheckConstraint("ck_iot_devices_battery", "battery_percentage BETWEEN 0 AND 100");
                    table.CheckConstraint("ck_iot_devices_status", "status IN ('Available', 'InService', 'Maintenance')");
                    table.CheckConstraint("ck_iot_devices_type", "device_type IN ('SmartCollar', 'WaterController')");
                });

            migrationBuilder.CreateTable(
                name: "water_troughs",
                schema: "iot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    lot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    min_temperature_celsius = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    max_temperature_celsius = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    version = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_water_troughs", x => x.id);
                    table.CheckConstraint("ck_iot_troughs_temperature", "(min_temperature_celsius IS NULL AND max_temperature_celsius IS NULL) OR (min_temperature_celsius IS NOT NULL AND max_temperature_celsius IS NOT NULL AND min_temperature_celsius < max_temperature_celsius)");
                });

            migrationBuilder.CreateTable(
                name: "device_assignments",
                schema: "iot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    release_reason = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_assignments", x => x.id);
                    table.CheckConstraint("ck_iot_assignments_interval", "ended_at IS NULL OR ended_at >= started_at");
                    table.CheckConstraint("ck_iot_assignments_release", "(ended_at IS NULL AND release_reason IS NULL) OR (ended_at IS NOT NULL AND release_reason IS NOT NULL AND length(trim(release_reason)) > 0)");
                    table.CheckConstraint("ck_iot_assignments_target", "target_type IN ('Cattle', 'WaterTrough')");
                    table.ForeignKey(
                        name: "FK_device_assignments_devices_device_id_ranch_id",
                        columns: x => new { x.device_id, x.ranch_id },
                        principalSchema: "iot",
                        principalTable: "devices",
                        principalColumns: new[] { "id", "ranch_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_device_assignments_device_id_ranch_id",
                schema: "iot",
                table: "device_assignments",
                columns: new[] { "device_id", "ranch_id" });

            migrationBuilder.CreateIndex(
                name: "ux_iot_assignments_open_device",
                schema: "iot",
                table: "device_assignments",
                column: "device_id",
                unique: true,
                filter: "ended_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_iot_assignments_open_target",
                schema: "iot",
                table: "device_assignments",
                columns: new[] { "ranch_id", "target_type", "target_id" },
                unique: true,
                filter: "ended_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_devices_ranch_id",
                schema: "iot",
                table: "devices",
                column: "ranch_id");

            migrationBuilder.CreateIndex(
                name: "ux_iot_devices_serial",
                schema: "iot",
                table: "devices",
                column: "serial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_water_troughs_ranch_id",
                schema: "iot",
                table: "water_troughs",
                column: "ranch_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_assignments",
                schema: "iot");

            migrationBuilder.DropTable(
                name: "water_troughs",
                schema: "iot");

            migrationBuilder.DropTable(
                name: "devices",
                schema: "iot");
        }
    }
}
