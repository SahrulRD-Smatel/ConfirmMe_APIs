using System.Threading.Tasks;
using ConfirmMe.Models;

namespace ConfirmMe.Services
{
    public interface IApprovalFlowService
    {
        Task<ApprovalFlow> CreateApprovalFlowAsync(int ApprovalRequestId, string approverId, int positionId, int orderIndex);
        Task<bool> UpdateApprovalFlowStatusAsync(int approvalFlowId, string status, string remark);
        Task<bool> IsAllApprovedAsync(int ApprovalRequestId);
        Task<List<ApprovalFlow>> GetApprovalFlowsByRequestIdAsync(int ApprovalRequestId);
        Task<List<ApprovalFlow>> GetApprovalFlowsByTypeAsync(string approvalType);
        Task<List<ApprovalFlow>> GetPendingApprovalsForUserAsync(string userId);
        Task UpdateAsync(ApprovalFlow flow);
        Task<ApprovalFlow> GetByIdAsync(int id);

    }
}
