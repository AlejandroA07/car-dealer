using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WestcoastCars.Infrastructure.Data;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

[DbContext(typeof(WestcoastCarsContext))]
[Migration("20260516000001_AddVehicleBodyTypeDoorsEngineVolume")]
public partial class AddVehicleBodyTypeDoorsEngineVolume : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "BodyType",
            table: "Vehicles",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Doors",
            table: "Vehicles",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "EngineVolume",
            table: "Vehicles",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "BodyType", table: "Vehicles");
        migrationBuilder.DropColumn(name: "Doors", table: "Vehicles");
        migrationBuilder.DropColumn(name: "EngineVolume", table: "Vehicles");
    }
}
