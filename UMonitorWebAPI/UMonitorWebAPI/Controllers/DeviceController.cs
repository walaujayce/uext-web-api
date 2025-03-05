using AutoMapper;
using Microsoft.AspNetCore.Http;
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
    public class DeviceController : ControllerBase
    {
        private readonly UneoWebContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;
        public const string TOPIC_NOTIFY_UPDATE_DEVIICE = "web/notify/update/device";

        public DeviceController(UneoWebContext uneoWebContext, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _context = uneoWebContext;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        // GET: api/Device
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceGetDto>>> GetDevice()
        {
            var devices = await _context.Devices.ToListAsync();
            var deviceDtos = _mapper.Map<List<DeviceGetDto>>(devices);
            return Ok(deviceDtos);
        }

        // GET: api/Device/{deviceid}
        [HttpGet("{deviceid}")]
        public async Task<IActionResult> GetDevice(string deviceid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var device = await _context.Devices.FindAsync(deviceid);

            if (device != null)
            {
                return Ok(device);
            }
            else 
            {
                return Ok(Empty);
            }
          
        }

        // POST: api/Device
        [HttpPost]
        public async Task<IActionResult> CreateDevice([FromBody] DevicePostDto deviceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var device = await _context.Devices.FirstOrDefaultAsync(d => d.Deviceid == deviceDto.Macaddress);

                if (device != null)
                {
                    device.Updatedat = DateTime.Now;
                    _mapper.Map(deviceDto, device);
                }
                else 
                {
                    device = _mapper.Map<Device>(deviceDto);
                    device.Deviceid = device.Macaddress;
                    device.Updatedat = DateTime.Now;
                    await _context.Devices.AddAsync(device);
                }

                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    // 使用 NotificationHub 發送消息
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", TOPIC_NOTIFY_UPDATE_DEVIICE, $"{device.Deviceid}");
                    return Ok(new ResponseCodeOK());
                }
                else
                {
                    return Ok(new ResponseCodeNG());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Ok();
            }
        }

        // PUT: api/Device/{id}
        [HttpPut("{deviceid}")]
        public async Task<IActionResult> UpdateDevice(string deviceid, [FromBody] DevicePutDto deviceDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var device = await _context.Devices
                .Include(d => d.Patients) // Include the related Patients
                .SingleOrDefaultAsync(d => d.Deviceid == deviceid);

            if (device == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Device with ID {deviceid} not found."
                });
            }

            device.Updatedat = DateTime.Now;
            _mapper.Map(deviceDto, device);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                // 使用 NotificationHub 發送消息
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", TOPIC_NOTIFY_UPDATE_DEVIICE, $"{device.Deviceid}");
                return Ok(new ResponseCodeOK());
            }
            else
            {
                return Ok(new ResponseCodeNG());
            }
        }

        // PUT: api/DeviceStatus/{id}
        [HttpPut("Status/{deviceid}")]
        public async Task<IActionResult> UpdateDeviceStatus(string deviceid, [FromBody] DeviceStatusPutDto deviceStatusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var device = await _context.Devices
                .Include(d => d.Patients) // Include the related Patients
                .SingleOrDefaultAsync(d => d.Deviceid == deviceid);

            if (device == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Device with ID {deviceid} not found."
                });
            }

            device.Updatedat = DateTime.Now;
            _mapper.Map(deviceStatusDto, device);
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

        // DELETE: api/Device/{id}
        [HttpDelete("{deviceid}")]
        public async Task<IActionResult> DeleteDevice(string deviceid)
        {
            var device = await _context.Devices.FindAsync(deviceid);

            if (device == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Device with ID {deviceid} not found."
                });
            }

            _context.Devices.Remove(device);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                // 使用 NotificationHub 發送消息
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", TOPIC_NOTIFY_UPDATE_DEVIICE, $"{device.Deviceid}");
                return Ok(new ResponseCodeOK());
            }
            else
            {
                return Ok(new ResponseCodeNG());
            }
        }


        // POST: api/Device/Filter
        [HttpPost("filter")]
        public async Task<IActionResult> GetDevicesByFilter([FromBody] DeviceFilterRequest filterRequest)
        {
            if (filterRequest == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Filter request cannot be null."
                });
            }

            var devices = _context.Devices.AsQueryable();
            if (!string.IsNullOrEmpty(filterRequest.Floor) && !string.IsNullOrEmpty(filterRequest.Section))
            {
                devices = devices.Where(d => d.Section == filterRequest.Section && d.Floor == filterRequest.Floor);
            }
            else if (!string.IsNullOrEmpty(filterRequest.Floor) && string.IsNullOrEmpty(filterRequest.Section))
            {
                devices = devices.Where(d => d.Floor == filterRequest.Floor);
            }
            else if (!string.IsNullOrEmpty(filterRequest.Section) && string.IsNullOrEmpty(filterRequest.Floor))
            {
                devices = devices.Where(d => d.Section == filterRequest.Section);
            }
            else
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Both Floor and Section cannot be empty."
                });
            }


            if (devices == null || !devices.Any())
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"No devices found for Floor '{filterRequest.Floor}' and Section '{filterRequest.Section}'."
                });
            }


            var deviceGetDtos = _mapper.Map<IEnumerable<DeviceGetDto>>(devices);

            return Ok(deviceGetDtos);
        }
    }
}
