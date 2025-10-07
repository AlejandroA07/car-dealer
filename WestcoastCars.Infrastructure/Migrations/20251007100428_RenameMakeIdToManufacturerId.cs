using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameMakeIdToManufacturerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Manufacturers_MakeId",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "MakeId",
                table: "Vehicles",
                newName: "ManufacturerId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_MakeId",
                table: "Vehicles",
                newName: "IX_Vehicles_ManufacturerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Manufacturers_ManufacturerId",
                table: "Vehicles",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Manufacturers_ManufacturerId",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "ManufacturerId",
                table: "Vehicles",
                newName: "MakeId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_ManufacturerId",
                table: "Vehicles",
                newName: "IX_Vehicles_MakeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Manufacturers_MakeId",
                table: "Vehicles",
                column: "MakeId",
                principalTable: "Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
