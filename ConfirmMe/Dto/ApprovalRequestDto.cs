namespace ConfirmMe.Dto
{
    public class ApprovalRequestDto
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CurrentStatus { get; set; }
        public string? Barcode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public UserDto RequestedByUser { get; set; }
        public ApprovalTypeStatDto ApprovalType { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
        public List<ApprovalFlowDto> ApprovalFlows { get; set; }
    }
}
