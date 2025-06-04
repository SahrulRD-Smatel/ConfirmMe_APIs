namespace ConfirmMe.Models
{
    public class Position
    {
        public int Id { get; set; } // ID unik untuk posisi
        public string Title { get; set; } // Nama posisi, seperti "Manager", "HRD", dll.
        public int ApprovalLevel { get; set; } // Tingkat approval (misalnya, level 2 untuk manager, level 4 untuk direktur, dll.)

        // Navigasi ke pengguna yang memiliki posisi ini
        public ICollection<ApplicationUser> Users { get; set; }
    }
}
