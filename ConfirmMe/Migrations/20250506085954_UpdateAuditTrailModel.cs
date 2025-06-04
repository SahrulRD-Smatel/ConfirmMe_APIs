using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConfirmMe.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditTrailModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActionType",
                table: "AuditTrails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ApproverId",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChangeDescription",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AuditTrails",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "ApproverId",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "ChangeDescription",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AuditTrails");
        }
    }
}
