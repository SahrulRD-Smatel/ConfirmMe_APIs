using System.ComponentModel.DataAnnotations.Schema;

namespace ConfirmMe.Models
{
    public class Attachment
    {
        public int Id { get; set; }

        
        public string FileName { get; set; }

        public string FilePath { get; set; } // e.g. uploads/REQ20250624-101/ttd.png


        public string ContentType { get; set; }      // misal "application/pdf", "image/png"
        public DateTime UploadedAt { get; set; }

        public int ApprovalRequestId { get; set; }

        public ApprovalRequest ApprovalRequest { get; set; }
    }
}
