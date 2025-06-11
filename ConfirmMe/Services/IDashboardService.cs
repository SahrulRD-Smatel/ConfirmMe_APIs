using ConfirmMe.Dto;

namespace ConfirmMe.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(string userId, string role);
        Task<List<ApprovalTypeStatDto>> GetMonthlyApprovalByTypeAsync(string userId, string role);
        Task<int> GetRequestsWaitingForApprovalAsync(string userId, string role);
    }
}
