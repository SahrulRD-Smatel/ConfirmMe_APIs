namespace ConfirmMe.Dto
{
    public class ApprovalTypeStatDto
    {
        public string ApprovalType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
