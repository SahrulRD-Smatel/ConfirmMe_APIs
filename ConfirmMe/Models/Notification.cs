using System;

namespace ConfirmMe.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string RecipientId { get; set; } // Ubah dari int ke string
        public ApplicationUser Recipient { get; set; } // Navigasi ke ApplicationUser

        public string Message { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UrgencyLevel { get; set; }
        public int? RelatedEntityId { get; set; }
        public string RelatedEntityType { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ActionUrl { get; set; }
    }
}
