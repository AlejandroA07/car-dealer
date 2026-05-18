using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WestcoastCars.Infrastructure.Data;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

[DbContext(typeof(WestcoastCarsContext))]
[Migration("20260516000000_AddVehicleWheelDriveAndHorsepower")]
public partial class AddVehicleWheelDriveAndHorsepower : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "WheelDrive",
            table: "Vehicles",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Horsepower",
            table: "Vehicles",
            type: "integer",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "WheelDrive",
            table: "Vehicles");

        migrationBuilder.DropColumn(
            name: "Horsepower",
            table: "Vehicles");
    }
}
