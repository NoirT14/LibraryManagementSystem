using APIServer.DTO.Notification;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace APIServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/notifications
        [EnableQuery]
        [HttpGet]
        public IActionResult GetAll()
        {
            var query = _notificationService.GetAllNotificationsQuery();
            return Ok(query); // Trả về IQueryable hỗ trợ OData
        }

        // GET: api/notifications/receiver/{receiverId}?$filter=...&$orderby=...
        [EnableQuery]
        [HttpGet("receiver/{receiverId}")]
        public IActionResult GetByReceiverId(int receiverId)
        {
            var query = _notificationService
                .GetAllNotificationsQuery()
                .Where(n => n.ReceiverId == receiverId);

            return Ok(query); // Trả về IQueryable để OData hoạt động
        }

        // GET: api/notifications/track?notificationId=1
        [HttpGet("track")]
        public async Task<IActionResult> Track(int notificationId)
        {
            var result = await _notificationService.TrackNotificationAsync(notificationId);

            if (!result)
                return NotFound("Notification not found.");

            // Trả về ảnh pixel 1x1
            var pixel = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAoMBgF9+FiQAAAAASUVORK5CYII=");

            return File(pixel, "image/png");
        }

        // PATCH: api/notifications/{id}/mark-read
        [HttpPatch("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);

            if (!result)
                return NotFound("Notification not found");

            return NoContent();
        }
    }
}
