using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddEdgeGatewaysSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "edge_gateways",
                schema: "iot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    key_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    registered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edge_gateways", x => x.id);
                    table.CheckConstraint("ck_iot_edge_gateways_key_hash", "octet_length(key_hash) = 32");
                    table.CheckConstraint("ck_iot_edge_gateways_status", "status IN ('Active', 'Revoked')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_edge_gateways_ranch_id",
                schema: "iot",
                table: "edge_gateways",
                column: "ranch_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "edge_gateways",
                schema: "iot");
        }
    }
}
