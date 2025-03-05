using System.Globalization;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;
using UMonitorWebAPI.Dtos;
using UMonitorWebAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UMonitorWebAPI.Controllers
{
    [ApiController]
    [Route("api/db/[controller]")]
    public class ErrorlogController : ControllerBase
    {
        private readonly UneoWebContext _context;
        private readonly IMapper _mapper;
        public ErrorlogController(UneoWebContext uneoWebContext, IMapper mapper)
        {
            _context = uneoWebContext;
            _mapper = mapper;
        }

        //GET: api/<ErrorlogController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Errorlog>>> GetErrorlog()
        {
            var utc24HoursAgo = DateTime.UtcNow.AddHours(-24);
            var twentyFourHoursAgo = DateTime.SpecifyKind(utc24HoursAgo, DateTimeKind.Unspecified); // 設置為 Unspecified

            var latestErrorlogsQuery = _context.Errorlogs
                .Where(n => !string.IsNullOrEmpty(n.Deviceid))
                .Where(n => n.Logtime >= twentyFourHoursAgo)
                .GroupBy(n => n.Deviceid)
                .Select(g => g.OrderByDescending(n => n.Logtime).First().Guid); // Select only the latest record's Guid

            var errorlogs = await _context.Errorlogs
                .Where(n => latestErrorlogsQuery.Contains(n.Guid)) // Filter logs to include only latest per device
                .Join(
                    _context.Devices, // Join with Devices table
                    errorlog => errorlog.Deviceid, // FK in errorlog
                    device => device.Deviceid, // PK in devices
                    (errorlog, device) => new ErrorlogGetDto
                    {
                        Guid = errorlog.Guid,
                        Deviceid = errorlog.Deviceid!,
                        Logtype = errorlog.Logtype!,
                        Log = errorlog.Log!,
                        Logtime = errorlog.Logtime,
                        CheckStatus = errorlog.CheckStatus,
                        DeviceType = device.Devicetype // Fetch DeviceType from Devices table
                    })
                .OrderByDescending(dto => dto.Logtime) // Sort by Logtime in descending order
                .ToListAsync();

            return Ok(errorlogs);
        }

        //// GET api/<ErrorlogController>/5
        [HttpGet("{guid}")]
        public async Task<IActionResult> GetErrorlog(Guid guid)
        {
            var errorlog = await _context.Errorlogs.FindAsync(guid);
            if (errorlog == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Notification not found."
                });
            }

            var errorlogDto = _mapper.Map<ErrorlogGetDto>(errorlog);
            return Ok(errorlogDto);
        }

        // POST api/<ErrorlogController>
        [HttpPost("filter")]
        [SwaggerRequestExample(typeof(ErrorlogFilterRequest), typeof(ErrorlogFilterRequestBodyExample))]
        public async Task<IActionResult> GetErrorlogByFilter([FromBody] ErrorlogFilterRequest filterRequest)
        {
            if (filterRequest == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Filter request cannot be null."
                });
            }

            if (!string.IsNullOrEmpty(filterRequest.Deviceid)
                && filterRequest.StartTime.HasValue
                && filterRequest.EndTime.HasValue)
            {
                if (!DateTime.TryParseExact(filterRequest.StartTime.Value.ToString("yyyy-MM-ddTHH:mm:ss"), "yyyy-MM-ddTHH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startTime))
                {
                    return BadRequest(new { message = "Invalid StartTime format. Expected format: yyyy-MM-ddTHH:mm:ss" });
                }
                if (!DateTime.TryParseExact(filterRequest.EndTime.Value.ToString("yyyy-MM-ddTHH:mm:ss"), "yyyy-MM-ddTHH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endTime))
                {
                    return BadRequest(new { message = "Invalid EndTime format. Expected format: yyyy-MM-ddTHH:mm:ss" });
                }
                var errorlogs = _context.Errorlogs
                    .Where(n => n.Deviceid == filterRequest.Deviceid)
                    .Where(n => n.Logtime >= filterRequest.StartTime.Value
                            && n.Logtime <= filterRequest.EndTime.Value).AsQueryable();
                if (!await errorlogs.AnyAsync())
                {
                    return Ok(new ResponseCodeNG
                    {
                        Code = -1,
                        Message = $"No rawdata is found for current time interval."
                    });
                }
                // Map the data to ErrorlogGetDto
                var errorlogDtos = _mapper.Map<IEnumerable<ErrorlogGetDto>>(errorlogs);
                //return Ok(errorlogDtos);
                // Generate CSV
                var sortedTime_errorlogDtos = errorlogDtos.OrderByDescending(dto => dto.Logtime); // sort with log time in descending order
                var csvData = GenerateCsv(sortedTime_errorlogDtos);
                try
                {
                    // Save CSV to a specified location
#if DEBUG
                    var downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

#else
                    var downloadsFolder = Path.GetTempPath();
                    if (!Directory.Exists(downloadsFolder))
                    {
                        Directory.CreateDirectory(downloadsFolder);
                    }
#endif

                    var fileName = $"{filterRequest.Deviceid}_Errorlog_{filterRequest.StartTime.Value:yyyyMMdd_HHmmss}_to_{filterRequest.EndTime.Value:yyyyMMdd_HHmmss}.csv";
                    var filePath = Path.Combine(downloadsFolder, fileName);
                    await System.IO.File.WriteAllTextAsync(filePath, csvData, new System.Text.UTF8Encoding(true));

                    // Create a downloadable link
                    var fileUrl = Url.Action("DownloadFile", "api/db/Errorlog", new { fileName }, Request.Scheme);

                    if (System.IO.File.Exists(filePath))
                    {
                        var fileBytes = System.IO.File.ReadAllBytes(filePath);
                        return File(fileBytes, "application/octet-stream", fileName);

                        //return Ok(new
                        //{
                        //    Code = 0,
                        //    Message = "CSV file generated successfully and save in Downloads folder.",
                        //    //DownloadUrl = fileUrl
                        //});
                    }
                    else
                    {
                        return NotFound("File not found.");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception (use a proper logging library in production)
                    Console.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");
                    return StatusCode(500, new { message = ex.Message, ex.StackTrace });
                }
            }
            else
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Filter request cannot be null."
                });
            }
        }
        private string GenerateCsv(IEnumerable<ErrorlogGetDto> errorlogDtos)
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Guid,DeviceId,LogType,Log,LogTime");

            foreach (var dto in errorlogDtos)
            {
                csvBuilder.AppendLine($"{dto.Guid},{dto.Deviceid},{dto.Logtype},{EscapeCsvValue(dto.Log)},{dto.Logtime}");
            }

            return csvBuilder.ToString();
        }
        // handle string value with "," that cause duplicate cell in CSV
        private string EscapeCsvValue(object value)
        {
            if (value == null) return string.Empty; // Handle null values

            var stringValue = value.ToString();

            // Escape double quotes by replacing " with ""
            if (stringValue.Contains("\""))
            {
                stringValue = stringValue.Replace("\"", "\"\"");
            }

            // Enclose in double quotes if the value contains a comma, double quote, or newline
            if (stringValue.Contains(",") || stringValue.Contains("\"") || stringValue.Contains("\n"))
            {
                stringValue = $"\"{stringValue}\"";
            }

            return stringValue;
        }

        //[HttpGet("DownloadFile")]
        //public IActionResult DownloadFile(string fileName)
        //{
        //    var downloadsFolder = Path.GetTempPath(); // Ensure this matches where the file is saved
        //    var filePath = Path.Combine(downloadsFolder, fileName);

        //    if (System.IO.File.Exists(filePath))
        //    {
        //        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        //        return File(fileBytes, "application/octet-stream", fileName);
        //    }
        //    else
        //    {
        //        return NotFound("File not found.");

        //    }
        //}

        // PUT api/<ErrorlogController>/5
        [HttpPut("{guid}")]
        public async Task<IActionResult> UpdateErrorlog(Guid guid, [FromBody] ErrorlogPutDto errorlogDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var errorlog = await _context.Errorlogs.FindAsync(guid);
            if (errorlog == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Notification not found."
                });
            }
            _mapper.Map(errorlogDto, errorlog);
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

        //// DELETE api/<ErrorlogController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
