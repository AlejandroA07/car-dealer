using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddRegistrationNumberNullableAndNewFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "RegistrationNumber",
            table: "Vehicles",
            type: "citext",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "citext",
            oldNullable: false,
            oldDefaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Address",
            table: "Vehicles",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Seats",
            table: "Vehicles",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "MaxTrailerWeight",
            table: "Vehicles",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "OwnerCount",
            table: "Vehicles",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<DateOnly>(
            name: "LastInspectionDate",
            table: "Vehicles",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<DateOnly>(
            name: "NextInspectionDate",
            table: "Vehicles",
            type: "date",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Address", table: "Vehicles");
        migrationBuilder.DropColumn(name: "Seats", table: "Vehicles");
        migrationBuilder.DropColumn(name: "MaxTrailerWeight", table: "Vehicles");
        migrationBuilder.DropColumn(name: "OwnerCount", table: "Vehicles");
        migrationBuilder.DropColumn(name: "LastInspectionDate", table: "Vehicles");
        migrationBuilder.DropColumn(name: "NextInspectionDate", table: "Vehicles");

        migrationBuilder.AlterColumn<string>(
            name: "RegistrationNumber",
            table: "Vehicles",
            type: "citext",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "citext",
            oldNullable: true);
    }
}
