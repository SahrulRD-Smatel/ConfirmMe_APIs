using ConfirmMe.Dto;

namespace ConfirmMe.Services
{
    public interface IPdfGeneratorService
    {
        byte[] GenerateApprovalPdf(ApprovalRequestDetailDto request, byte[] barcodeImage);
    }
}
