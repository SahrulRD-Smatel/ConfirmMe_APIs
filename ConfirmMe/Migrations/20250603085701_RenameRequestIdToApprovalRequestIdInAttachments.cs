using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConfirmMe.Migrations
{
    /// <inheritdoc />
    public partial class RenameRequestIdToApprovalRequestIdInAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalFlows_ApprovalRequests_RequestId",
                table: "ApprovalFlows");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "PrintHistories");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "ApprovalFlows",
                newName: "ApprovalRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalFlows_RequestId",
                table: "ApprovalFlows",
                newName: "IX_ApprovalFlows_ApprovalRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalFlows_ApprovalRequests_ApprovalRequestId",
                table: "ApprovalFlows",
                column: "ApprovalRequestId",
                principalTable: "ApprovalRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalFlows_ApprovalRequests_ApprovalRequestId",
                table: "ApprovalFlows");

            migrationBuilder.RenameColumn(
                name: "ApprovalRequestId",
                table: "ApprovalFlows",
                newName: "RequestId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalFlows_ApprovalRequestId",
                table: "ApprovalFlows",
                newName: "IX_ApprovalFlows_RequestId");

            migrationBuilder.AddColumn<int>(
                name: "RequestId",
                table: "PrintHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequestId",
                table: "Attachments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalFlows_ApprovalRequests_RequestId",
                table: "ApprovalFlows",
                column: "RequestId",
                principalTable: "ApprovalRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
