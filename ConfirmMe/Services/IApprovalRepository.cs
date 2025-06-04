using ConfirmMe.Models;

namespace ConfirmMe.Services
{
    public interface IApprovalRepository
    {
        Task<ApprovalRequest> GetApprovalRequestByIdAsync(int approvalRequestId);
        Task<bool> UpdateApprovalStatusAsync(ApprovalRequest approvalRequest);
        Task AddApprovalRequestAsync(ApprovalRequest approvalRequest);
    }

}
