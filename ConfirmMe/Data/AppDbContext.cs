using ConfirmMe.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConfirmMe.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Position> Positions { get; set; }
        public DbSet<ApprovalType> ApprovalTypes { get; set; }
        public DbSet<ApprovalRequest> ApprovalRequests { get; set; }
        public DbSet<ApprovalFlow> ApprovalFlows { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Attachment> Attachments { get; set; }  // Added Attachments
        public DbSet<PrintHistory> PrintHistories { get; set; }  // Added PrintHistories
        public DbSet<EmailErrorLog> EmailErrorLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);  // Ensure identity-related configurations are applied.

            // User -> Position (ApplicationUser to Position)
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Position)  // User has one Position
                .WithMany(p => p.Users)   // Position has many Users
                .HasForeignKey(u => u.PositionId)  // Foreign Key is PositionId in ApplicationUser
                .OnDelete(DeleteBehavior.Restrict); // If a Position is deleted, prevent deletion of related users

            // ApprovalRequest -> RequestedByUser (ApplicationUser)
            modelBuilder.Entity<ApprovalRequest>()
                .HasOne(ar => ar.RequestedByUser)  // ApprovalRequest has one RequestedByUser (ApplicationUser)
                .WithMany(u => u.ApprovalRequests) // ApplicationUser has many ApprovalRequests
                .HasForeignKey(ar => ar.RequestedById)  // Foreign Key in ApprovalRequest
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of a user who has ApprovalRequests

            // ApprovalRequest -> ApprovalType
            modelBuilder.Entity<ApprovalRequest>()
                .HasOne(ar => ar.ApprovalType)  // ApprovalRequest has one ApprovalType
                .WithMany(at => at.ApprovalRequests) // ApprovalType has many ApprovalRequests
                .HasForeignKey(ar => ar.ApprovalTypeId)  // Foreign Key in ApprovalRequest
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of ApprovalType if it's in use

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.ApprovalRequest)
                .WithMany(ar => ar.Attachments)
                .HasForeignKey(a => a.ApprovalRequestId) // sesuai nama FK di Attachment
                .OnDelete(DeleteBehavior.Cascade); // jika ApprovalRequest dihapus, attachment ikut terhapus


            // ApprovalFlow -> ApprovalRequest
            modelBuilder.Entity<ApprovalFlow>()
                .HasOne(af => af.ApprovalRequest)  // ApprovalFlow has one ApprovalRequest
                .WithMany(ar => ar.ApprovalFlows)  // ApprovalRequest has many ApprovalFlows
                .HasForeignKey(af => af.ApprovalRequestId)  // Foreign Key in ApprovalFlow
                .OnDelete(DeleteBehavior.Cascade);  // Cascade delete to remove related ApprovalFlows if an ApprovalRequest is deleted

            // ApprovalFlow -> Approver (ApplicationUser)
            modelBuilder.Entity<ApprovalFlow>()
                .HasOne(af => af.Approver)  // ApprovalFlow has one Approver (ApplicationUser)
                .WithMany()  // ApplicationUser doesn't have a reverse navigation property to ApprovalFlow
                .HasForeignKey(af => af.ApproverId)  // Foreign Key in ApprovalFlow
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of Approver if they have related ApprovalFlows

            // ApprovalFlow -> Position
            modelBuilder.Entity<ApprovalFlow>()
                .HasOne(af => af.Position)  // ApprovalFlow has one Position
                .WithMany()  // Position doesn't have a reverse navigation property to ApprovalFlow
                .HasForeignKey(af => af.PositionId)  // Foreign Key in ApprovalFlow
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of Position if it's used in ApprovalFlows
        }

    }
}
