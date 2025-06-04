using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace ConfirmMe.Models
{
    public class ApprovalRequest
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } // Menambahkan kolom RequestNumber
        public string Title { get; set; }
        public string Description { get; set; }
        public int ApprovalTypeId { get; set; }
        public string RequestedById { get; set; }  // Menggunakan ApplicationUser ID (string)
        public string CurrentStatus { get; set; } // Waiting, Approved, Rejected
        public string? Barcode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } // Menambahkan UpdatedAt (nullable)

        // Navigation Properties
        public ApplicationUser RequestedByUser { get; set; } // Properti navigasi ke ApplicationUser
        public ApprovalType ApprovalType { get; set; } // Properti navigasi ke ApprovalType
        public ICollection<ApprovalFlow> ApprovalFlows { get; set; } // Properti navigasi ke ApprovalFlow

        // Additional Navigation Properties
        public ICollection<Attachment> Attachments { get; set; } // Properti navigasi ke Attachment
        public ICollection<PrintHistory> PrintHistories { get; set; } // Properti navigasi ke PrintHistory

        public byte[]? LetterPdf { get; set; }
    }
}
