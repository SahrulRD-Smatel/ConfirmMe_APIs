namespace ConfirmMe.Dto
{
    public class QrApprovalDto
    {
        public int ApprovalRequestId { get; set; }
        public int ApproverId { get; set; }
        public int FlowId { get; set; } // Tambahan: untuk referensi approval step tertentu
        public string Action { get; set; } // "Approved" atau "Rejected"
        public DateTime IssuedAt { get; set; }
        public string QrToken { get; set; }
        public string Remark { get; set; } // Tambahan: agar bisa isi catatan saat QR approval

    }

}
