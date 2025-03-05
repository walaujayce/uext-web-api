using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMonitorWebAPI.Models;

namespace UMonitorWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FloorController : ControllerBase
    {
        private readonly UneoWebContext _context;
        private readonly IMapper _mapper;

        public FloorController(UneoWebContext uneoWebContext, IMapper mapper)
        {
            _context = uneoWebContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetFloors()
        {
            var floors = await _context.Floors.ToArrayAsync();
            return Ok(floors);
        }

    }
}
