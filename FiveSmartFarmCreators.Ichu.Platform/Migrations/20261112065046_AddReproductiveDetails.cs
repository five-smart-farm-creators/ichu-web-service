using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddReproductiveDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "Reproductive_ExpectedCalvingDate",
                schema: "monitoring",
                table: "LivestockOperations",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reproductive_PregnancyResult",
                schema: "monitoring",
                table: "LivestockOperations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reproductive_SireReference",
                schema: "monitoring",
                table: "LivestockOperations",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reproductive_ExpectedCalvingDate",
                schema: "monitoring",
                table: "LivestockOperations");

            migrationBuilder.DropColumn(
                name: "Reproductive_PregnancyResult",
                schema: "monitoring",
                table: "LivestockOperations");

            migrationBuilder.DropColumn(
                name: "Reproductive_SireReference",
                schema: "monitoring",
                table: "LivestockOperations");
        }
    }
}
