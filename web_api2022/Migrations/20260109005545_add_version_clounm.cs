using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_api2022.Migrations
{
    /// <inheritdoc />
    public partial class add_version_clounm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "ZPL_DataRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "ZPL_DataLatest",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "ZPL_DataRecords");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ZPL_DataLatest");
        }
    }
}
