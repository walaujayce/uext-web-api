using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMonitorWebAPI.Models;

namespace UMonitorWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SectionController : ControllerBase
    {
        private readonly UneoWebContext _context;
        private readonly IMapper _mapper;
        public SectionController(UneoWebContext uneoWebContext, IMapper mapper)
        {
            _context = uneoWebContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetSections()
        {
            var sections = await _context.Sections.ToListAsync();
            return Ok(sections);
        }

    }
}
