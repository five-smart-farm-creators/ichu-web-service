using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AllowOneThresholdPerSeverity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MonitoringThresholds_RanchId_LifeStage_Metric",
                schema: "monitoring",
                table: "MonitoringThresholds");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringThresholds_RanchId_LifeStage_Metric_Severity",
                schema: "monitoring",
                table: "MonitoringThresholds",
                columns: new[] { "RanchId", "LifeStage", "Metric", "Severity" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MonitoringThresholds_RanchId_LifeStage_Metric_Severity",
                schema: "monitoring",
                table: "MonitoringThresholds");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringThresholds_RanchId_LifeStage_Metric",
                schema: "monitoring",
                table: "MonitoringThresholds",
                columns: new[] { "RanchId", "LifeStage", "Metric" },
                unique: true);
        }
    }
}
