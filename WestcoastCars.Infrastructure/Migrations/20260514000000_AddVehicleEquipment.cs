using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WestcoastCars.Infrastructure.Data;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

[DbContext(typeof(WestcoastCarsContext))]
[Migration("20260514000000_AddVehicleEquipment")]
public partial class AddVehicleEquipment : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Equipment",
            table: "Vehicles",
            type: "text",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Equipment",
            table: "Vehicles");
    }
}
