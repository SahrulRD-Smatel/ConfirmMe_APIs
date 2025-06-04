using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConfirmMe.Data;
using ConfirmMe.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfirmMe.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(string recipientId, string message, string type, int? relatedEntityId = null, string relatedEntityType = null, string urgencyLevel = "Medium", DateTime? expiryDate = null, string actionUrl = null)
        {
            var notification = new Notification
            {
                RecipientId = recipientId,
                Message = message,
                Type = type,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                UrgencyLevel = urgencyLevel,
                ExpiryDate = expiryDate,
                ActionUrl = actionUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientId == userId && !n.IsRead)
                .ToListAsync();
        }

        // Implementation of the missing method
        public async Task<List<Notification>> GetAllNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientId == userId)
                .ToListAsync();
        }
    }
}
