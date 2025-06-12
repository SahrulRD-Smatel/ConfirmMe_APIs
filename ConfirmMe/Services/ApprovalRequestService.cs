using ConfirmMe.Data;
using ConfirmMe.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // Untuk IFormFile
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ConfirmMe.Dto;

namespace ConfirmMe.Services
{
    public class ApprovalRequestService : IApprovalRequestService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApprovalRequestService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<ApproverDto>> GetApproversAsync()
        {
            var approvers = await _context.Users
                .Include(u => u.Position)
                .Where(u => u.Role == "Manager" || u.Role == "HRD" || u.Role == "Direktur")
                .Select(u => new ApproverDto
                {
                    ApproverId = u.Id,
                    ApproverName = u.FullName,
                    ApproverEmail = u.Email,
                    PositionId = u.PositionId, // Pastikan sudah sesuai dengan DTO
                    PositionName = u.Position.Title
                })
                .ToListAsync();

            return approvers;
        }

        // Implementasi AddAttachmentAsync yang sesuai dengan antarmuka
        public async Task AddAttachmentAsync(Attachment attachment, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File tidak valid.");

            var allowedExtensions = new[] { ".pdf", ".docx", ".jpg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("File type tidak diizinkan.");

            if (file.Length > 10_000_000) // 10 MB limit
                throw new ArgumentException("Ukuran file terlalu besar.");

            // ⛔️ Pastikan ApprovalRequestId tidak kosong/null
            if (attachment.ApprovalRequestId == 0)
                throw new ArgumentException("ApprovalRequestId harus diisi sebelum menyimpan attachment.");

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                attachment.FileContent = ms.ToArray();
            }

            attachment.FileName = file.FileName;
            attachment.ContentType = file.ContentType;
            attachment.UploadedAt = DateTime.UtcNow;

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
        }


        //public async Task AddAttachmentAsync(Attachment attachment, IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        throw new ArgumentException("File tidak valid.");

        //    var allowedExtensions = new[] { ".pdf", ".docx", ".jpg", ".png" };
        //    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        //    if (!allowedExtensions.Contains(extension))
        //        throw new ArgumentException("File type tidak diizinkan.");

        //    if (file.Length > 10_000_000) // 10 MB limit
        //        throw new ArgumentException("Ukuran file terlalu besar.");

        //    // Baca file ke memory stream dan simpan ke byte[]
        //    using (var ms = new MemoryStream())
        //    {
        //        await file.CopyToAsync(ms);
        //        attachment.FileContent = ms.ToArray();
        //    }

        //    attachment.FileName = file.FileName;
        //    attachment.ContentType = file.ContentType;
        //    attachment.UploadedAt = DateTime.UtcNow;

        //    _context.Attachments.Add(attachment);
        //    await _context.SaveChangesAsync();
        //}


        // Implementasi metode lainnya yang sesuai dengan antarmuka IApprovalRequestService

        public async Task<IEnumerable<ApplicationUser>> GetUsersByRolesAsync(string[] roles)
        {
            var users = new List<ApplicationUser>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                users.AddRange(usersInRole);
            }

