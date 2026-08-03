using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_api2022.Migrations
{
    /// <inheritdoc />
    public partial class jnn666 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComputerName",
                table: "ZPL_DataRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "ZPL_DataRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "ZPL_DataRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MacAddress",
                table: "ZPL_DataRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComputerName",
                table: "ZPL_DataLatest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "ZPL_DataLatest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "ZPL_DataLatest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MacAddress",
                table: "ZPL_DataLatest",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComputerName",
                table: "ZPL_DataRecords");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "ZPL_DataRecords");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "ZPL_DataRecords");

            migrationBuilder.DropColumn(
                name: "MacAddress",
                table: "ZPL_DataRecords");

            migrationBuilder.DropColumn(
                name: "ComputerName",
                table: "ZPL_DataLatest");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "ZPL_DataLatest");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "ZPL_DataLatest");

            migrationBuilder.DropColumn(
                name: "MacAddress",
                table: "ZPL_DataLatest");
        }
    }
}
