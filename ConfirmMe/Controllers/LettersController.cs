using Microsoft.AspNetCore.Mvc;
using ConfirmMe.Services;
using Microsoft.AspNetCore.Authorization;
using ConfirmMe.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using ConfirmMe.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace ConfirmMe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LettersController : ControllerBase
    {
        private readonly ILetterService _letterService;
        private readonly AppDbContext _context;

        // Folder penyimpanan file PDF
        private readonly string _storageFolder = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Letters");

        public LettersController(ILetterService letterService, AppDbContext context)
        {
            _letterService = letterService;
            _context = context;

            // Pastikan folder storage ada
            if (!Directory.Exists(_storageFolder))
            {
                Directory.CreateDirectory(_storageFolder);
            }
        }

        // GET: /api/letters/metadata/123
        [HttpGet("metadata/{ApprovalRequestId}")]
        public async Task<IActionResult> GetLetterMetadata(int ApprovalRequestId)
        {
            var metadata = await _letterService.GetLetterMetadataAsync(ApprovalRequestId);
            if (metadata == null)
                return NotFound("Surat belum tersedia atau belum di-approve sepenuhnya.");

            return Ok(metadata);
        }

        // Tambahkan method untuk membuat folder path sesuai tahun dan bulan
        private string GetStorageFolder()
        {
            var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Letters");
            var yearFolder = DateTime.Now.Year.ToString();
            var monthFolder = DateTime.Now.Month.ToString("D2"); // format 01, 02, ..., 12
            var fullPath = Path.Combine(baseFolder, yearFolder, monthFolder);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            return fullPath;
        }

        // Ganti di method DownloadLetter
        [HttpGet("{ApprovalRequestId}/download")]
        public async Task<IActionResult> DownloadLetter(int ApprovalRequestId)
        {
            var request = await _context.ApprovalRequests
                .Include(r => r.ApprovalFlows)
                    .ThenInclude(f => f.Approver)
                .Include(r => r.ApprovalFlows)
                    .ThenInclude(f => f.Position)
                .Include(r => r.RequestedByUser)
                .Include(r => r.ApprovalType)
                .FirstOrDefaultAsync(r => r.Id == ApprovalRequestId);

            if (request == null)
                return NotFound("Request tidak ditemukan.");

            var approvalStepCount = request.ApprovalFlows.Count;

            var approvedStepCount = await _context.AuditTrails
                .Where(a =>
                    a.RecordId == request.Id &&
                    a.TableName == "ApprovalRequests" &&
                    a.ActionType == ActionType.Approved)
                .Select(a => a.ApproverId)
                .Distinct()
                .CountAsync();

            var allApproved = approvedStepCount >= approvalStepCount;

            if (!allApproved)
                return BadRequest("Approval anda belum selesai. Tidak dapat mengunduh surat.");

            // Dapatkan folder penyimpanan dengan struktur tahun/bulan
            var storageFolder = GetStorageFolder();

            string fileName = $"Surat_{request.Id}.pdf";
            string filePath = Path.Combine(storageFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                var pdfBytes = GenerateLetterPdf(request);
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }


        private byte[] GenerateLetterPdf(ApprovalRequest request)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(12));
                    page.Header()
                        .Height(60)
                        .Row(row =>
                        {                            
                            row.RelativeItem(1).AlignMiddle().Image(Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Images", "logo.png"), ImageScaling.FitHeight);

                            row.RelativeItem(4).AlignMiddle().Text("PERUSAHAAN INFO SOLUSINDO DATA UTAMA").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Column(col =>
                        {
                            col.Item().Text("SURAT PERSETUJUAN").FontSize(18).Bold().AlignCenter();
                            col.Item().Text($"Tanggal: {DateTime.Now:dd MMMM yyyy}").AlignRight();

                            col.Item().Text($"Nama Pemohon: {request.RequestedByUser.FullName}");
                            col.Item().Text($"Jenis Permohonan: {request.ApprovalType.Name}");
                            col.Item().Text("Status: DISETUJUI").FontColor(Colors.Green.Medium).Bold();

                            col.Item().PaddingTop(15).Text("Rincian Persetujuan:").Bold();
                            foreach (var flow in request.ApprovalFlows.OrderBy(f => f.OrderIndex))
                            {
                                col.Item().Text($"• {flow.Position?.Title} ({flow.Approver?.FullName}) - {flow.Status}");
                            }

                            col.Item().PaddingTop(30).Text("Dokumen ini dihasilkan secara digital dan telah disetujui oleh seluruh pihak terkait.").Italic();
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("© 2025 Perusahaan Info Solusindo Data Utama. Semua hak cipta dilindungi. ").FontSize(9).FontColor(Colors.Grey.Medium);
                            x.Span("Halaman ");
                            x.CurrentPageNumber();
                            x.Span(" dari ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();
        }
    }
}
