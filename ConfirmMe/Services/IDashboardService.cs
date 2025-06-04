using ConfirmMe.Dto;

namespace ConfirmMe.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(string userId, string role);
        Task<List<ApprovalTypeStatDto>> GetApprovalStatisticsAsync(string userId, string role);
        Task<int> GetRequestsWaitingForApprovalAsync(string userId, string role);
    }
}
