using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBlocketImportFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "Vehicles",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Vehicles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Vehicles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ExternalListingId",
                table: "Vehicles",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ImportedAt",
                table: "Vehicles",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Vehicles",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Vehicles",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "Vehicles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Error = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ExternalListingId",
                table: "Vehicles",
                column: "ExternalListingId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Source",
                table: "Vehicles",
                column: "Source");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_ExternalListingId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Source",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ExternalListingId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ImportedAt",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "Vehicles");

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "RegistrationNumber",
                keyValue: null,
                column: "RegistrationNumber",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "Vehicles",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
