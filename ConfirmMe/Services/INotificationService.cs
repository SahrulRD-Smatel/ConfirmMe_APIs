using ConfirmMe.Models;

namespace ConfirmMe.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string recipientId, string message, string type, int? relatedEntityId = null, string relatedEntityType = null, string urgencyLevel = "Medium", DateTime? expiryDate = null, string actionUrl = null);
        Task MarkNotificationAsReadAsync(int notificationId);
        Task<List<Notification>> GetUnreadNotificationsAsync(string userId);

        // Menambahkan metode untuk mendapatkan semua notifikasi
        Task<List<Notification>> GetAllNotificationsAsync(string userId);
    }
}
