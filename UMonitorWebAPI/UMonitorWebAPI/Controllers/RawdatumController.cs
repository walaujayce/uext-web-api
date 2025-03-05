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
    public class RawdatumController : ControllerBase
    {
        private readonly UneoWebContext _context;
        private readonly IMapper _mapper;
        public RawdatumController(UneoWebContext uneoWebContext, IMapper mapper)
        {
            _context = uneoWebContext;
            _mapper = mapper;
        }

        // GET: api/<RawdatumController>
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Rawdatum>>> GetRawdatums()
        //{
        //    var utcNow = DateTime.UtcNow.AddHours(-3);
        //    var twentyFourHoursAgo = DateTime.SpecifyKind(utcNow, DateTimeKind.Unspecified); // 設置為 Unspecified

        //    var rawdatums = await _context.Rawdata
        //        .Where(n => n.Createdat >= twentyFourHoursAgo) // 篩選 CreateDate 大於等於 24 小時前
        //        .ToListAsync();
        //    var rawdatumDtos = _mapper.Map<IEnumerable<RawdatumGetDto>>(rawdatums);
        //    return Ok(rawdatumDtos);
        //}

        //GET api/<RawdatumController>/5
        //[HttpGet("{deviceid}")]
        //public async Task<ActionResult<IEnumerable<Rawdatum>>> GetRawdatums(string deviceid)
        //{
        //    var utcNow = DateTime.UtcNow.AddHours(-1);
        //    var twentyFourHoursAgo = DateTime.SpecifyKind(utcNow, DateTimeKind.Unspecified); // 設置為 Unspecified
        //    using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(0.5))) // Set a timeout of 30 seconds
        //    {
        //        try
        //        {
        //            var rawdatums = await _context.Rawdata
        //            .Where(n => n.Deviceid == deviceid)
        //            .Where(n => n.Createdat >= twentyFourHoursAgo) // 篩選 CreateDate 大於等於 24 小時前
        //            .ToListAsync();

        //            if (rawdatums == null)
        //            {
        //                return Ok(new ResponseCodeNG
        //                {
        //                    Code = -1,
        //                    Message = $"Rawdata not found."
        //                });
        //            }

        //            var rawdatumDtos = _mapper.Map<IEnumerable<RawdatumGetDto>>(rawdatums);
        //            return Ok(rawdatumDtos);
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            // The operation was canceled due to exceeding the timeout
        //            return Ok(new ResponseCodeNG
        //            {
        //                Code = 99,
        //                Message = "Request spend too much time."
        //            });
        //        }
        //    }
        //}

        // POST api/<RawdatumController>
        [HttpPost("filter")]
        [SwaggerRequestExample(typeof(RawdatumFilterRequest), typeof(RawdatumFilterRequestBodyExample))]
        public async Task<IActionResult> GetRawdatumByFilter([FromBody] RawdatumFilterRequest filterRequest)
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
                var rawdatums =  _context.Rawdata
                    .Where(n => n.Deviceid == filterRequest.Deviceid)
                    .Where(n => n.Createdat >= filterRequest.StartTime.Value
                            && n.Createdat <= filterRequest.EndTime.Value).AsQueryable();
                if (!await rawdatums.AnyAsync())
                {
                    return Ok(new ResponseCodeNG
                    {
                        Code = -1,
                        Message = $"No rawdata is found for current time interval."
                    });
                }
                // Map the data to RawdatumGetDto
                var rawdatumDtos = _mapper.Map<IEnumerable<RawdatumGetDto>>(rawdatums);
                //return Ok(rawdatumDtos);
                // Generate CSV
                var sortedTime_rawdatumDtos = rawdatumDtos.OrderByDescending(dto => dto.Createdat); // sort with Createdate in descending order
                var csvData = GenerateCsv(sortedTime_rawdatumDtos);
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
                var fileName = $"{filterRequest.Deviceid}_Rawdatum_{filterRequest.StartTime.Value:yyyyMMdd_HHmmss}_to_{filterRequest.EndTime.Value:yyyyMMdd_HHmmss}.csv";
                var filePath = Path.Combine(downloadsFolder, fileName);
                await System.IO.File.WriteAllTextAsync(filePath, csvData);

                // Create a downloadable link
                var fileUrl = Url.Action("DownloadFile", "api/db/Rawdatum", new { fileName }, Request.Scheme);

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
        private string GenerateCsv(IEnumerable<RawdatumGetDto> rawdatumDtos)
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Rawid,DeviceId,Data,Createdat,Devicetype,FrameId");

            foreach (var dto in rawdatumDtos)
            {
                csvBuilder.AppendLine($"{dto.Rawid},{dto.Deviceid},{dto.Data},{dto.Createdat},{dto.Devicetype},{dto.Frameid}");
            }

            return csvBuilder.ToString();
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

        // PUT api/<RawdatumController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE api/<RawdatumController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
