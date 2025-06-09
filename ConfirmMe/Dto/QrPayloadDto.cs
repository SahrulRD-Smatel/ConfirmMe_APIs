namespace ConfirmMe.Dto
{
    public class QrPayloadDto
    {
        public int ApprovalRequestId { get; set; }
        public int FlowId { get; set; }
        public string Action { get; set; } = "Approve"; // Default Approve
        public string GeneratedBy { get; set; } = "";
        public string QrToken { get; set; } = "";
        public string Remark { get; set; } = "";
    }
}
