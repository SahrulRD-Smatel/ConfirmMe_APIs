namespace ConfirmMe.Models
{
    public class EmailErrorLog
    {
        public int Id { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
