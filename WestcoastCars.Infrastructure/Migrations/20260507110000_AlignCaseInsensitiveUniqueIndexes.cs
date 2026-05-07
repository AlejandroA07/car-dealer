using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WestcoastCars.Infrastructure.Data;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

[DbContext(typeof(WestcoastCarsContext))]
[Migration("20260507110000_AlignCaseInsensitiveUniqueIndexes")]
public partial class AlignCaseInsensitiveUniqueIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""CREATE EXTENSION IF NOT EXISTS citext;""");

        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Vehicles_RegistrationNumber";""");
        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_FuelTypes_Name";""");
        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_TransmissionTypes_Name";""");
        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Manufacturers_Name";""");

        migrationBuilder.AlterColumn<string>(
            name: "RegistrationNumber",
            table: "Vehicles",
            type: "citext",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "FuelTypes",
            type: "citext",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "TransmissionTypes",
            type: "citext",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.CreateIndex(
            name: "IX_Vehicles_RegistrationNumber",
            table: "Vehicles",
            column: "RegistrationNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_FuelTypes_Name",
            table: "FuelTypes",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TransmissionTypes_Name",
            table: "TransmissionTypes",
            column: "Name",
            unique: true);

        migrationBuilder.Sql(
            """CREATE UNIQUE INDEX "IX_Manufacturers_Name" ON "Manufacturers" (lower("Name"));""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Vehicles_RegistrationNumber",
            table: "Vehicles");

        migrationBuilder.DropIndex(
            name: "IX_FuelTypes_Name",
            table: "FuelTypes");

        migrationBuilder.DropIndex(
            name: "IX_TransmissionTypes_Name",
            table: "TransmissionTypes");

        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Manufacturers_Name";""");

        migrationBuilder.AlterColumn<string>(
            name: "RegistrationNumber",
            table: "Vehicles",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "citext");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "FuelTypes",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "citext");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "TransmissionTypes",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "citext");

        migrationBuilder.Sql(
            """CREATE UNIQUE INDEX "IX_Vehicles_RegistrationNumber" ON "Vehicles" (lower("RegistrationNumber"));""");

        migrationBuilder.Sql(
            """CREATE UNIQUE INDEX "IX_FuelTypes_Name" ON "FuelTypes" (lower("Name"));""");

        migrationBuilder.Sql(
            """CREATE UNIQUE INDEX "IX_TransmissionTypes_Name" ON "TransmissionTypes" (lower("Name"));""");

        migrationBuilder.Sql(
            """CREATE UNIQUE INDEX "IX_Manufacturers_Name" ON "Manufacturers" (lower("Name"));""");
    }
}
