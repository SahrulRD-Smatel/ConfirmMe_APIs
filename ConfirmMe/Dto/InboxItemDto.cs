namespace ConfirmMe.Dto
{
    public class InboxItemDto
    {
        public int ApprovalRequestId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RequestedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public string Status { get; set; }

        //tambahan
        public string ApprovalTypeName { get; set; }
    }
}
