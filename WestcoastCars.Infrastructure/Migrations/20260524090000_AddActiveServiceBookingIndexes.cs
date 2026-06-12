using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

public partial class AddActiveServiceBookingIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "VehicleRegistrationNumber",
            table: "ServiceBookings",
            type: "citext",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(10)",
            oldMaxLength: 10);

        migrationBuilder.CreateIndex(
            name: "IX_ServiceBookings_ActiveSlot",
            table: "ServiceBookings",
            columns: ["BookingDate", "TimeSlot"],
            unique: true,
            filter: "\"Status\" NOT IN (2, 3)");

        migrationBuilder.CreateIndex(
            name: "IX_ServiceBookings_ActiveRegistrationNumber",
            table: "ServiceBookings",
            column: "VehicleRegistrationNumber",
            unique: true,
            filter: "\"Status\" NOT IN (2, 3)");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ServiceBookings_ActiveSlot",
            table: "ServiceBookings");

        migrationBuilder.DropIndex(
            name: "IX_ServiceBookings_ActiveRegistrationNumber",
            table: "ServiceBookings");

        migrationBuilder.AlterColumn<string>(
            name: "VehicleRegistrationNumber",
            table: "ServiceBookings",
            type: "character varying(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "citext",
            oldMaxLength: 10);
    }
}
