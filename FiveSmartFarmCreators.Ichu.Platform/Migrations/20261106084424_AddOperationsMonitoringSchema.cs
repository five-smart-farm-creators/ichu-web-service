using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationsMonitoringSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "monitoring");

            migrationBuilder.CreateTable(
                name: "Geofences",
                schema: "monitoring",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RanchId = table.Column<Guid>(type: "uuid", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ToleranceMeters = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Geofences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LivestockOperations",
                schema: "monitoring",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RanchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CattleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Observation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Product = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Dose = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivestockOperations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringAlerts",
                schema: "monitoring",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RanchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CattleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerReadingId = table.Column<Guid>(type: "uuid", nullable: false),
                    GatewayId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DetectedValue = table.Column<double>(type: "double precision", nullable: true),
                    ThresholdMinValue = table.Column<double>(type: "double precision", nullable: true),
                    ThresholdMaxValue = table.Column<double>(type: "double precision", nullable: true),
                    ConfigurationRevision = table.Column<long>(type: "bigint", nullable: false),
                    GeofenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    DistanceToBoundaryMeters = table.Column<double>(type: "double precision", nullable: true),
                    RaisedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClearedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Finding = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Action = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringRuleSets",
                schema: "monitoring",
                columns: table => new
                {
                    RanchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringRuleSets", x => x.RanchId);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringThresholds",
                schema: "monitoring",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RanchId = table.Column<Guid>(type: "uuid", nullable: false),
                    LifeStage = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Metric = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    Range_MaxValue = table.Column<double>(type: "double precision", nullable: false),
                    Range_MinValue = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringThresholds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryReadings",
                schema: "monitoring",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadingId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    GatewayId = table.Column<Guid>(type: "uuid", nullable: false),
                    RanchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CattleId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActivityIndex_InvalidReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ActivityIndex_Value = table.Column<double>(type: "double precision", nullable: true),
                    BatteryPercent_InvalidReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BatteryPercent_Value = table.Column<double>(type: "double precision", nullable: true),
                    BodyTemperature_InvalidReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BodyTemperature_Value = table.Column<double>(type: "double precision", nullable: true),
                    Position_AccuracyMeters = table.Column<double>(type: "double precision", nullable: true),
                    Position_InvalidReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Position_Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Position_Longitude = table.Column<double>(type: "double precision", nullable: true),
                    RuminationMinutes_InvalidReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RuminationMinutes_Value = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryReadings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeofenceVertices",
                schema: "monitoring",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    GeofenceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeofenceVertices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeofenceVertices_Geofences_GeofenceId",
                        column: x => x.GeofenceId,
                        principalSchema: "monitoring",
                        principalTable: "Geofences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlertChanges",
                schema: "monitoring",
                columns: table => new
                {
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OperationData = table.Column<string>(type: "text", nullable: false),
                    AppliedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AlertId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertChanges", x => x.OperationId);
                    table.ForeignKey(
                        name: "FK_AlertChanges_MonitoringAlerts_AlertId",
                        column: x => x.AlertId,
                        principalSchema: "monitoring",
                        principalTable: "MonitoringAlerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlertEvidence",
                schema: "monitoring",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadingId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertEvidence", x => new { x.AlertId, x.DeviceId, x.ReadingId });
                    table.ForeignKey(
                        name: "FK_AlertEvidence_MonitoringAlerts_AlertId",
                        column: x => x.AlertId,
                        principalSchema: "monitoring",
                        principalTable: "MonitoringAlerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertChanges_AlertId",
                schema: "monitoring",
                table: "AlertChanges",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_Geofences_RanchId_LotId",
                schema: "monitoring",
                table: "Geofences",
                columns: new[] { "RanchId", "LotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeofenceVertices_GeofenceId_Sequence",
                schema: "monitoring",
                table: "GeofenceVertices",
                columns: new[] { "GeofenceId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LivestockOperations_CattleId_OccurredAt",
                schema: "monitoring",
                table: "LivestockOperations",
                columns: new[] { "CattleId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringAlerts_CattleId_Type_Status",
                schema: "monitoring",
                table: "MonitoringAlerts",
                columns: new[] { "CattleId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringAlerts_RanchId_Status_RaisedAt",
                schema: "monitoring",
                table: "MonitoringAlerts",
                columns: new[] { "RanchId", "Status", "RaisedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringThresholds_RanchId_LifeStage_Metric",
                schema: "monitoring",
                table: "MonitoringThresholds",
                columns: new[] { "RanchId", "LifeStage", "Metric" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryReadings_CattleId_RecordedAt",
                schema: "monitoring",
                table: "TelemetryReadings",
                columns: new[] { "CattleId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryReadings_DeviceId_ReadingId",
                schema: "monitoring",
                table: "TelemetryReadings",
                columns: new[] { "DeviceId", "ReadingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryReadings_RanchId_RecordedAt",
                schema: "monitoring",
                table: "TelemetryReadings",
                columns: new[] { "RanchId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertChanges",
                schema: "monitoring");

            migrationBuilder.DropTable(
                name: "AlertEvidence",
                schema: "monitoring");

            migrationBuilder.DropTable(
                name: "GeofenceVertices",
                schema: "monitoring");

            migrationBuilder.DropTable(
                name: "LivestockOperations",
                schema: "monitoring");

            migrationBuilder.DropTable(
                name: "MonitoringRuleSets",
                schema: "monitoring");

            migrationBuilder.DropTable(
                name: "MonitoringThresholds",
                schema: "monitoring");

            migrationBuilder.DropTable(
                name: "TelemetryReadings",
                schema: "monitoring");

            migrationBuilder.DropTable(
                name: "MonitoringAlerts",
                schema: "monitoring");

            migrationBuilder.DropTable(
                name: "Geofences",
                schema: "monitoring");
        }
    }
}
