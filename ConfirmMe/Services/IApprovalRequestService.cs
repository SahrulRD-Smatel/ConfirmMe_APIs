using ConfirmMe.Dto;
using ConfirmMe.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfirmMe.Services
{
    public interface IApprovalRequestService
    {
        Task<ApprovalRequest> CreateApprovalRequestAsync(ApprovalRequest request);
        Task UpdateApprovalRequestAsync(ApprovalRequest request);
        Task<bool> UpdateApprovalRequestStatusAsync(int approvalRequestId);


        Task<ApprovalRequest> GetApprovalRequestByIdAsync(int id);
        Task<IEnumerable<ApprovalRequest>> GetAllApprovalRequestsAsync();
        Task<ApprovalRequest> UpdateApprovalStatusAsync(int id, string status);
        Task<bool> ApproveRequestAsync(int id, string approverId, string status);

        Task<IEnumerable<ApprovalRequest>> GetApprovalRequestsByUserIdAsync(string UserId);

        // ➡️ Tambahan
        Task AddAttachmentAsync(Attachment attachment, IFormFile file);
        Task<IEnumerable<ApplicationUser>> GetUsersByRolesAsync(string[] roles);
        Task<ApplicationUser> GetUserByIdAsync(string UserId);
        Task<List<ApproverDto>> GetApproversAsync();

        Task DeleteApprovalRequestAsync(int id);

    }
}
