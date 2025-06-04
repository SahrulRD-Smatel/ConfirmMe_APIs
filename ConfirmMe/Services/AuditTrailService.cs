using ConfirmMe.Data;
using ConfirmMe.Models;
using Microsoft.AspNetCore.Http;  // Untuk IHttpContextAccessor
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace ConfirmMe.Services
{
    public class AuditTrailService : IAuditTrailService
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;  // Untuk mengambil informasi pengguna

        // Menyuntikkan IHttpContextAccessor dan UserManager untuk akses HttpContext dan informasi pengguna
        public AuditTrailService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task LogActionAsync(string userId, string action, string tableName, int recordId,
            string oldValue = null, string newValue = null, string actionDetails = null,
            string approverId = null, string role = null, ActionType? actionType = null,
            string remark = null, string ipAddress = null, string userAgent = null )
        {
            var user = await _userManager.FindByIdAsync(userId);
            var roleName = role ?? await GetUserRoleAsync(user);

            var auditTrail = new AuditTrail
            {
                UserId = userId,
                ApproverId = approverId,  // ID yang melakukan approve, bisa null jika tidak ada
                Action = action,
                TableName = tableName,
                RecordId = recordId,
                OldValue = oldValue ?? "",
                NewValue = newValue ?? "",
                ActionDetails = actionDetails,
                ChangeDescription = $"Action: {action}, Details: {actionDetails ?? "N/A"}",
                CreatedAt = DateTime.UtcNow,
                IPAddress = GetClientIp(),
                UserAgent = GetUserAgent(),
                Role = roleName,  // Role pengguna, bisa Requester, Approver, dll.
                ActionType = actionType ?? ActionType.Submit, // Default jika null
                Remark = remark ?? "",  // Komentar atau keterangan tambahan
                Status = "Submitted",

            };

            _dbContext.AuditTrails.Add(auditTrail);
            await _dbContext.SaveChangesAsync();
        }

        // Mendapatkan IP address dari HttpContext
        private string GetClientIp()
        {
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        }

        // Mendapatkan User-Agent dari HttpContext
        private string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
        }

        // Mengambil role pengguna berdasarkan UserManager
        private async Task<string> GetUserRoleAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.Count > 0 ? roles[0] : "Unknown";  // Ambil role pertama atau "Unknown" jika tidak ada role
        }
    }
}
