namespace ConfirmMe.Dto
{
    public class LetterMetadataDto
    {
        public string Title { get; set; } = "";
        public string PdfUrl { get; set; } = "";
        public string QrCodeUrl { get; set; } = "";
        public bool Approved { get; set; }
    }

}
