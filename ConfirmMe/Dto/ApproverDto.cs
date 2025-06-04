using System.ComponentModel.DataAnnotations;

namespace ConfirmMe.Dto
{
    public class ApproverDto
    {
        [Required]
        public string ApproverId { get; set; }

        [Required]
        public int PositionId { get; set; }

        [Required]
        public string ApproverName { get; set; }

        [Required]
        public string ApproverEmail { get; set; }

        [Required]
        public string PositionName { get; set; }
    }
}
