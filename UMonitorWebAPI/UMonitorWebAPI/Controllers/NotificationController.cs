using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UMonitorWebAPI.Dtos;
using UMonitorWebAPI.Hubs;
using UMonitorWebAPI.Models;

namespace UMonitorWebAPI.Controllers
{
    [ApiController]
    [Route("api/db/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UneoWebContext _context;
        private readonly IMapper _mapper;
        public const string TOPIC_NOTIFY_UPDATE_NOTIFICATION = "web/notify/update/notification";


        public NotificationController(UneoWebContext uneoWebContext,
            IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
            _context = uneoWebContext;
            _mapper = mapper;
        }

        // GET: api/Notification
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            var utcNow = DateTime.UtcNow.AddHours(-24);
            var twentyFourHoursAgo = DateTime.SpecifyKind(utcNow, DateTimeKind.Unspecified); // 設置為 Unspecified

            // Get all notifications that are older than the latest one per Deviceid and set CheckStatus to true
            var notifications_sorted = await _context.Notifications
                .Where(n => n.PunchTime >= twentyFourHoursAgo && (n.CheckStatus == false || n.CheckStatus == null))
                .GroupBy(n => n.Deviceid)
                .Select(g => g.OrderByDescending(n => n.PunchTime).FirstOrDefault()) // Get the latest PunchTime for each Deviceid
                .ToListAsync();

            if (!notifications_sorted.Any())
                return Ok(new List<NotificationGetDto>());

            var deviceIds = notifications_sorted.Select(n => n.Deviceid).ToList();

            var olderNotifications = await _context.Notifications
                .Where(n => deviceIds.Contains(n.Deviceid) && n.CheckStatus == false)
                .ToListAsync();

            foreach (var notification in olderNotifications)
            {
                var latestPunchTime = notifications_sorted.FirstOrDefault(n => n.Deviceid == notification.Deviceid)?.PunchTime;
                if (notification.PunchTime < latestPunchTime)
                {
                    notification.CheckStatus = true; // Mark as checked
                }
            }

            // Save changes
            await _context.SaveChangesAsync();

            // Map and return all notifications
            var notifications = await _context.Notifications
                .Where(n => n.PunchTime >= twentyFourHoursAgo) // 篩選 CreateDate 大於等於 24 小時前
                .OrderByDescending(n => n.PunchTime)
                .ToListAsync();
            var notificationDtos = _mapper.Map<IEnumerable<NotificationGetDto>>(notifications);
            return Ok(notificationDtos);
        }

        // GET: api/Notification/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Notification not found."
                });
            }

            var notificationDto = _mapper.Map<NotificationGetDto>(notification);
            return Ok(notificationDto);
        }

        // POST: api/Notification
        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationPostDto notificationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var notification = _mapper.Map<Notification>(notificationDto);
            notification.CreateDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _context.Notifications.AddAsync(notification);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(new ResponseCodeOK());
            }
            else
            {
                return Ok(new ResponseCodeNG());
            }
        }

        // PUT: api/Notification/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(Guid id, [FromBody] NotificationPutDto notificationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Notification not found."
                });
            }
            notification.CreateDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            _mapper.Map(notificationDto, notification);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var notify = new Notify
                {
                    Id = id,
                    Macaddress = notification.Deviceid
                };

                string notifyJson = System.Text.Json.JsonSerializer.Serialize(notify);

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", TOPIC_NOTIFY_UPDATE_NOTIFICATION, notifyJson);
                return Ok(new ResponseCodeOK());

            }
            else
            {
                return Ok(new ResponseCodeNG());
            }
        }

        // DELETE: api/Notification/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Notification not found."
                });
            }

            _context.Notifications.Remove(notification);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(new ResponseCodeOK());
            }
            else
            {
                return Ok(new ResponseCodeNG());
            }
        }


    // 發送廣播訊息給所有連線的用戶
    [HttpPost("broadcast")]
    public async Task<IActionResult> BroadcastMessage([FromBody] NotificationMessage message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.User, message.Content);
        return Ok(new { Status = "Message sent to all clients" });
    }

 }

    public class NotificationMessage
    {
        public string User { get; set; }
        public string Content { get; set; }
    }

}
