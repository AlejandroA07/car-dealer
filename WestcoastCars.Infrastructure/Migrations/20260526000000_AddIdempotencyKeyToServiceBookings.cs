using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WestcoastCars.Infrastructure.Data;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

[DbContext(typeof(WestcoastCarsContext))]
[Migration("20260526000000_AddIdempotencyKeyToServiceBookings")]
public partial class AddIdempotencyKeyToServiceBookings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "IdempotencyKey",
            table: "ServiceBookings",
            type: "character varying(36)",
            maxLength: 36,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ServiceBookings_IdempotencyKey",
            table: "ServiceBookings",
            column: "IdempotencyKey",
            unique: true,
            filter: "\"IdempotencyKey\" IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ServiceBookings_IdempotencyKey",
            table: "ServiceBookings");

        migrationBuilder.DropColumn(
            name: "IdempotencyKey",
            table: "ServiceBookings");
    }
}
