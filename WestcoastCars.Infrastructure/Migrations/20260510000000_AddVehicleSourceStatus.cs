using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WestcoastCars.Infrastructure.Data;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

[DbContext(typeof(WestcoastCarsContext))]
[Migration("20260510000000_AddVehicleSourceStatus")]
public partial class AddVehicleSourceStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SourceStatus",
            table: "Vehicles",
            type: "character varying(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "Active");

        migrationBuilder.AddColumn<DateTime>(
            name: "SourceRemovedAt",
            table: "Vehicles",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Vehicles_SourceStatus",
            table: "Vehicles",
            column: "SourceStatus");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Vehicles_SourceStatus",
            table: "Vehicles");

        migrationBuilder.DropColumn(
            name: "SourceStatus",
            table: "Vehicles");

        migrationBuilder.DropColumn(
            name: "SourceRemovedAt",
            table: "Vehicles");
    }
}
