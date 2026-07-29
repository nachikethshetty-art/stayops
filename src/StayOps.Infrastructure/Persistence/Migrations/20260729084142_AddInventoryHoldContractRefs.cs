using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryHoldContractRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "InventoryHolds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TravelAgentId",
                table: "InventoryHolds",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "InventoryHolds");

            migrationBuilder.DropColumn(
                name: "TravelAgentId",
                table: "InventoryHolds");
        }
    }
}
