using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddVehiclesActivePartialIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Vehicles_Active",
            table: "Vehicles",
            column: "Id",
            filter: "\"SourceStatus\" != 'SourceRemoved'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Vehicles_Active",
            table: "Vehicles");
    }
}
