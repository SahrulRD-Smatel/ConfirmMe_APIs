using ConfirmMe.Services;
using Microsoft.AspNetCore.Mvc;
using ConfirmMe.Dto;
using Microsoft.AspNetCore.Authorization;
using ZXing;  // Pastikan ZXing.Net sudah di-import

namespace ConfirmMe.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PrintController : ControllerBase
    {
        private readonly IApprovalRequestService _approvalService;
        private readonly IBarcodeService _barcodeService;
        private readonly IPdfGeneratorService _pdfGeneratorService;

        public PrintController(
            IApprovalRequestService approvalService,
            IBarcodeService barcodeService,
            IPdfGeneratorService pdfGeneratorService)
        {
            _approvalService = approvalService;
            _barcodeService = barcodeService;
            _pdfGeneratorService = pdfGeneratorService;
        }

        // GET: api/print/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> PrintApproval(int id)
        {
            var approval = await _approvalService.GetApprovalRequestByIdAsync(id);
            if (approval == null)
            {
                return BadRequest("Request tidak ditemukan.");
            }

            // Mengecek Role Pengguna
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value; 
            if (userRole == null)
            {
                return Unauthorized("User role is missing.");
            }


            if (!HasPrintPermission(userRole, approval.CurrentStatus))
            {
                return Forbid("Anda tidak memiliki akses untuk mencetak dokumen.");
            }

            var barcodeFormat = BarcodeFormat.CODE_128; 
            var barcodeImage = _barcodeService.GenerateBarcode(approval.Id.ToString(), barcodeFormat, 300, 150); 

            var dto = new ApprovalRequestDetailDto
            {
                Title = approval.Title,
                Description = approval.Description,
                ApprovalType = approval.ApprovalType.Name,
                ApprovalFlows = approval.ApprovalFlows.Select(f => new ApprovalFlowDto
                {
                    PositionTitle = f.Position.Title,
                    ApproverName = f.Approver.FullName,
                    Status = f.Status,
                    ApprovedAt = f.ApprovedAt
                }).ToList()
            };

            return Ok(new
            {
                dto.Title,
                dto.Description,
                dto.ApprovalType,
                dto.ApprovalFlows,
                Barcode = Convert.ToBase64String(barcodeImage)
            });
        }

        // GET: api/print/{id}/pdf
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var approval = await _approvalService.GetApprovalRequestByIdAsync(id);
            if (approval == null)
            {
                return BadRequest("Request tidak ditemukan.");
            }

            // Mengecek Role Pengguna
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value; 
            if (userRole == null)
            {
                return Unauthorized("User role is missing.");
            }

            if (!HasPrintPermission(userRole, approval.CurrentStatus))
            {
                return Forbid("Anda tidak memiliki akses untuk mencetak dokumen.");
            }


            var barcodeFormat = BarcodeFormat.CODE_128; 
            var barcodeImage = _barcodeService.GenerateBarcode(approval.Id.ToString(), barcodeFormat, 300, 150); 

            var dto = new ApprovalRequestDetailDto
            {
                Title = approval.Title,
                Description = approval.Description,
                ApprovalType = approval.ApprovalType.Name,
                ApprovalFlows = approval.ApprovalFlows.Select(f => new ApprovalFlowDto
                {
                    PositionTitle = f.Position.Title,
                    ApproverName = f.Approver.FullName,
                    Status = f.Status,
                    ApprovedAt = f.ApprovedAt
                }).ToList()
            };

            var pdfBytes = _pdfGeneratorService.GenerateApprovalPdf(dto, barcodeImage);

            return File(pdfBytes, "application/pdf", $"Approval_{approval.Id}.pdf");
        }

 
        private bool HasPrintPermission(string role, string approvalStatus)
        {
            // Memeriksa izin berdasarkan role dan status approval
            switch (role)
            {
                case "Staff":
                    return approvalStatus == "Approved"; 
                case "HRD":
                case "Manager":
                case "Direktur":
                    return true;  
                default:
                    return false; 
            }
        }
    }
}
