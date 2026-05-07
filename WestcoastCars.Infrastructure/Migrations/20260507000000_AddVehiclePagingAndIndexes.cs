using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WestcoastCars.Infrastructure.Data;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

[DbContext(typeof(WestcoastCarsContext))]
[Migration("20260507000000_AddVehiclePagingAndIndexes")]
public partial class AddVehiclePagingAndIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""CREATE EXTENSION IF NOT EXISTS pg_trgm;""");

        migrationBuilder.CreateIndex(
            name: "IX_Vehicles_IsSold",
            table: "Vehicles",
            column: "IsSold");

        migrationBuilder.Sql(
            """CREATE INDEX "IX_Vehicles_Model" ON "Vehicles" USING GIN ("Model" gin_trgm_ops);""");

        migrationBuilder.Sql(
            """CREATE UNIQUE INDEX "IX_Vehicles_RegistrationNumber" ON "Vehicles" (lower("RegistrationNumber"));""");

        migrationBuilder.Sql(
            """CREATE UNIQUE INDEX "IX_Manufacturers_Name" ON "Manufacturers" (lower("Name"));""");

        migrationBuilder.Sql(
            """CREATE INDEX "IX_Manufacturers_Name_Trgm" ON "Manufacturers" USING GIN ("Name" gin_trgm_ops);""");

        migrationBuilder.Sql(
            """CREATE UNIQUE INDEX "IX_FuelTypes_Name" ON "FuelTypes" (lower("Name"));""");

        migrationBuilder.Sql(
            """CREATE UNIQUE INDEX "IX_TransmissionTypes_Name" ON "TransmissionTypes" (lower("Name"));""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Vehicles_IsSold",
            table: "Vehicles");

        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Vehicles_Model";""");
        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Vehicles_RegistrationNumber";""");
        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Manufacturers_Name";""");
        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Manufacturers_Name_Trgm";""");
        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_FuelTypes_Name";""");
        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_TransmissionTypes_Name";""");
    }
}
