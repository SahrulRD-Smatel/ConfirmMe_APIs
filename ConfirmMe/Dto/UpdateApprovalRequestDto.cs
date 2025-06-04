using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConfirmMe.Dto
{
    public class UpdateApprovalRequestDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }
        public string? RequestedById { get; set; }

        [Required]
        public int ApprovalTypeId { get; set; }

        [ModelBinder(BinderType = typeof(JsonModelBinder))]
        public List<ApproverDto>? Approvers { get; set; }

        public List<IFormFile>? Attachments { get; set; }
    }
}
