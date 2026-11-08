using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanningSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "planning");

            migrationBuilder.CreateTable(
                name: "campaigns",
                schema: "planning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    starts_on = table.Column<DateOnly>(type: "date", nullable: false),
                    ends_on = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campaigns", x => x.id);
                    table.CheckConstraint("ck_campaigns_date_range", "ends_on >= starts_on");
                });

            migrationBuilder.CreateTable(
                name: "withdrawal_periods",
                schema: "planning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cattle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    intervention_id = table.Column<Guid>(type: "uuid", nullable: true),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_withdrawal_periods", x => x.id);
                    table.CheckConstraint("ck_withdrawal_periods_interval", "ends_at >= starts_at");
                });

            migrationBuilder.CreateTable(
                name: "planned_activities",
                schema: "planning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cattle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planned_activities", x => x.id);
                    table.ForeignKey(
                        name: "FK_planned_activities_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalSchema: "planning",
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "application_records",
                schema: "planning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cattle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    applied_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    intervention_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_application_records_planned_activities_activity_id",
                        column: x => x.activity_id,
                        principalSchema: "planning",
                        principalTable: "planned_activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "follow_ups",
                schema: "planning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_follow_ups", x => x.id);
                    table.ForeignKey(
                        name: "FK_follow_ups_planned_activities_activity_id",
                        column: x => x.activity_id,
                        principalSchema: "planning",
                        principalTable: "planned_activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_application_records_activity_id_applied_at",
                schema: "planning",
                table: "application_records",
                columns: new[] { "activity_id", "applied_at" });

            migrationBuilder.CreateIndex(
                name: "ix_application_records_intervention_id",
                schema: "planning",
                table: "application_records",
                column: "intervention_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_ranch_id_starts_on",
                schema: "planning",
                table: "campaigns",
                columns: new[] { "ranch_id", "starts_on" });

            migrationBuilder.CreateIndex(
                name: "ix_follow_ups_activity_id_due_at",
                schema: "planning",
                table: "follow_ups",
                columns: new[] { "activity_id", "due_at" });

            migrationBuilder.CreateIndex(
                name: "ix_planned_activities_campaign_id",
                schema: "planning",
                table: "planned_activities",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "ix_planned_activities_ranch_id_scheduled_at",
                schema: "planning",
                table: "planned_activities",
                columns: new[] { "ranch_id", "scheduled_at" });

            migrationBuilder.CreateIndex(
                name: "ix_withdrawal_periods_intervention_id",
                schema: "planning",
                table: "withdrawal_periods",
                column: "intervention_id");

            migrationBuilder.CreateIndex(
                name: "ix_withdrawal_periods_ranch_cattle_starts_at",
                schema: "planning",
                table: "withdrawal_periods",
                columns: new[] { "ranch_id", "cattle_id", "starts_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_records",
                schema: "planning");

            migrationBuilder.DropTable(
                name: "follow_ups",
                schema: "planning");

            migrationBuilder.DropTable(
                name: "withdrawal_periods",
                schema: "planning");

            migrationBuilder.DropTable(
                name: "planned_activities",
                schema: "planning");

            migrationBuilder.DropTable(
                name: "campaigns",
                schema: "planning");
        }
    }
}
