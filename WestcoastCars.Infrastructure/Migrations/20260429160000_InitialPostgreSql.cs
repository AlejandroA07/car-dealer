using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WestcoastCars.Infrastructure.Data;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

[DbContext(typeof(WestcoastCarsContext))]
[Migration("20260429160000_InitialPostgreSql")]
public partial class InitialPostgreSql : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "FuelTypes",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_FuelTypes", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Manufacturers",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Manufacturers", x => x.Id));

        migrationBuilder.CreateTable(
            name: "OutboxMessages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Type = table.Column<string>(type: "text", nullable: false),
                Content = table.Column<string>(type: "text", nullable: false),
                ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Error = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_OutboxMessages", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ServiceBookings",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                VehicleRegistrationNumber = table.Column<string>(type: "text", nullable: false),
                ServiceType = table.Column<string>(type: "text", nullable: false),
                BookingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CustomerName = table.Column<string>(type: "text", nullable: false),
                CustomerEmail = table.Column<string>(type: "text", nullable: false),
                CustomerPhone = table.Column<string>(type: "text", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ServiceBookings", x => x.Id));

        migrationBuilder.CreateTable(
            name: "TransmissionTypes",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_TransmissionTypes", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Vehicles",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RegistrationNumber = table.Column<string>(type: "text", nullable: true),
                Model = table.Column<string>(type: "text", nullable: false),
                ModelYear = table.Column<string>(type: "text", nullable: false),
                Mileage = table.Column<int>(type: "integer", nullable: false),
                ImageUrl = table.Column<string>(type: "text", nullable: false),
                Value = table.Column<int>(type: "integer", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                IsSold = table.Column<bool>(type: "boolean", nullable: false),
                ExternalListingId = table.Column<string>(type: "text", nullable: true),
                Source = table.Column<string>(type: "text", nullable: true),
                SourceUrl = table.Column<string>(type: "text", nullable: true),
                PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ImportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Color = table.Column<string>(type: "text", nullable: true),
                City = table.Column<string>(type: "text", nullable: true),
                ManufacturerId = table.Column<int>(type: "integer", nullable: false),
                FuelTypeId = table.Column<int>(type: "integer", nullable: false),
                TransmissionTypeId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Vehicles", x => x.Id);
                table.ForeignKey("FK_Vehicles_FuelTypes_FuelTypeId", x => x.FuelTypeId, "FuelTypes", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_Vehicles_Manufacturers_ManufacturerId", x => x.ManufacturerId, "Manufacturers", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_Vehicles_TransmissionTypes_TransmissionTypeId", x => x.TransmissionTypeId, "TransmissionTypes", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_Vehicles_ExternalListingId", "Vehicles", "ExternalListingId");
        migrationBuilder.CreateIndex("IX_Vehicles_FuelTypeId", "Vehicles", "FuelTypeId");
        migrationBuilder.CreateIndex("IX_Vehicles_ManufacturerId", "Vehicles", "ManufacturerId");
        migrationBuilder.CreateIndex("IX_Vehicles_Source", "Vehicles", "Source");
        migrationBuilder.CreateIndex("IX_Vehicles_TransmissionTypeId", "Vehicles", "TransmissionTypeId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("OutboxMessages");
        migrationBuilder.DropTable("ServiceBookings");
        migrationBuilder.DropTable("Vehicles");
        migrationBuilder.DropTable("FuelTypes");
        migrationBuilder.DropTable("Manufacturers");
        migrationBuilder.DropTable("TransmissionTypes");
    }
}
