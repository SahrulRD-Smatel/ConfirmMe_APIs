using ConfirmMe.Data;
using ConfirmMe.Dto;
using ConfirmMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using System.Globalization;
using System.Security.Claims;

namespace ConfirmMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;
        private readonly AppDbContext _context;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger, AppDbContext context)
        {
            _dashboardService = dashboardService;
            _logger = logger;
            _context = context;
        }

        private (string userId, string role) GetUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
            return (userId ?? "", role ?? "");
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                var (userId, role) = GetUserInfo();
                var summary = await _dashboardService.GetDashboardSummaryAsync(userId, role);

                // Return kosong jika null
                return Ok(summary ?? new DashboardSummaryDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get dashboard summary.");
                return StatusCode(500, new { message = "Failed to get dashboard summary", detail = ex.Message });
            }
        }


        [HttpGet("approval-monthly-by-type")]
        public async Task<IActionResult> GetMonthlyApprovalStatsByType()
        {
            // Step 1: Ambil data scalar ke memory
            var rawData = await _context.ApprovalRequests
                .Where(r => r.CurrentStatus == "Approved" || r.CurrentStatus == "Rejected")
                .Select(r => new
                {
                    ApprovalTypeName = r.ApprovalType.Name,
                    Status = r.CurrentStatus,
                    Month = r.CreatedAt.Month,
                    Year = r.CreatedAt.Year
                })
                .ToListAsync();

            // Step 2: Group dan hitung di memory
            var result = rawData
                .GroupBy(x => new { x.ApprovalTypeName, x.Status, x.Month, x.Year })
                .Select(g => new ApprovalTypeStatDto
                {
                    ApprovalType = g.Key.ApprovalTypeName,
                    Status = g.Key.Status,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Count = g.Count()
                })
                .ToList();

            return Ok(result);
        }



        [HttpGet("waiting-approval")]
        public async Task<IActionResult> GetRequestsWaitingForApproval()
        {
            try
            {
                var (userId, role) = GetUserInfo();
                var count = await _dashboardService.GetRequestsWaitingForApprovalAsync(userId, role);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get requests waiting for approval.");
                return StatusCode(500, new { message = "Failed to get waiting approval data", detail = ex.Message });
            }
        }
    }
}
