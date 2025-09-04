using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeTransmissionNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_TransmissionTypes_TransmissionsTypeId",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "TransmissionsTypeId",
                table: "Vehicles",
                newName: "TransmissionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_TransmissionsTypeId",
                table: "Vehicles",
                newName: "IX_Vehicles_TransmissionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_TransmissionTypes_TransmissionTypeId",
                table: "Vehicles",
                column: "TransmissionTypeId",
                principalTable: "TransmissionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_TransmissionTypes_TransmissionTypeId",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "TransmissionTypeId",
                table: "Vehicles",
                newName: "TransmissionsTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_TransmissionTypeId",
                table: "Vehicles",
                newName: "IX_Vehicles_TransmissionsTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_TransmissionTypes_TransmissionsTypeId",
                table: "Vehicles",
                column: "TransmissionsTypeId",
                principalTable: "TransmissionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
