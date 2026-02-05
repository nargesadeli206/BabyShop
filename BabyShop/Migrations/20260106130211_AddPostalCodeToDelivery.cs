using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabyShop.Migrations
{
    /// <inheritdoc />
    public partial class AddPostalCodeToDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Deliveries");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Inventories",
                newName: "Count");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Deliveries");

            migrationBuilder.RenameColumn(
                name: "Count",
                table: "Inventories",
                newName: "Quantity");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Deliveries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
