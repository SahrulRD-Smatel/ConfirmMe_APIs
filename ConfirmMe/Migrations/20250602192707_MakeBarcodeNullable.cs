using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConfirmMe.Migrations
{
    /// <inheritdoc />
    public partial class MakeBarcodeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Attachments",
                newName: "ContentType");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                table: "Attachments",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "ApprovalRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "Attachments",
                newName: "FilePath");

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "ApprovalRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
