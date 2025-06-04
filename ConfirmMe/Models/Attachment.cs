using System.ComponentModel.DataAnnotations.Schema;

namespace ConfirmMe.Models
{
    public class Attachment
    {
        public int Id { get; set; }

        
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }      // isi file disimpan di sini
        public string ContentType { get; set; }      // misal "application/pdf", "image/png"
        public DateTime UploadedAt { get; set; }

        public int ApprovalRequestId { get; set; }

        public ApprovalRequest ApprovalRequest { get; set; }
    }
}
