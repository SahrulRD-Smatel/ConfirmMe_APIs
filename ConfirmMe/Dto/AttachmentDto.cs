namespace ConfirmMe.Dto
{
    public class AttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
