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
    public class PatientController : ControllerBase
    {
        private readonly UneoWebContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;
        public const string TOPIC_NOTIFY_UPDATE_PATIENT = "web/notify/update/patient";

        public PatientController(UneoWebContext uneoWebContext, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _context = uneoWebContext;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        // GET: api/Patient
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
        {
            var patients = await _context.Patients
                .Include(p => p.Device) 
                .ToListAsync();

            var patientDtos = patients.Select(patient => new PatientGetDto
            {
                Patientname = patient.Patientname,
                Patientid = patient.Patientid,
                Sex = patient.Sex,
                Birthday = patient.Birthday,
                Height = patient.Height,
                Weight = patient.Weight,
                Bed = patient.Device?.Bed ?? "",
                Section = patient.Device?.Section ?? "",
                Floor = patient.Device?.Floor ?? "",
                Deviceid = patient.Device?.Deviceid ?? string.Empty,
                Devicestatus = patient.Device?.Devicestatus ?? 0
            }).ToList();

            return Ok(patientDtos);
        }

        // GET: api/Patient/5
        [HttpGet("{patientid}")]
        public async Task<IActionResult> GetPatient(string patientid)
        {
            var patient = await _context.Patients
                .Include(p => p.Device)
                .FirstOrDefaultAsync(p => p.Patientid == patientid);

            if (patient == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"$\"Patient with ID {patientid} not found."
                });
            }

            var device = await _context.Devices.FindAsync(patient.Deviceid);
            var patientDto = _mapper.Map<PatientGetDto>(patient);
            patientDto.Bed = device?.Bed ?? "";
            patientDto.Section = device?.Section ?? "";
            patientDto.Floor = device?.Floor ?? "";
            return Ok(patientDto);
        }

        // POST: api/Patient
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] PatientPostDto patientDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = _mapper.Map<Patient>(patientDto);

            var existingPatient = await _context.Patients.FindAsync(patient.Patientid);
            if (existingPatient != null)
            {
                return Conflict(new { message = "A patient with this ID already exists." });
            }

            if (!string.IsNullOrEmpty(patient.Deviceid))
            {
                var device = await _context.Devices.FirstOrDefaultAsync(d => d.Deviceid == patient.Deviceid);
                if (device == null)
                {
                    return BadRequest(new { message = $"Device with ID '{patient.Deviceid}' does not exist." });
                }
                patient.Device = device;
            }

            await _context.Patients.AddAsync(patient);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                // 使用 NotificationHub 發送消息
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", TOPIC_NOTIFY_UPDATE_PATIENT, $"{patient.Patientid}");
                return Ok(new ResponseCodeOK());
            }
            else
            {
                return Ok(new ResponseCodeNG());
            }
        }

        // PUT: api/Patient/5
        [HttpPut("{patientid}")]
        public async Task<IActionResult> UpdatePatient(string patientid, [FromBody] PatientPutDto patientDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = await _context.Patients.FindAsync(patientid);

            if (patient == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"$\"Patient with ID {patientid} not found."
                });
            }


            _mapper.Map(patientDto,patient);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                // 使用 NotificationHub 發送消息
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", TOPIC_NOTIFY_UPDATE_PATIENT, $"{patient.Patientid}");
                return Ok(new ResponseCodeOK());
            }
            else
            {
                return Ok(new ResponseCodeNG());
            }
        }

        // DELETE: api/Patient/5
        [HttpDelete("{patientid}")]
        public async Task<IActionResult> DeletePatient(string patientid)
        {
            var patient = await _context.Patients.FindAsync(patientid);

            if (patient == null)
                return Ok(new ResponseCodeNG
                    {
                        Code = -1,
                        Message = $"Patient with ID {patientid} not found."
                    }
                );

            _context.Patients.Remove(patient);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                // 使用 NotificationHub 發送消息
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", TOPIC_NOTIFY_UPDATE_PATIENT, $"{patient.Patientid}");
                return Ok(new ResponseCodeOK());
            }
            else
            {
                return Ok(new ResponseCodeNG());
            }
        }

        // POST: api/Patient/Filter
        [HttpPost("filter")]
        public async Task<IActionResult> GetPatientByFilter([FromBody] PatientFilterRequest filterRequest)
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

            if (!await devices.AnyAsync())
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"No devices found for Floor '{filterRequest.Floor}' and Section '{filterRequest.Section}'."
                });
            }

            // Fetch patients with joined device details
            var patientsWithDevices = await _context.Patients
                .Where(p => devices.Select(d => d.Deviceid).Contains(p.Deviceid))
                .Join(
                    _context.Devices,                   // Join with Devices table
                    patient => patient.Deviceid,        // Match Deviceid in Patient
                    device => device.Deviceid,          // Match Deviceid in Device
                    (patient, device) => new            // Create a new object with both data
                    {
                        patient,
                        device
                    }
                )
                .ToListAsync();

            if (!patientsWithDevices.Any())
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = "No patients found for the specified filters."
                });
            }

            // Map the joined data to PatientGetDto
            var patientGetDtos = patientsWithDevices.Select(pd => new PatientGetDto
            {
                Patientname = pd.patient.Patientname,
                Patientid = pd.patient.Patientid,
                Sex = pd.patient.Sex,
                Birthday = pd.patient.Birthday,
                Height = pd.patient.Height,
                Weight = pd.patient.Weight,
                Bed = pd.device.Bed ?? "",                  
                Section = pd.device.Section ?? "",   
                Floor = pd.device.Floor ?? "",   
                Deviceid = pd.patient.Deviceid ?? "",
                Devicestatus = pd.device.Devicestatus 
            });
            // Map to PatientGetDto
            //var patientGetDtos = _mapper.Map<IEnumerable<PatientGetDto>>(patients);

            return Ok(patientGetDtos);

        }
    }
}
