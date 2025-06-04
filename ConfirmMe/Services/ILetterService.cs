using ConfirmMe.Dto;

namespace ConfirmMe.Services
{
    public interface ILetterService
    {
        Task<LetterMetadataDto?> GetLetterMetadataAsync(int ApprovalRequestId);
        Task<byte[]?> GetPdfAsync(int ApprovalRequestId);
    }

}
