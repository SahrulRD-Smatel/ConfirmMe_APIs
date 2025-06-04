using System.IO;
using System.Threading.Tasks;
using ConfirmMe.Data;
using Microsoft.EntityFrameworkCore;
using ZXing;
using ZXing.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using ConfirmMe.Dto;
using ConfirmMe.Services;

public class LetterService : ILetterService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public LetterService(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<LetterMetadataDto?> GetLetterMetadataAsync(int ApprovalRequestId)
    {
        var request = await _context.ApprovalRequests
            .Include(r => r.ApprovalFlows)
            .FirstOrDefaultAsync(r => r.Id == ApprovalRequestId);

        if (request == null || !request.ApprovalFlows.All(a => a.Status == "Approved"))
            return null;

        return new LetterMetadataDto
        {
            Title = $"Surat Keterangan {request.ApprovalType} - ID {ApprovalRequestId}",
            PdfUrl = $"/api/letters/download/{ApprovalRequestId}",
            QrCodeUrl = $"/api/barcode/generate?data=https://app.confirmme.com/request/{ApprovalRequestId}&format=QR_CODE",
            Approved = true
        };
    }

    public async Task<byte[]?> GetPdfAsync(int ApprovalRequestId)
    {
        // Simulasikan generate PDF. Nanti bisa gunakan QuestPDF, PdfSharp, dll.
        var request = await _context.ApprovalRequests.FindAsync(ApprovalRequestId);
        if (request == null) return null;

        var content = $"Surat ini menyatakan bahwa request ID {request.Id} telah disetujui sepenuhnya.";
        var filePath = Path.Combine(_env.WebRootPath, "temp", $"Surat_{request.Id}.pdf");

        // Dummy file creation (ganti dengan real PDF generator)
        await File.WriteAllTextAsync(filePath, content);
        return await File.ReadAllBytesAsync(filePath);
    }
}
