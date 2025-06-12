using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ConfirmMe.Data;
using ConfirmMe.Models;

namespace ConfirmMe.Services
{
    public class ApprovalFlowService : IApprovalFlowService
    {
        private readonly AppDbContext _context;

        public ApprovalFlowService(AppDbContext context)
        {
            _context = context;
        }

        // Membuat ApprovalFlow baru
        public async Task<ApprovalFlow> CreateApprovalFlowAsync(int ApprovalRequestId, string approverId, int positionId, int orderIndex)
        {
            var approvalRequest = await _context.ApprovalRequests.FindAsync(ApprovalRequestId);
            if (approvalRequest == null)
            {
                throw new ArgumentException("Approval Request tidak ditemukan");
            }

            var position = await _context.Positions.FindAsync(positionId);
            if (position == null)
            {
                throw new ArgumentException("Position tidak ditemukan");
            }

            // Mendapatkan urutan Approval berdasarkan tipe dan level posisi

            //orderIndexType lagi ga dipake sebenernya tapi dibiarin siapa tau kepake lagi, nyalain ini sama GetApprovalOrderIndex dibawahnya kalo kepake
            //var orderIndexType = GetApprovalOrderIndex(approvalRequest.ApprovalType.Name, position.ApprovalLevel);

            var approvalFlow = new ApprovalFlow
            {
                ApprovalRequestId = ApprovalRequestId,
                ApproverId = approverId,
                PositionId = positionId,
                Status = "Pending",
                OrderIndex = orderIndex,  // Correcting OrderIndex here
                QrToken = Guid.NewGuid().ToString(),
                Remark = "",
                ApprovedAt = null
            };

            _context.ApprovalFlows.Add(approvalFlow);
            await _context.SaveChangesAsync();

            return approvalFlow;
        }

        public async Task<ApprovalFlow> GetByIdAsync(int id)
        {
            return await _context.ApprovalFlows.FindAsync(id);
        }


        // Mengupdate status ApprovalFlow
        public async Task<bool> UpdateApprovalFlowStatusAsync(int approvalFlowId, string status)
        {
            var approvalFlow = await _context.ApprovalFlows.FindAsync(approvalFlowId);
            if (approvalFlow == null)
            {
                throw new ArgumentException("Approval Flow tidak ditemukan");
            }

            approvalFlow.Status = status;
            approvalFlow.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Menentukan tipe approval valid or not
        private bool IsValidApprovalType(string approvalType)
        {
            var validTypes = new List<string> { "Approval Only", "Approval to PO", "Approval to Invoice", "Approval Bulk" };
            return validTypes.Contains(approvalType);
        }


        public async Task<List<ApprovalFlow>> GetApprovalFlowsByRequestIdAsync(int ApprovalRequestId)
        {
            var approvalFlows = await _context.ApprovalFlows
                .Include(af => af.ApprovalRequest)  // Menyertakan ApprovalRequest
                .Where(af => af.ApprovalRequestId == ApprovalRequestId)  // Menyaring berdasarkan RequestId
                .OrderBy(af => af.OrderIndex)  // Mengurutkan berdasarkan OrderIndex
                .ToListAsync();  // Mengambil data dari database

            return approvalFlows;
        }

        // Mendapatkan Approval Flow berdasarkan tipe approval
        public async Task<List<ApprovalFlow>> GetApprovalFlowsByTypeAsync(string approvalType)
        {
            if (!IsValidApprovalType(approvalType))
            {
                throw new ArgumentException("Tipe approval tidak valid");
            }

            var approvalFlows = await _context.ApprovalFlows
                .Include(af => af.ApprovalRequest)
                .ThenInclude(ar => ar.ApprovalType)
                .Where(af => af.ApprovalRequest.ApprovalType.Name == approvalType)
                .OrderBy(af => af.OrderIndex)
                .ToListAsync();

            return approvalFlows;
        }


        public async Task<List<ApprovalFlow>> GetPendingApprovalsForUserAsync(string userId)
        {
            return await _context.ApprovalFlows
                .Where(f => f.ApproverId == userId && f.Status == "Pending")
                .Include(f => f.ApprovalRequest)
                    .ThenInclude(r => r.RequestedByUser)
                .Include(f => f.ApprovalRequest)
                    .ThenInclude(r => r.ApprovalType) // 👈 Untuk ApprovalTypeName
                .Include(f => f.ApprovalRequest)
                    .ThenInclude(r => r.ApprovalFlows) // 👈 Untuk hitung TotalSteps
                .OrderBy(f => f.OrderIndex)
                .ToListAsync();
        }

        //a
        public async Task UpdateAsync(ApprovalFlow flow)
        {
            _context.ApprovalFlows.Update(flow);
            await _context.SaveChangesAsync();
        }


        // Memeriksa apakah semua approval sudah disetujui
        public async Task<bool> IsAllApprovedAsync(int ApprovalRequestId)
        {
            var flows = await _context.ApprovalFlows
                .Where(f => f.ApprovalRequestId == ApprovalRequestId)
                .ToListAsync();

            return flows.All(f => f.Status == "Approved");
        }
    }
}
