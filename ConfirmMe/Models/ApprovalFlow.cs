namespace ConfirmMe.Models
{
    public class ApprovalFlow
    {
        public int Id { get; set; }
        public int ApprovalRequestId { get; set; }
        public string ApproverId { get; set; }
        public int PositionId { get; set; }
        public string Status { get; set; } 
        public int OrderIndex { get; set; }
        public string Remark { get; set; } = "";
        public DateTime? ApprovedAt { get; set; }

        //QR exp & udh dipake
        // ✅ Token QR untuk approval
        public string QrToken { get; set; }
        public DateTime? QrTokenGeneratedAt { get; set; } 
        public bool IsQrUsed { get; set; } = false;
        public DateTime? QrUsedAt { get; set; }

        // Navigation
        public ApprovalRequest ApprovalRequest { get; set; }
        public ApplicationUser Approver { get; set; }
        public Position Position { get; set; }
    }
}
