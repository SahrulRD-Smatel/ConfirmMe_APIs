using ConfirmMe.Models;
using ConfirmMe.Services;
using ConfirmMe.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AutoMapper;
using Newtonsoft.Json;


namespace ConfirmMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApprovalRequestsController : ControllerBase
    {
        private readonly IApprovalRequestService _approvalRequestService;
        private readonly IApprovalFlowService _approvalFlowService;
        private readonly INotificationService _notificationService;
        private readonly IAuditTrailService _auditTrailService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public ApprovalRequestsController(
            IApprovalRequestService approvalRequestService,
            IApprovalFlowService approvalFlowService,
            INotificationService notificationService,
            IAuditTrailService auditTrailService,
            IEmailService emailService,
            IMapper mapper)
        {
            _approvalRequestService = approvalRequestService;
            _approvalFlowService = approvalFlowService;
            _notificationService = notificationService;
            _auditTrailService = auditTrailService;
            _emailService = emailService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Staff, Manager, HRD, Direktur")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApprovalRequestDto>>> GetAll()
        {
            var requests = await _approvalRequestService.GetAllApprovalRequestsAsync();
            var dtos = _mapper.Map<List<ApprovalRequestDto>>(requests);
            return Ok(dtos);
        }

        [Authorize(Roles = "Manager, HRD, Direktur, Staff")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApprovalRequest>> GetById(int id)
        {
            var request = await _approvalRequestService.GetApprovalRequestByIdAsync(id);
            if (request == null)
                return NotFound();
            return Ok(request);
        }

        [Authorize(Roles = "Staff, Manager, HRD, Direktur")]
        [HttpGet("approvers")]
        public async Task<ActionResult<List<ApproverDto>>> GetApprovers()
        {
            var approvers = await _approvalRequestService.GetApproversAsync();
            return Ok(approvers);
        }


        [Authorize(Roles = "Staff, Manager, HRD, Direktur")]
        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromForm] CreateApprovalRequestDto dto)
        {
            // Validasi input
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest("Title and Description are required.");

            if (string.IsNullOrEmpty(dto.RequestedById))
                return BadRequest("RequestedById is required.");

            if (dto.Approvers == null || !dto.Approvers.Any())
                return BadRequest("At least one approver is required.");

            try
            {
                // Buat entitas ApprovalRequest
                var newRequest = new ApprovalRequest
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    RequestedById = dto.RequestedById,
                    ApprovalTypeId = dto.ApprovalTypeId,
                    RequestNumber = GenerateRequestNumber(),
                    CurrentStatus = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _approvalRequestService.CreateApprovalRequestAsync(newRequest);

                if (created == null || created.Id == 0)
                    return StatusCode(500, "Failed to create approval request.");

                // Buat flow approval untuk setiap approver
                for (int i = 0; i < dto.Approvers.Count; i++)
                {
                    var approver = dto.Approvers[i];
                    await _approvalFlowService.CreateApprovalFlowAsync(
                        created.Id,
                        approver.ApproverId,
                        approver.PositionId,
                        i + 1);
                }

                // Simpan attachment (file upload)
                if (dto.Attachments != null && dto.Attachments.Count > 0)
                {
                    foreach (var file in dto.Attachments)
                    {
                        var attachment = new Attachment
                        {
                            ApprovalRequestId = created.Id
                        };
                        await _approvalRequestService.AddAttachmentAsync(attachment, file);
                    }
                }

                // Log audit trail
                await _auditTrailService.LogActionAsync(
                    dto.RequestedById,
                    "Create Approval Request",
                    "ApprovalRequests",
                    created.Id,
                    actionDetails: $"Title: {created.Title}, RequestedBy: {dto.RequestedById}",
                    approverId: dto.RequestedById);

                // Return 201 Created dengan data approval request yang baru dibuat
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Internal server error",
                    details = ex.Message,
                    inner = ex.InnerException?.Message,
                    inner2 = ex.InnerException?.InnerException?.Message
                });
            }
        }


        [Authorize(Roles = "Staff, Manager, HRD, Direktur")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateRequest(int id, [FromForm] UpdateApprovalRequestDto dto)
        {
            var existing = await _approvalRequestService.GetApprovalRequestByIdAsync(id);
            if (existing == null)
                return NotFound("Approval request not found.");

            // Cek status boleh diupdate kalau Pending atau Reject
            if (existing.CurrentStatus != "Pending" && existing.CurrentStatus != "Reject")
                return BadRequest("Only pending or rejected requests can be updated.");

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found.");

                // Update properti dasar
                existing.Title = dto.Title;
                existing.Description = dto.Description;
                existing.ApprovalTypeId = dto.ApprovalTypeId;
                existing.UpdatedAt = DateTime.UtcNow;

                await _approvalRequestService.UpdateApprovalRequestAsync(existing);

                // Update attachments jika ada
                if (dto.Attachments != null && dto.Attachments.Count > 0)
                {
                    foreach (var file in dto.Attachments)
                    {
                        var attachment = new Attachment
                        {
                            ApprovalRequestId = id
                        };
                        await _approvalRequestService.AddAttachmentAsync(attachment, file);
                    }
                }

                // Log Audit
                await _auditTrailService.LogActionAsync(
                    userId,
                    "Update Approval Request",
                    "ApprovalRequests",
                    id,
                    actionDetails: $"Updated Title: {dto.Title}, Updated Description: {dto.Description}",
                    approverId: userId,
                    role: "Staff",
                    actionType: ActionType.Resubmit
                );

                return Ok(new { message = "Request updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Internal server error",
                    details = ex.Message
                });
            }
        }


        private string GenerateRequestNumber()
        {
            var datePart = DateTime.Now.ToString("yyyyMMdd");
            var randomPart = new Random().Next(100, 999);
            return $"REQ{datePart}-{randomPart}";
        }


        [Authorize(Roles = "Manager, HRD, Direktur")]
        [HttpGet("inbox")]
        public async Task<ActionResult<IEnumerable<InboxItemDto>>> GetInbox()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found.");

            var pendingFlows = await _approvalFlowService.GetPendingApprovalsForUserAsync(userId);

            var mapped = _mapper.Map<List<InboxItemDto>>(pendingFlows);

            return Ok(mapped);
        }



        [Authorize(Roles = "HRD, Manager, Direktur")]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveRequest(int id, [FromBody] ApproveRequestDto dto)
        {
            try
            {
                // Ambil semua approval flow dan urutkan berdasarkan step
                var flows = (await _approvalFlowService.GetApprovalFlowsByRequestIdAsync(id)).OrderBy(f => f.OrderIndex).ToList();

                // Cari step aktif (belum diapprove/reject)
                var currentStep = flows.FirstOrDefault(f => f.Status != "Approved");

                if (currentStep == null)
                    return BadRequest("This request has already been fully approved.");

                // Cek apakah approver saat ini adalah yang berhak menyetujui
                if (currentStep.ApproverId != dto.ApproverId)
                    return Forbid("You are not authorized to approve this request at this stage.");

                // Update status pada current step
                await _approvalFlowService.UpdateApprovalFlowStatusAsync(currentStep.Id, dto.Status);

                // Log audit trail
                await _auditTrailService.LogActionAsync(
                    dto.ApproverId,
                    "Approve Request",
                    "ApprovalRequests",
                    id,
                    oldValue: "Pending",
                    newValue: dto.Status,
                    actionDetails: $"Approver: {dto.ApproverId}, Status: {dto.Status}",
                    approverId: dto.ApproverId,
                    role: "Approver",
                    actionType: dto.Status == "Approved" ? ActionType.Approved : ActionType.Reject,
                    remark: "Approved after review"
                );

                // Notifikasi ke Approver
                await _notificationService.CreateNotificationAsync(
                    dto.ApproverId,
                    $"You have {dto.Status.ToLower()} the request.",
                    "Approval Status Updated",
                    id,
                    "ApprovalRequest"
                );

                // Ambil data approval request
                var approvalRequest = await _approvalRequestService.GetApprovalRequestByIdAsync(id);
                if (approvalRequest == null)
                    return NotFound("Approval request not found.");

                // Notifikasi ke Requestor
                await _notificationService.CreateNotificationAsync(
                    approvalRequest.RequestedById,
                    $"Your approval request \"{approvalRequest.Title}\" has been {dto.Status.ToLower()} by one of the approvers.",
                    "Approval Progress Update",
                    approvalRequest.Id,
                    "ApprovalRequest"
                );

                // Email ke Requestor
                await _emailService.SendApprovalStatusChangedAsync(
                    approvalRequest.RequestedByUser.Email,
                    approvalRequest.Title,
                    dto.Status,
                    "Requester"
                );

                // Jika ada reject, hentikan proses approval
                if (dto.Status == "Rejected")
                {
                    await _approvalRequestService.UpdateApprovalStatusAsync(id, "Rejected");

                    return Ok(new { message = "Request rejected." });
                }

                // Cek apakah semua step sudah approved
                var approvedCount = flows.Count(f => f.Status == "Approved");
                var totalSteps = flows.Count();
                if (approvedCount == totalSteps)
                {
                    await _approvalRequestService.UpdateApprovalStatusAsync(id, "Completed");

                    // Notifikasi akhir ke Requestor
                    await _notificationService.CreateNotificationAsync(
                        approvalRequest.RequestedById,
                        $"Your request \"{approvalRequest.Title}\" has been fully approved.",
                        "Request Fully Approved",
                        approvalRequest.Id,
                        "ApprovalRequest"
                    );

                    // Email akhir ke Requestor
                    await _emailService.SendApprovalStatusChangedAsync(
                        approvalRequest.RequestedByUser.Email,
                        approvalRequest.Title,
                        "Fully Approved",
                        "Requester"
                    );

                    // TODO: Tambahkan proses generate surat & barcode disini kalo butuh
                }

                return Ok(new { message = $"Request {dto.Status.ToLower()}." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [AllowAnonymous]
        [HttpPost("approval-via-qr")]
        public async Task<IActionResult> ApproveViaQrCode([FromBody] QrApprovalDto dto)
        {
            try
            {
                if (dto == null || dto.ApprovalRequestId <= 0 || dto.ApproverId <= 0 || dto.FlowId <= 0)
                    return BadRequest("QR code data is incomplete.");

                if (!Enum.TryParse<ActionType>(dto.Action, true, out var actionType))
                    return BadRequest("Invalid action. Use Approved or Reject.");

                if (actionType != ActionType.Approved && actionType != ActionType.Reject)
                    return BadRequest("Only Approved or Reject actions are allowed via QR.");

                var flow = await _approvalFlowService.GetByIdAsync(dto.FlowId);
                if (flow == null)
                    return NotFound("Approval step not found.");

                if (flow.ApprovalRequestId != dto.ApprovalRequestId || flow.ApproverId != dto.ApproverId.ToString())
                    return Forbid("QR code does not match the approver or request.");

                if (flow.Status == "Approved" || flow.Status == "Rejected")
                    return BadRequest("This step has already been processed.");

                // Token check
                if (string.IsNullOrEmpty(dto.QrToken) || dto.QrToken != flow.QrToken)
                    return Forbid("QR token is invalid or has expired.");

                if (flow.IsQrUsed)
                    return BadRequest("QR code has already been used.");

                if (flow.QrTokenGeneratedAt == null || flow.QrTokenGeneratedAt.Value.AddHours(1) < DateTime.UtcNow)
                    return BadRequest("QR code has expired.");


                // ✅ Update approval status
                await _approvalFlowService.UpdateApprovalFlowStatusAsync(flow.Id, actionType.ToString());

                // ✅ Tandai QR sudah digunakan dan simpan Remark
                flow.IsQrUsed = true;
                flow.QrUsedAt = DateTime.UtcNow;
                flow.Remark = dto.Remark;
                await _approvalFlowService.UpdateAsync(flow);

                // 📝 Audit Trail
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";

                await _auditTrailService.LogActionAsync(
                    userId: dto.ApproverId.ToString(),
                    action: dto.Action,
                    tableName: "ApprovalRequests",
                    recordId: dto.ApprovalRequestId,
                    oldValue: "Pending",
                    newValue: dto.Action,
                    actionDetails: "Approval via QR code",
                    approverId: dto.ApproverId.ToString(),
                    role: "Approver",
                    actionType: actionType,
                    remark: dto.Remark ?? $"{dto.Action} via QR",
                    ipAddress: ipAddress,
                    userAgent: userAgent
                );

                // Jika ditolak
                if (actionType == ActionType.Reject)
                {
                    await _approvalRequestService.UpdateApprovalStatusAsync(dto.ApprovalRequestId, "Rejected");
                    return Ok(new { message = "Request rejected via QR." });
                }

                // Jika semua sudah disetujui
                var flows = (await _approvalFlowService.GetApprovalFlowsByRequestIdAsync(dto.ApprovalRequestId)).ToList();
                var approvedCount = flows.Count(f => f.Status == "Approved");
                if (approvedCount == flows.Count)
                {
                    await _approvalRequestService.UpdateApprovalStatusAsync(dto.ApprovalRequestId, "Completed");
                    return Ok(new { message = "Request fully approved via QR." });
                }

                return Ok(new { message = $"Request {dto.Action.ToLower()} via QR." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }


        [Authorize(Roles = "Manager, HRD, Direktur")]
        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApprovalRequest>> UpdateStatus(int id, [FromQuery] string status)
        {
            var updated = await _approvalRequestService.UpdateApprovalStatusAsync(id, status);
            return Ok(updated);
        }

        [Authorize(Roles = "Staff, HRD, Manager, Direktur")]
        [HttpGet("my-requests")]
        public async Task<ActionResult<IEnumerable<ApprovalRequest>>> GetMyApprovalRequests()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found.");

            var requests = await _approvalRequestService.GetApprovalRequestsByUserIdAsync(userId);

            if (requests == null || !requests.Any())
                return NotFound("No approval requests found for this user.");

            return Ok(requests);
        }

        [Authorize(Roles = "Staff, Manager, HRD, Direktur")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found.");

            var request = await _approvalRequestService.GetApprovalRequestByIdAsync(id);
            if (request == null)
                return NotFound("Approval request not found.");

            if (request.RequestedById != userId)
                return Forbid("You are not authorized to delete this request.");

            // Cek status jika ingin batasi hanya request yang status Pending atau Reject yang bisa dihapus
            if (request.CurrentStatus != "Pending" && request.CurrentStatus != "Reject")
                return BadRequest("Only pending or rejected requests can be deleted.");

            try
            {
                // Hapus request via service
                await _approvalRequestService.DeleteApprovalRequestAsync(id);

                // Log audit trail
                await _auditTrailService.LogActionAsync(
                    userId,
                    "Delete Approval Request",
                    "ApprovalRequests",
                    id,
                    actionDetails: $"Deleted request titled: {request.Title}",
                    approverId: userId,
                    role: "Staff",
                    actionType: ActionType.Delete
                );

                return Ok(new { message = "Request deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
