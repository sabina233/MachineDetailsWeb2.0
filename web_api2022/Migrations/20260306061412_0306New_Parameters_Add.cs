using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_api2022.Migrations
{
    /// <inheritdoc />
    public partial class _0306New_Parameters_Add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ERPNo",
                table: "ZPL_DataRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Layoutdesign",
                table: "ZPL_DataRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaximNo",
                table: "ZPL_DataRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "ZPL_DataRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ERPNo",
                table: "ZPL_DataLatest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Layoutdesign",
                table: "ZPL_DataLatest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaximNo",
                table: "ZPL_DataLatest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "ZPL_DataLatest",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ERPNo",
                table: "ZPL_DataRecords");

            migrationBuilder.DropColumn(
                name: "Layoutdesign",
                table: "ZPL_DataRecords");

            migrationBuilder.DropColumn(
                name: "MaximNo",
                table: "ZPL_DataRecords");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ZPL_DataRecords");

            migrationBuilder.DropColumn(
                name: "ERPNo",
                table: "ZPL_DataLatest");

            migrationBuilder.DropColumn(
                name: "Layoutdesign",
                table: "ZPL_DataLatest");

            migrationBuilder.DropColumn(
                name: "MaximNo",
                table: "ZPL_DataLatest");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ZPL_DataLatest");
        }
    }
}
