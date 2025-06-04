using ConfirmMe.Models;
using ConfirmMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfirmMe.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        // Constructor untuk dependency injection
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // 🔹 GET: api/notifications/{userId}
        // Untuk mengambil semua notifikasi untuk user
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetAllNotifications(string userId)
        {
            var notifications = await _notificationService.GetAllNotificationsAsync(userId);
            return Ok(notifications);
        }

        // 🔹 GET: api/notifications/{userId}/unread
        // Untuk mengambil semua notifikasi yang belum dibaca oleh user
        [HttpGet("{userId}/unread")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetUnreadNotifications(string userId)
        {
            var unreadNotifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            return Ok(unreadNotifications);
        }

        // 🔹 PUT: api/notifications/{id}/mark-as-read
        // Untuk menandai notifikasi sebagai dibaca
        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            await _notificationService.MarkNotificationAsReadAsync(id);
            return Ok(new { message = "Notification marked as read" });
        }
    }
}
