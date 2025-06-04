namespace ConfirmMe.Models
{
    public class PrintHistory
    {
        public int Id { get; set; }
        public int ApprovalRequestId { get; set; }
        public int PrintedBy { get; set; }
        public DateTime PrintedAt { get; set; }

        // Relasi dengan ApprovalRequest
        public ApprovalRequest ApprovalRequest { get; set; }
    }

}
