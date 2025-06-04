using ConfirmMe.Data;
using ConfirmMe.Dto;
using Microsoft.EntityFrameworkCore;

namespace ConfirmMe.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(string userId, string role)
        {
            var query = _context.ApprovalRequests.AsQueryable();

            if (role == "Staff")
            {
                query = query.Where(r => r.RequestedById == userId);
            }
            else if (role == "HRD" || role == "Manager" || role == "Director")
            {
                query = query.Where(r =>
                    r.ApprovalFlows.Any(f => f.ApproverId == userId && f.Status == "Pending"));
            }

            var summary = await query.GroupBy(r => r.CurrentStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new DashboardSummaryDto
            {
                Pending = summary.FirstOrDefault(s => s.Status == "Pending")?.Count ?? 0,
                Approved = summary.FirstOrDefault(s => s.Status == "Approved")?.Count ?? 0,
                Rejected = summary.FirstOrDefault(s => s.Status == "Rejected")?.Count ?? 0,
                Completed = summary.FirstOrDefault(s => s.Status == "Completed")?.Count ?? 0,
            };

            return result;
        }

        public async Task<List<ApprovalTypeStatDto>> GetApprovalStatisticsAsync(string userId, string role)
        {
            var query = _context.ApprovalRequests.AsQueryable();

            if (role == "Staff")
            {
                query = query.Where(r => r.RequestedById == userId);
            }
            else if (role == "HRD" || role == "Manager" || role == "Director")
            {
                query = query.Where(r =>
                    r.ApprovalFlows.Any(f => f.ApproverId == userId && f.Status == "Pending"));
            }

            var stats = await query
                .GroupBy(r => r.ApprovalType.Name)
                .Select(g => new ApprovalTypeStatDto
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return stats;
        }

        public async Task<int> GetRequestsWaitingForApprovalAsync(string userId, string role)
        {
            if (role != "HRD" && role != "Manager" && role != "Director")
                return 0;

            return await _context.ApprovalRequests
                .CountAsync(r => r.ApprovalFlows.Any(f => f.ApproverId == userId && f.Status == "Pending"));
        }
    }
}
