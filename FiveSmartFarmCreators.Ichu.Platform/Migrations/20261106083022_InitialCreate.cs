using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "iam");

            migrationBuilder.CreateTable(
                name: "RanchRegistrations",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Region = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    District = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    EstimatedHeadCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RanchRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RegisteredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BlockedUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastFailedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessAttempts",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessAttempts_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalSchema: "iam",
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HerdAdvisories",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VeterinarianId = table.Column<Guid>(type: "uuid", nullable: false),
                    RanchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Scope = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Revision = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HerdAdvisories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HerdAdvisories_RanchRegistrations_RanchId",
                        column: x => x.RanchId,
                        principalSchema: "iam",
                        principalTable: "RanchRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HerdAdvisories_UserAccounts_VeterinarianId",
                        column: x => x.VeterinarianId,
                        principalSchema: "iam",
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProfessionalProfiles",
                schema: "iam",
                columns: table => new
                {
                    UserAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Specialty = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    YearsOfExperience = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionalProfiles", x => x.UserAccountId);
                    table.ForeignKey(
                        name: "FK_ProfessionalProfiles_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalSchema: "iam",
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RanchMemberships",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RanchId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsOwner = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RanchMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RanchMemberships_RanchRegistrations_RanchId",
                        column: x => x.RanchId,
                        principalSchema: "iam",
                        principalTable: "RanchRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RanchMemberships_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalSchema: "iam",
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessAttempts_UserAccountId_RecordedAt",
                schema: "iam",
                table: "AccessAttempts",
                columns: new[] { "UserAccountId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HerdAdvisories_RanchId",
                schema: "iam",
                table: "HerdAdvisories",
                column: "RanchId");

            migrationBuilder.CreateIndex(
                name: "IX_HerdAdvisories_VeterinarianId_RanchId",
                schema: "iam",
                table: "HerdAdvisories",
                columns: new[] { "VeterinarianId", "RanchId" },
                unique: true,
                filter: "\"Status\" IN ('Pending', 'Granted')");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalProfiles_LicenseNumber",
                schema: "iam",
                table: "ProfessionalProfiles",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RanchMemberships_RanchId",
                schema: "iam",
                table: "RanchMemberships",
                column: "RanchId");

            migrationBuilder.CreateIndex(
                name: "IX_RanchMemberships_UserAccountId_RanchId",
                schema: "iam",
                table: "RanchMemberships",
                columns: new[] { "UserAccountId", "RanchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_Email",
                schema: "iam",
                table: "UserAccounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_ExternalUid",
                schema: "iam",
                table: "UserAccounts",
                column: "ExternalUid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessAttempts",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "HerdAdvisories",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "ProfessionalProfiles",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "RanchMemberships",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "RanchRegistrations",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "UserAccounts",
                schema: "iam");
        }
    }
}
