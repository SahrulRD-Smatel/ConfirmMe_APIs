using ConfirmMe.Dto;
using ConfirmMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
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

        [HttpGet("approval-stats")]
        public async Task<IActionResult> GetApprovalStatistics()
        {
            try
            {
                var (userId, role) = GetUserInfo();
                var stats = await _dashboardService.GetApprovalStatisticsAsync(userId, role);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get approval statistics.");
                return StatusCode(500, new { message = "Failed to get approval statistics", detail = ex.Message });
            }
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
