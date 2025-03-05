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
    public class RecordDataController : ControllerBase
    {
        private readonly UneoWebContext _context;
        private readonly IMapper _mapper;
        public RecordDataController(UneoWebContext uneoWebContext, IMapper mapper)
        {
            _context = uneoWebContext;
            _mapper = mapper;
        }
        //// GET: api/<RecordDataController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<RecordDataController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/<RecordDataController>
        [HttpPost("filter")]
        [SwaggerRequestExample(typeof(RecordDataFilterRequest), typeof(RecordDataFilterRequestBodyExample))]
        public async Task<IActionResult> GetRecordDatumByFilter([FromBody] RecordDataFilterRequest filterRequest)
        {
            if (filterRequest == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Filter request cannot be null."
                });
            }

            var recordDatums = _context.Recorddata.AsQueryable();
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

                recordDatums = recordDatums
                    .Where(n => n.Deviceid == filterRequest.Deviceid)
                    .Where(n => n.Recordtime >= filterRequest.StartTime.Value
                            && n.Recordtime <= filterRequest.EndTime.Value);
            }
            else
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"Filter request cannot be null."
                });
            }
            if (!await recordDatums.AnyAsync())
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"No rawdata is found for current time interval."
                });
            }

            // Map the data to RawdatumGetDto
            var recordDatumDtos = _mapper.Map<IEnumerable<RecordDataGetDto>>(recordDatums);
            //return Ok(recordDatumDtos);

            // Generate CSV
            var sortedTime_recordDatumDtos = recordDatumDtos.OrderByDescending(dto => dto.Recordtime); // sort with record time in descending order
            var csvData = GenerateCsv(sortedTime_recordDatumDtos);
            try
            {
                // Save CSV to a temporary location
#if DEBUG
            var downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

#else
                var downloadsFolder = Path.GetTempPath();
            if (!Directory.Exists(downloadsFolder))
            {
                Directory.CreateDirectory(downloadsFolder);
            }
#endif            
            var fileName = $"{filterRequest.Deviceid}_RecordData_{filterRequest.StartTime.Value:yyyyMMdd_HHmmss}_to_{filterRequest.EndTime.Value:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(downloadsFolder, fileName);
            await System.IO.File.WriteAllTextAsync(filePath, csvData);

            // Create a downloadable link
            var fileUrl = Url.Action("DownloadFile", "api/db/RecordData", new { fileName }, Request.Scheme);

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
        
        private string GenerateCsv(IEnumerable<RecordDataGetDto> recordDatumDtos)
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("DeviceId,RecordTime,StatusId,Duration,FrameId");

            foreach (var dto in recordDatumDtos)
            {
                csvBuilder.AppendLine($"{dto.Deviceid},{dto.Recordtime},{dto.Statusid},{dto.Duration},{dto.Frameid}");
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

        // PUT api/<RecordDataController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<RecordDataController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
