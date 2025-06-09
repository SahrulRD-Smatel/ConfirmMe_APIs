using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConfirmMe.Migrations
{
    /// <inheritdoc />
    public partial class QRApproveFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ApprovalFlows");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "ApprovalFlows",
                newName: "Remark");

            migrationBuilder.AddColumn<DateTime>(
                name: "QrTokenGeneratedAt",
                table: "ApprovalFlows",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QrTokenGeneratedAt",
                table: "ApprovalFlows");

            migrationBuilder.RenameColumn(
                name: "Remark",
                table: "ApprovalFlows",
                newName: "Notes");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ApprovalFlows",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
