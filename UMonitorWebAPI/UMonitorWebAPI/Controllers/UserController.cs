using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using UMonitorWebAPI.Dtos;
using UMonitorWebAPI.Models;

namespace UMonitorWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UneoWebContext _context;
        private readonly IMapper _mapper;

        public UserController(UneoWebContext uneoWebContext, IMapper mapper)
        {
            _context = uneoWebContext;
            _mapper = mapper;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            var userDtos = _mapper.Map< IEnumerable<User>>(users);
            return Ok(userDtos);
        }

        // GET: api/User/5
        [HttpGet("{userid}")]
        public async Task<IActionResult> GetUser(string userid)
        {
            var user = await _context.Users.FindAsync(userid);
            if (user == null)
            {
                return Ok(new ResponseCodeNG
                {
                    Code = -1,
                    Message = $"User with ID {userid} not found."
                }
                );
            }
            var userDto = _mapper.Map<UserGetDto>(user);
            return Ok(userDto);
        }

        // POST: api/User
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserPostDto userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _mapper.Map<User>(userDto);
            var existingUser= await _context.Users.FindAsync(user.Userid);
            if (existingUser != null)
            {
                return Ok(new ResponseCodeNG
                    { 
                        Code = -1,
                        Message = "A User with this ID already exists." 
                    }
                ) ;
            }

            // 加密
            //user.Password = HashPassword(userDto.Password);
            user.Lastlogin = DateTime.Now;
            await _context.Users.AddAsync(user);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var newUser = await _context.Users.FindAsync(user.Userid);
                if (newUser == null)
                {
                    return NotFound($"User with ID {user.Userid} not found.");
                }
                var newUserDto = _mapper.Map<UserGetDto>(newUser);
                return Ok(newUserDto);
            }
            else
            {
                return Ok(new ResponseCodeNG());
            }
            //return CreatedAtAction(nameof(GetUser), new { userid = user.Userid }, user);
        }

        // POST: api/Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(userLoginDto.Userid);
            if (user == null)
                return Unauthorized(new ResponseCodeNG
                    {
                        Code = -1,
                        Message = "User not found" 
                    }
                );

            // 驗證密碼
            //var hashedInputPassword = HashPassword(userLoginDto.Password);
            if (user.Password != userLoginDto.Password)
                return Unauthorized(new ResponseCodeNG
                    {
                        Code = -1,
                        Message = "Invalid password" 
                    }
                );

            // Update last login time
            user.Lastlogin = DateTime.Now;
            _context.Users.Update(user); // Mark user entity as modified
            await _context.SaveChangesAsync(); // Persist changes to the database

            return Ok(new ResponseCodeOK());
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // PUT: api/User/5
        [HttpPut("{userid}")]
        public async Task<IActionResult> UpdateUser(string userid, [FromBody] UserPutDto userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(userid);

            if (user == null)
                return NotFound($"User with ID {userid} not found.");

            _mapper.Map(userDto, user);
            user.Lastlogin = DateTime.Now;
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

        // DELETE: api/User/5
        [HttpDelete("{userid}")]
        public async Task<IActionResult> DeleteUser(string userid)
        {
            var user = await _context.Users.FindAsync(userid);

            if (user == null)
                return NotFound($"User with ID {userid} not found.");

            _context.Users.Remove(user);
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
    }
}
