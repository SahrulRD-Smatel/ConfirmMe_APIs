using ConfirmMe.Dto;
using ConfirmMe.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

public class CreateApprovalRequestDto
{
    [Required]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string RequestedById { get; set; }

    [Required]
    public int ApprovalTypeId { get; set; }

    
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [Required]
    public List<ApproverDto> Approvers { get; set; }

    public List<IFormFile> Attachments { get; set; }


}
