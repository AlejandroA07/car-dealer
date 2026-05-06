using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

[Migration("20260506000000_FixDomainEntityConsistency")]
public partial class FixDomainEntityConsistency : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""UPDATE "Vehicles" SET "RegistrationNumber" = '' WHERE "RegistrationNumber" IS NULL;""");

        migrationBuilder.AlterColumn<string>(
            name: "RegistrationNumber",
            table: "Vehicles",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedAt",
            table: "ServiceBookings",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "NOW()",
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "RegistrationNumber",
            table: "Vehicles",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedAt",
            table: "ServiceBookings",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone",
            oldDefaultValueSql: "NOW()");
    }
}
