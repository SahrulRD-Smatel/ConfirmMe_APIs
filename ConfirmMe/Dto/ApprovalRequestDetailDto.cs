namespace ConfirmMe.Dto
{
    public class ApprovalRequestDetailDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ApprovalType { get; set; }

        public List<ApprovalFlowDto> ApprovalFlows { get; set; }
    }

    public class ApprovalFlowDto
    {
        public string PositionTitle { get; set; }
        public string ApproverName { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Remark { get; set; }
    }

}
