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
    public class AlertController : ControllerBase 
    {
        private readonly UneoWebContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;
        public const string TOPIC_NOTIFY_UPDATE_ALERT = "web/notify/update/alert";

        public AlertController(UneoWebContext uneoWebContext, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _context = uneoWebContext;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        // GET: api/Alert
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Alert>>> GetAlerts()
        {
            var alerts = await _context.Alerts.ToListAsync();
            var alertDtos = _mapper.Map<IEnumerable<AlertGetDto>>(alerts);
            return Ok(alertDtos);
        }

        // GET: api/Alert/{id}
        [HttpGet("{patientid}")]
        public async Task<IActionResult> GetAlert(string patientid)
        {
            var alert = await _context.Alerts.FirstOrDefaultAsync(a => a.Patientid == patientid);
            if (alert == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Alert not found."
                });
            }

            var alertDto = _mapper.Map<AlertGetDto>(alert);
            return Ok(alertDto);
        }

        // POST: api/Alert
        [HttpPost]
        public async Task<IActionResult> CreateAlert([FromBody] AlertPostDto alertDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var alert = _mapper.Map<Alert>(alertDto);
            alert.Alertguid = Guid.NewGuid(); 
            var existingAlert = await _context.Alerts.AnyAsync(a => a.Patientid == alert.Patientid);
            if (existingAlert)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"A Alert with this ID already exists.."
                });
            }

            await _context.Alerts.AddAsync(alert);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                // 使用 NotificationHub 發送消息
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", TOPIC_NOTIFY_UPDATE_ALERT, $"{alert.Alertguid}");
                return Ok(new ResponseCodeOK());
            }
            else
            {
                return Ok(new ResponseCodeNG());
            }
        }

        // PUT: api/Alert/{id}
        [HttpPut("{patientid}")]
        public async Task<IActionResult> UpdateAlert(string patientid, [FromBody] AlertPutDto alertDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var alert = await _context.Alerts.FirstOrDefaultAsync(a => a.Patientid == patientid);
            if (alert == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Alert not found."
                });
            }

            _mapper.Map(alertDto, alert);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                // 使用 NotificationHub 發送消息
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", TOPIC_NOTIFY_UPDATE_ALERT, $"{alert.Alertguid}");
                return Ok(new ResponseCodeOK());
            }
            else
            {
                return Ok(new ResponseCodeNG());
            }
        }

        // DELETE: api/Alert/{id}
        [HttpDelete("{patientid}")]
        public async Task<IActionResult> DeleteAlert(string patientid)
        {
            var alert = await _context.Alerts.FirstOrDefaultAsync(a => a.Patientid == patientid);
            if (alert == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Alert not found."
                });
            }

            _context.Alerts.Remove(alert);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                // 使用 NotificationHub 發送消息
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", TOPIC_NOTIFY_UPDATE_ALERT, $"{alert.Alertguid}");
                return Ok(new ResponseCodeOK());
            }
            else
            {
                return Ok(new ResponseCodeNG());
            }
        }
    }
}
