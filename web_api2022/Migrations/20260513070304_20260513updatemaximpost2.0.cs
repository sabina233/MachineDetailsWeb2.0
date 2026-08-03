using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_api2022.Migrations
{
    /// <inheritdoc />
    public partial class _20260513updatemaximpost20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaximPostPath",
                table: "UserAccounts",
                newName: "ProductionMode");

            migrationBuilder.AddColumn<string>(
                name: "Api",
                table: "UserAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BrandIdDefault",
                table: "UserAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BrandIds",
                table: "UserAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BrandNames",
                table: "UserAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrintShopId",
                table: "UserAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Api",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "BrandIdDefault",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "BrandIds",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "BrandNames",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "PrintShopId",
                table: "UserAccounts");

            migrationBuilder.RenameColumn(
                name: "ProductionMode",
                table: "UserAccounts",
                newName: "MaximPostPath");
        }
    }
}
