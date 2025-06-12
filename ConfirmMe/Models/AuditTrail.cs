using ConfirmMe.Models;

public enum ActionType
{
    Submit,
    Pending,
    Withdraw,
    Approved,
    Rejected,
    Resubmit,
    Revise,
    Delete
}

public class AuditTrail
{
    public int Id { get; set; }
    public string UserId { get; set; }  // Kunci asing yang merujuk ke ApplicationUser
    public string ApproverId { get; set; }  // ID yang melakukan approve (jika berbeda dari UserId)
    public string Action { get; set; } // Jenis aksi yang dilakukan (Submit, Withdraw, Approve, dsb)
    public string TableName { get; set; } // Nama tabel yang dipengaruhi
    public int RecordId { get; set; } // ID record yang dipengaruhi
    public string OldValue { get; set; } // Nilai lama (untuk perubahan)
    public string NewValue { get; set; } // Nilai baru (untuk perubahan)
    public string ActionDetails { get; set; } // Deskripsi tambahan dari aksi
    public DateTime CreatedAt { get; set; }  // Waktu aksi dilakukan
    public DateTime? UpdatedAt { get; set; }  // Waktu aksi diperbarui (opsional)
    public string IPAddress { get; set; }   // IP Address pengguna
    public string UserAgent { get; set; }   // User-Agent perangkat pengguna
    public string Role { get; set; }  // Peran pengguna dalam konteks aksi ini (Approver, Requester)
    public string Remark { get; set; }  // Komentar atau keterangan dari aksi
    public string Status { get; set; }  // Status dari aksi (opsional)
    public ActionType ActionType { get; set; } // Menandakan tipe aksi (Submit, Withdraw, dsb)
    public string ChangeDescription { get; set; } // Penjelasan lebih rinci tentang perubahan (opsional)

    // Properti navigasi
    public ApplicationUser User { get; set; }  // Merujuk ke ApplicationUser yang melakukan aksi
}