            return users.Distinct(); // Hindari duplikat jika user punya banyak role
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ArgumentException("User tidak ditemukan.");
            }

            return user;
        }

        // Implementasi lainnya sesuai antarmuka (CreateApprovalRequestAsync, etc.)
        public async Task<ApprovalRequest> CreateApprovalRequestAsync(ApprovalRequest request)
        {
            var approvalType = await _context.ApprovalTypes.FindAsync(request.ApprovalTypeId);
            if (approvalType == null)
            {
                throw new ArgumentException("Approval Type tidak ditemukan");
            }

            request.CreatedAt = DateTime.UtcNow;
            _context.ApprovalRequests.Add(request);
            await _context.SaveChangesAsync();

            return request;
        }

        public async Task<ApprovalRequest> GetApprovalRequestByIdAsync(int id)
        {
            var request = await _context.ApprovalRequests
                .Include(ar => ar.ApprovalType)
                .Include(ar => ar.RequestedByUser)
                .Include(ar => ar.ApprovalFlows)
                    .ThenInclude(af => af.Position)
                .Include(ar => ar.ApprovalFlows)
                    .ThenInclude(af => af.Approver)
                .FirstOrDefaultAsync(ar => ar.Id == id);

            return request;
        }

        public async Task<IEnumerable<ApprovalRequest>> GetAllApprovalRequestsAsync()
        {
            return await _context.ApprovalRequests
                .Include(r => r.RequestedByUser)
                .Include(ar => ar.ApprovalType)
                .Include(r => r.Attachments)
                .Include(r => r.ApprovalFlows)
                    .ThenInclude(f => f.Approver)
                .Include(r => r.ApprovalFlows)
                    .ThenInclude(f => f.Position)
                .ToListAsync();
        }

        public async Task<ApprovalRequest> UpdateApprovalStatusAsync(int id, string status)
        {
            var request = await _context.ApprovalRequests.FindAsync(id);
            if (request == null)
            {
                throw new ArgumentException("Approval Request tidak ditemukan");
            }

            request.CurrentStatus = status;
            await _context.SaveChangesAsync();

            return request;
        }

        //public async Task<bool> ApproveRequestAsync(int id, string approverId, string status)
        //{
        //    var request = await _context.ApprovalRequests
        //        .Include(ar => ar.ApprovalFlows)
        //        .FirstOrDefaultAsync(ar => ar.Id == id);

        //    if (request == null)
        //        throw new ArgumentException("Approval Request tidak ditemukan");

        //    var approvalFlow = request.ApprovalFlows.FirstOrDefault(af => af.ApproverId == approverId);
        //    if (approvalFlow == null)
        //        throw new ArgumentException("Approver tidak ditemukan di flow ini");

        //    approvalFlow.Status = status;
        //    approvalFlow.ApprovedAt = DateTime.UtcNow;

        //    request.CurrentStatus = request.ApprovalFlows.All(af => af.Status == "Approved")
        //        ? "Approved"
        //        : "In Progress";

        //    await _context.SaveChangesAsync();
        //    return true;
        //}


        public async Task<bool> ApproveRequestAsync(int requestId, string approverId, string status)
        {
            var request = await _context.ApprovalRequests
                .Include(ar => ar.ApprovalFlows)
                .FirstOrDefaultAsync(ar => ar.Id == requestId);

            if (request == null)
                throw new ArgumentException("Approval Request tidak ditemukan");

            var currentStep = request.ApprovalFlows
                .OrderBy(f => f.OrderIndex)
                .FirstOrDefault(f => f.Status != "Approved" && f.Status != "Rejected");

            if (currentStep == null)
                throw new InvalidOperationException("Request has been fully processed.");

            if (currentStep.ApproverId != approverId)
                throw new UnauthorizedAccessException("Anda tidak berhak menyetujui pada tahap ini.");

            currentStep.Status = status;
            currentStep.ApprovedAt = DateTime.UtcNow;

            // Jika Rejected, hentikan proses approval
            if (status == "Rejected")
            {
                request.CurrentStatus = "Rejected";
            }
            else
            {
                var approvedCount = request.ApprovalFlows.Count(f => f.Status == "Approved");
                var total = request.ApprovalFlows.Count;

                request.CurrentStatus = (approvedCount == total) ? "Completed" : "In Progress";
            }

            // Tambahan: update UpdatedAt agar tidak null jika diwajibkan di DB
            request.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // Ambil pesan inner exception (biasanya dari SQL Server)
                var innerMessage = ex.InnerException?.Message ?? "Tidak ada inner exception.";
                throw new Exception($"Gagal menyimpan perubahan: {ex.Message} | Inner: {innerMessage}", ex);
            }
        }


        public async Task<IEnumerable<ApprovalRequest>> GetApprovalRequestsByUserIdAsync(string userId)
        {
            return await _context.ApprovalRequests
                .Where(r => r.RequestedById == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateApprovalRequestAsync(ApprovalRequest request)
        {
            _context.ApprovalRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteApprovalRequestAsync(int id)
        {
            var request = await _context.ApprovalRequests
                .Include(ar => ar.Attachments)
                .Include(ar => ar.ApprovalFlows)
                .FirstOrDefaultAsync(ar => ar.Id == id);

            if (request == null)
                throw new ArgumentException("Approval Request tidak ditemukan.");

            // Jika ada attachment, hapus juga
            if (request.Attachments != null && request.Attachments.Any())
            {
                _context.Attachments.RemoveRange(request.Attachments);
            }

            // Hapus approval flows terkait
            if (request.ApprovalFlows != null && request.ApprovalFlows.Any())
            {
                _context.ApprovalFlows.RemoveRange(request.ApprovalFlows);
            }

            // Hapus approval request itu sendiri
            _context.ApprovalRequests.Remove(request);

            await _context.SaveChangesAsync();
        }


    }
}
