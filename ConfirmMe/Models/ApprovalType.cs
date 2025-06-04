namespace ConfirmMe.Models
{
    public class ApprovalType
    {
        public int Id { get; set; }
        public string Name { get; set; } // Approval to PO, Approval Only, etc.
        public string Description { get; set; }

        public ICollection<ApprovalRequest> ApprovalRequests { get; set; }
    }
}
