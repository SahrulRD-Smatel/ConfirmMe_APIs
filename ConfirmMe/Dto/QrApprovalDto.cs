namespace ConfirmMe.Dto
{
    public class QrApprovalDto
    {
        public int ApprovalRequestId { get; set; }
        public int ApproverId { get; set; }
        public string Action { get; set; } // "Approved" atau "Rejected"
        public DateTime IssuedAt { get; set; }
    }

}
