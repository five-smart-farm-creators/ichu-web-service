using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddWaterTroughReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WaterTroughReadings",
                schema: "monitoring",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControllerId = table.Column<Guid>(type: "uuid", nullable: false),
                    WaterTroughId = table.Column<Guid>(type: "uuid", nullable: false),
                    RanchId = table.Column<Guid>(type: "uuid", nullable: false),
                    GatewayId = table.Column<Guid>(type: "uuid", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric", nullable: false),
                    TemperatureUnit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    HeaterState = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Origin = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterTroughReadings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WaterTroughReadings_ControllerId_ReadingId",
                schema: "monitoring",
                table: "WaterTroughReadings",
                columns: new[] { "ControllerId", "ReadingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaterTroughReadings_RanchId_ControllerId_CapturedAt",
                schema: "monitoring",
                table: "WaterTroughReadings",
                columns: new[] { "RanchId", "ControllerId", "CapturedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WaterTroughReadings_RanchId_WaterTroughId_CapturedAt",
                schema: "monitoring",
                table: "WaterTroughReadings",
                columns: new[] { "RanchId", "WaterTroughId", "CapturedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WaterTroughReadings",
                schema: "monitoring");
        }
    }
}
