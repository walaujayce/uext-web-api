using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMonitorWebAPI.Models;

namespace UMonitorWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InitDefaultValueController : ControllerBase
    {
        private readonly UneoWebContext _context;
        private readonly IMapper _mapper;
        public InitDefaultValueController(UneoWebContext uneoWebContext, IMapper mapper)
        {
            _context = uneoWebContext;
            _mapper = mapper;
        }

        // POST: api/Section/InitializeData
        [HttpPost("InitializeData")]
        public async Task<IActionResult> InitializeData()
        {
            // 檢查資料是否已存在
            if (_context.Sections.Any() || _context.Floors.Any())
            {
                return BadRequest("Default data already exists.");
            }

            // 新增 Section 資料
            var sections = new List<Section>
            {
                new Section { Sectionid = 1, Description = "Zone A" },
                new Section { Sectionid = 2, Description = "Zone B" },
                new Section { Sectionid = 3, Description = "Zone C" },
                new Section { Sectionid = 4, Description = "Zone D" },
                new Section { Sectionid = 5, Description = "Zone E" }
            };

            // 新增 Floor 資料
            var floors = new List<Floor>
            {
                new Floor { Floorid = 1, Description = "1F" },
                new Floor { Floorid = 2, Description = "2F" },
                new Floor { Floorid = 3, Description = "3F" },
                new Floor { Floorid = 4, Description = "4F" },
                new Floor { Floorid = 5, Description = "5F" },
                new Floor { Floorid = 6, Description = "6F" },
                new Floor { Floorid = 7, Description = "7F" },
                new Floor { Floorid = 8, Description = "8F" },
                new Floor { Floorid = 9, Description = "9F" }
            };

            // 將資料新增至資料庫
            await _context.Sections.AddRangeAsync(sections);
            await _context.Floors.AddRangeAsync(floors);

            // 儲存變更
            await _context.SaveChangesAsync();

            return Ok("Default data initialized successfully.");
        }
    }
}
