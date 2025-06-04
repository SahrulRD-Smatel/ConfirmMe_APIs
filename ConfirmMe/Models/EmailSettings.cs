namespace ConfirmMe.Models
{
    public class EmailSettings
    {
        public string FromName { get; set; }   // Menyimpan nama pengirim
        public string FromEmail { get; set; }  // Menyimpan email pengirim
        public string SmtpServer { get; set; } // Menyimpan alamat server SMTP
        public int Port { get; set; }          // Menyimpan port SMTP
        public string Username { get; set; }   // Menyimpan username SMTP
        public string Password { get; set; }   // Menyimpan password SMTP
    }

    
}