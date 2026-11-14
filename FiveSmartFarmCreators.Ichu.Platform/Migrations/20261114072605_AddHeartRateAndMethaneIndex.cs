using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddHeartRateAndMethaneIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeartRate_InvalidReason",
                schema: "monitoring",
                table: "TelemetryReadings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HeartRate_Value",
                schema: "monitoring",
                table: "TelemetryReadings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MethaneIndex_InvalidReason",
                schema: "monitoring",
                table: "TelemetryReadings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MethaneIndex_Value",
                schema: "monitoring",
                table: "TelemetryReadings",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeartRate_InvalidReason",
                schema: "monitoring",
                table: "TelemetryReadings");

            migrationBuilder.DropColumn(
                name: "HeartRate_Value",
                schema: "monitoring",
                table: "TelemetryReadings");

            migrationBuilder.DropColumn(
                name: "MethaneIndex_InvalidReason",
                schema: "monitoring",
                table: "TelemetryReadings");

            migrationBuilder.DropColumn(
                name: "MethaneIndex_Value",
                schema: "monitoring",
                table: "TelemetryReadings");
        }
    }
}
