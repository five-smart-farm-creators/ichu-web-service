using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiveSmartFarmCreators.Ichu.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddRanchAccessCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing ranches have no code yet, so the column starts nullable and becomes required after the backfill.
            migrationBuilder.AddColumn<string>(
                name: "AccessCode",
                schema: "iam",
                table: "RanchRegistrations",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            // The alphabet and length must match RanchAccessCode.Alphabet and RanchAccessCode.Length.
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE
                    ranch RECORD;
                    candidate text;
                    alphabet constant text := 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
                BEGIN
                    FOR ranch IN SELECT "Id" FROM iam."RanchRegistrations" WHERE "AccessCode" IS NULL LOOP
                        LOOP
                            SELECT string_agg(substr(alphabet, 1 + floor(random() * 32)::int, 1), '')
                              INTO candidate FROM generate_series(1, 8);
                            EXIT WHEN NOT EXISTS (SELECT 1 FROM iam."RanchRegistrations" WHERE "AccessCode" = candidate);
                        END LOOP;
                        UPDATE iam."RanchRegistrations" SET "AccessCode" = candidate WHERE "Id" = ranch."Id";
                    END LOOP;
                END $$;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "AccessCode",
                schema: "iam",
                table: "RanchRegistrations",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RanchRegistrations_AccessCode",
                schema: "iam",
                table: "RanchRegistrations",
                column: "AccessCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RanchRegistrations_AccessCode",
                schema: "iam",
                table: "RanchRegistrations");

            migrationBuilder.DropColumn(
                name: "AccessCode",
                schema: "iam",
                table: "RanchRegistrations");
        }
    }
}
