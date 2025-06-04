namespace ConfirmMe.Dto
{
    public class UpdateApprovalStatusDto
    {
        public int ApprovalRequestId { get; set; }
        public string NewStatus { get; set; }  // Status baru (misalnya "Approved")
        public string StaffEmail { get; set; }  // Email staff yang akan menerima pemberitahuan
    }
}
