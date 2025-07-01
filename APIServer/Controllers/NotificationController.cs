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

        [EnableQuery]
        [HttpGet]
        public IActionResult GetAll()
        {
            var query = _notificationService.GetAllNotificationsQuery();
            return Ok(query); // Không await, không ToListAsync
        }

        [HttpGet("receiver/{receiverId}")]
        public async Task<IActionResult> GetByReceiverId(int receiverId)
        {
            var result = await _notificationService.GetNotificationsByReceiverIdAsync(receiverId);

            if (result == null || !result.Any())
                return NotFound("Không tìm thấy thông báo nào cho người dùng này.");

            return Ok(result);
        }


        [HttpGet("track")]
        public async Task<IActionResult> Track(int notificationId)
        {
            var result = await _notificationService.TrackNotificationAsync(notificationId);

            if (!result)
                return NotFound("Notification not found.");

            // Trả pixel 1x1 transparent png
            var pixel = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAoMBgF9+FiQAAAAASUVORK5CYII=");

            return File(pixel, "image/png");
        }

        [HttpPatch("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);

            if (!result)
                return NotFound("Notification not found");

            return NoContent(); // hoặc Ok() nếu bạn muốn
        }


    }
}
