using ConfirmMe.Data;
using ConfirmMe.Models;
using ConfirmMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using ZXing;
using ZXing.Common;
using ZXing.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using QuestImage = QuestPDF.Infrastructure.Image;
using SharpImage = SixLabors.ImageSharp.Image;

namespace ConfirmMe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LettersController : ControllerBase
    {
        private readonly ILetterService _letterService;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public LettersController(ILetterService letterService, AppDbContext context, IWebHostEnvironment environment)
        {
            _letterService = letterService;
            _context = context;
            _environment = environment;
        }

        private static string GetStorageFolder()
        {
            var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Letters");
            var year = DateTime.Now.Year.ToString();
            var month = DateTime.Now.Month.ToString("D2");
            var fullPath = Path.Combine(baseFolder, year, month);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            return fullPath;
        }

        [HttpGet("metadata/{ApprovalRequestId}")]
        public async Task<IActionResult> GetLetterMetadata(int ApprovalRequestId)
        {
            var metadata = await _letterService.GetLetterMetadataAsync(ApprovalRequestId);
            if (metadata == null)
                return NotFound("Surat belum tersedia atau belum di-approve sepenuhnya.");
            return Ok(metadata);
        }

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
                .Where(a => a.RecordId == request.Id && a.TableName == "ApprovalRequests" && a.ActionType == ActionType.Approved)
                .Select(a => a.ApproverId)
                .Distinct()
                .CountAsync();

            if (approvedStepCount < approvalStepCount)
                return BadRequest("Approval belum selesai. Tidak dapat mengunduh surat.");

            var storageFolder = GetStorageFolder();
            var fileName = $"Surat_{request.Id}.pdf";
            var filePath = Path.Combine(storageFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                var pdfBytes = GenerateLetterPdf(request);
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }

        private static byte[] GenerateLetterPdf(ApprovalRequest request)
        {
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Images", "logo.png");
            var qrBytes = GenerateQrImage($"https://app.confirmme.com/request/{request.Id}");

            QuestImage qrImage;
            using (var ms = new MemoryStream(qrBytes))
            {
                qrImage = QuestImage.FromStream(ms);
            }

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(12));
                    page.Content().Column(col =>
                    {
                        // Header
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(80).Height(60).Image(QuestImage.FromFile(logoPath));
                            row.RelativeItem().AlignCenter().Text("PT. INFO SOLUSINDO DATA UTAMA")
                                .FontSize(18).Bold().FontColor(Colors.Blue.Medium);
                        });

                        col.Item().AlignCenter().Text("SURAT PERSETUJUAN").FontSize(16).Bold();
                        col.Item().AlignRight().Text($"Tanggal: {DateTime.Now:dd MMMM yyyy}");

                        col.Item().Text($"Nama Pemohon: {request.RequestedByUser.FullName}");
                        col.Item().Text($"Jenis Permohonan: {request.ApprovalType.Name}");
                        col.Item().Text("Status: DISETUJUI").Bold().FontColor(Colors.Green.Medium);

                        col.Item().Text("Rincian Persetujuan:").Bold();
                        foreach (var flow in request.ApprovalFlows.OrderBy(f => f.OrderIndex))
                        {
                            col.Item().Text($"• {flow.Position?.Title} - {flow.Approver?.FullName} ({flow.Status})");
                        }

                        col.Item().PaddingTop(10).Text("Dokumen ini dihasilkan secara digital dan telah disetujui oleh seluruh pihak terkait.").Italic();

                        // QR + Signature
                        col.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().Column(inner =>
                            {
                                inner.Item().Text("QR Code Verifikasi:");
                                inner.Item().Height(100).Image(qrImage);
                            });

                            //row.RelativeItem().Column(inner =>
                            //{
                            //    inner.Item().Text("Tanda Tangan Digital").Bold();
                            //    inner.Item().Height(60).LineHorizontal(1f);
                            //    inner.Item().Text(request.RequestedByUser.FullName);
                            //});
                        });
                    });

                    page.Footer().AlignCenter().Text("© 2025 PT Info Solusindo Data Utama. Semua hak cipta dilindungi.")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
            }).GeneratePdf();
        }

        private static byte[] GenerateQrImage(string data)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions { Height = 150, Width = 150, Margin = 1 }
            };

            var pixelData = writer.Write(data);

            using var image = SharpImage.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height);
            using var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            return ms.ToArray();
        }

        [HttpGet("attachments/{id}/download")]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null || string.IsNullOrEmpty(attachment.FilePath))
                return NotFound("Attachment tidak ditemukan.");

            var filePath = Path.Combine(_environment.WebRootPath ?? "", attachment.FilePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound("File tidak ditemukan di server.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            Response.Headers.Add("Content-Disposition", $"inline; filename={attachment.FileName}");
            return File(fileBytes, attachment.ContentType ?? "application/octet-stream");
        }
    }
}
