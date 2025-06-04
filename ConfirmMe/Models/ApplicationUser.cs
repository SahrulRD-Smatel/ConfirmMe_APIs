using Microsoft.AspNetCore.Identity;

namespace ConfirmMe.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public int PositionId { get; set; }
        public string Role { get; set; } // Staff, HRD, Manager, Direktur
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public Position Position { get; set; }
        public ICollection<ApprovalRequest> ApprovalRequests { get; set; }
    }
}
