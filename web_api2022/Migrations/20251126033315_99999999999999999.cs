using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_api2022.Migrations
{
    /// <inheritdoc />
    public partial class _99999999999999999 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LocalTime",
                table: "ZPL_DataRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LocalTime",
                table: "ZPL_DataLatest",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalTime",
                table: "ZPL_DataRecords");

            migrationBuilder.DropColumn(
                name: "LocalTime",
                table: "ZPL_DataLatest");
        }
    }
}
