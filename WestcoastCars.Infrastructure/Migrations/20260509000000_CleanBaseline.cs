using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WestcoastCars.Infrastructure.Data;

#nullable disable

namespace WestcoastCars.Infrastructure.Migrations;

[DbContext(typeof(WestcoastCarsContext))]
[Migration("20260509000000_CleanBaseline")]
public partial class CleanBaseline : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS citext;");
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_AspNetRoles", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: true),
                SecurityStamp = table.Column<string>(type: "text", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                PhoneNumber = table.Column<string>(type: "text", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_AspNetUsers", x => x.Id));

        migrationBuilder.CreateTable(
            name: "FuelTypes",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                Name = table.Column<string>(type: "citext", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_FuelTypes", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Manufacturers",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Manufacturers", x => x.Id));

        migrationBuilder.CreateTable(
            name: "TransmissionTypes",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                Name = table.Column<string>(type: "citext", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_TransmissionTypes", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RoleId = table.Column<string>(type: "text", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", x => x.RoleId, "AspNetRoles", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "text", nullable: false),
                ProviderKey = table.Column<string>(type: "text", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                UserId = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "text", nullable: false),
                RoleId = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", x => x.RoleId, "AspNetRoles", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "text", nullable: false),
                LoginProvider = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Vehicles",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                RegistrationNumber = table.Column<string>(type: "citext", nullable: false),
                Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                ModelYear = table.Column<int>(type: "integer", nullable: false),
                Mileage = table.Column<int>(type: "integer", nullable: false),
                ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Price = table.Column<int>(type: "integer", nullable: false),
                Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                IsSold = table.Column<bool>(type: "boolean", nullable: false),
                ExternalListingId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                SourceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ImportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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

        migrationBuilder.CreateTable(
            name: "ServiceBookings",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                VehicleId = table.Column<int>(type: "integer", nullable: true),
                VehicleRegistrationNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                ServiceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                BookingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                CustomerEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                CustomerPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ServiceBookings", x => x.Id);
                table.ForeignKey("FK_ServiceBookings_Vehicles_VehicleId", x => x.VehicleId, "Vehicles", "Id", onDelete: ReferentialAction.SetNull);
            });

        // Check constraints for citext columns (length enforced here; citext does not support varchar(N))
        migrationBuilder.Sql("""ALTER TABLE "FuelTypes" ADD CONSTRAINT "CK_FuelTypes_Name_Length" CHECK (length("Name") <= 50);""");
        migrationBuilder.Sql("""ALTER TABLE "TransmissionTypes" ADD CONSTRAINT "CK_TransmissionTypes_Name_Length" CHECK (length("Name") <= 50);""");
        migrationBuilder.Sql("""ALTER TABLE "Vehicles" ADD CONSTRAINT "CK_Vehicles_RegistrationNumber_Length" CHECK (length("RegistrationNumber") <= 10);""");

        // Identity indexes
        migrationBuilder.CreateIndex("IX_AspNetRoleClaims_RoleId", "AspNetRoleClaims", "RoleId");
        migrationBuilder.CreateIndex("RoleNameIndex", "AspNetRoles", "NormalizedName", unique: true);
        migrationBuilder.CreateIndex("IX_AspNetUserClaims_UserId", "AspNetUserClaims", "UserId");
        migrationBuilder.CreateIndex("IX_AspNetUserLogins_UserId", "AspNetUserLogins", "UserId");
        migrationBuilder.CreateIndex("IX_AspNetUserRoles_RoleId", "AspNetUserRoles", "RoleId");
        migrationBuilder.CreateIndex("EmailIndex", "AspNetUsers", "NormalizedEmail");
        migrationBuilder.CreateIndex("UserNameIndex", "AspNetUsers", "NormalizedUserName", unique: true);

        // Domain indexes
        migrationBuilder.CreateIndex("IX_FuelTypes_Name", "FuelTypes", "Name", unique: true);
        migrationBuilder.CreateIndex("IX_TransmissionTypes_Name", "TransmissionTypes", "Name", unique: true);
        migrationBuilder.CreateIndex("IX_Vehicles_ExternalListingId", "Vehicles", "ExternalListingId");
        migrationBuilder.CreateIndex("IX_Vehicles_FuelTypeId", "Vehicles", "FuelTypeId");
        migrationBuilder.CreateIndex("IX_Vehicles_IsSold", "Vehicles", "IsSold");
        migrationBuilder.CreateIndex("IX_Vehicles_ManufacturerId", "Vehicles", "ManufacturerId");
        migrationBuilder.CreateIndex("IX_Vehicles_RegistrationNumber", "Vehicles", "RegistrationNumber", unique: true);
        migrationBuilder.CreateIndex("IX_Vehicles_Source", "Vehicles", "Source");
        migrationBuilder.CreateIndex("IX_Vehicles_TransmissionTypeId", "Vehicles", "TransmissionTypeId");
        migrationBuilder.CreateIndex("IX_ServiceBookings_VehicleId", "ServiceBookings", "VehicleId");

        migrationBuilder.Sql("""CREATE INDEX "IX_Vehicles_Model" ON "Vehicles" USING GIN ("Model" gin_trgm_ops);""");
        migrationBuilder.Sql("""CREATE UNIQUE INDEX "IX_Manufacturers_Name" ON "Manufacturers" (lower("Name"));""");
        migrationBuilder.Sql("""CREATE INDEX "IX_Manufacturers_Name_Trgm" ON "Manufacturers" USING GIN ("Name" gin_trgm_ops);""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ServiceBookings");
        migrationBuilder.DropTable("Vehicles");
        migrationBuilder.DropTable("AspNetRoleClaims");
        migrationBuilder.DropTable("AspNetUserClaims");
        migrationBuilder.DropTable("AspNetUserLogins");
        migrationBuilder.DropTable("AspNetUserRoles");
        migrationBuilder.DropTable("AspNetUserTokens");
        migrationBuilder.DropTable("FuelTypes");
        migrationBuilder.DropTable("Manufacturers");
        migrationBuilder.DropTable("TransmissionTypes");
        migrationBuilder.DropTable("AspNetRoles");
        migrationBuilder.DropTable("AspNetUsers");
    }
}
