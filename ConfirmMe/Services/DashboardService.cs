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

        public async Task<List<ApprovalTypeStatDto>> GetMonthlyApprovalByTypeAsync(string userId, string role)
        {
            var currentYear = DateTime.Now.Year;
            var query = _context.ApprovalRequests
                .Where(r => r.CreatedAt.Year == currentYear)
                .AsQueryable();

            if (role == "Staff")
            {
                query = query.Where(r => r.RequestedById == userId);
            }
            else if (role == "HRD" || role == "Manager" || role == "Director")
            {
                query = query.Where(r =>
                    r.ApprovalFlows.Any(f => f.ApproverId == userId));
            }

            var rawData = await query
                .Select(r => new
                {
                    r.CreatedAt.Month,
                    r.CreatedAt.Year,
                    ApprovalType = r.ApprovalType.Name,
                    Status = r.CurrentStatus // status di ApprovalRequest (Approved, Rejected, Pending)
                })
                .GroupBy(x => new { x.Month, x.Year, x.ApprovalType, x.Status })
                .Select(g => new ApprovalTypeStatDto
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    ApprovalType = g.Key.ApprovalType,
                    Status = g.Key.Status,
                    Count = g.Count()
                })
                .ToListAsync();

            // Ambil semua kombinasi ApprovalType & Status dari raw data
            var approvalTypes = rawData.Select(x => x.ApprovalType).Distinct().ToList();
            var statuses = new[] { "Pending", "Approved", "Rejected" };

            // Buat data default 12 bulan x approval type x status
            var result = new List<ApprovalTypeStatDto>();
            for (int month = 1; month <= 12; month++)
            {
                foreach (var type in approvalTypes)
                {
                    foreach (var status in statuses)
                    {
                        var match = rawData.FirstOrDefault(x =>
                            x.Month == month &&
                            x.ApprovalType == type &&
                            x.Status == status);

                        result.Add(new ApprovalTypeStatDto
                        {
                            Month = month,
                            Year = currentYear,
                            ApprovalType = type,
                            Status = status,
                            Count = match?.Count ?? 0
                        });
                    }
                }
            }

            return result;
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
