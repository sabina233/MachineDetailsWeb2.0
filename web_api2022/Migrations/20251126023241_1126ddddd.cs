using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_api2022.Migrations
{
    /// <inheritdoc />
    public partial class _1126ddddd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 给已存在的 ZPL_DataLatest 表添加 MaterialNumber 列
            migrationBuilder.AddColumn<string>(
                name: "MaterialNumber",
                table: "ZPL_DataLatest",
                type: "nvarchar(max)",
                nullable: true);

            // 给已存在的 ZPL_DataRecords 表添加 MaterialNumber 列
            migrationBuilder.AddColumn<string>(
                name: "MaterialNumber",
                table: "ZPL_DataRecords",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 回滚：移除 ZPL_DataLatest 表的 MaterialNumber 列
            migrationBuilder.DropColumn(
                name: "MaterialNumber",
                table: "ZPL_DataLatest");

            // 回滚：移除 ZPL_DataRecords 表的 MaterialNumber 列
            migrationBuilder.DropColumn(
                name: "MaterialNumber",
                table: "ZPL_DataRecords");
        }
    }
}