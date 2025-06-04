namespace ConfirmMe.Dto
{
    public class DashboardSummaryDto
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Completed { get; set; }
    }
}
