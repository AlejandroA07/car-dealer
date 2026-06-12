using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WestcoastCars.Infrastructure.Data;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

[DbContext(typeof(WestcoastCarsContext))]
[Migration("20260519000000_AddTimeSlotToServiceBookings")]
public partial class AddTimeSlotToServiceBookings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "TimeSlot",
            table: "ServiceBookings",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TimeSlot",
            table: "ServiceBookings");
    }
}
