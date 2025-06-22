namespace ConfirmMe.Dto
{
    public class ApproveRequestDto
    {
        public string ApproverId { get; set; }
        public string Status { get; set; } // "Approved" atau "Rejected"

        //tambahan
        public string? Remark { get; set; }
    }
}
