using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConfirmMe.Migrations
{
    /// <inheritdoc />
    public partial class AddQRCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "LetterPdf",
                table: "ApprovalRequests",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ApprovalFlows",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsQrUsed",
                table: "ApprovalFlows",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "QrUsedAt",
                table: "ApprovalFlows",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LetterPdf",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ApprovalFlows");

            migrationBuilder.DropColumn(
                name: "IsQrUsed",
                table: "ApprovalFlows");

            migrationBuilder.DropColumn(
                name: "QrUsedAt",
                table: "ApprovalFlows");
        }
    }
}
