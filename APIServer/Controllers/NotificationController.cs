using APIServer.DTO.Notification;
using APIServer.Service;
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

     

            // PATCH: api/notifications/{id}/mark-read
            [HttpPatch("{id}/mark-read")]
            public async Task<IActionResult> MarkAsRead(int id)
            {
                var result = await _notificationService.MarkAsReadAsync(id);

                if (!result)
                    return NotFound("Notification not found");

                return NoContent();
            }

                [HttpGet("unread-count/{receiverId}")]
                public async Task<IActionResult> GetUnreadCount(int receiverId)
                {
                    var count = await _notificationService.CountUnreadNotificationsAsync(receiverId);
                    return Ok(count);
                }
    }

            

}
