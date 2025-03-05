using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using UMonitorWebAPI.Dtos;

namespace UMonitorWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendEmailController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public SendEmailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("validate-email-token")]
        public IActionResult CheckTokenValidity([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Token is required.");
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:EmailKey"]));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                if (validatedToken is JwtSecurityToken jwtToken)
                {
                    var expiryDate = jwtToken.ValidTo;
                    if (expiryDate > DateTime.UtcNow)
                    {
                        return Ok(new { Valid = true, Expiry = expiryDate });
                    }
                    else
                    {
                        return Ok(new { Valid = false, Message = "Token has expired." });
                    }
                }
                return BadRequest("Invalid token format.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Valid = false, Message = ex.Message });
            }
        }

        [HttpPost("generate-email-token")]
        public IActionResult GenerateEmailToken([FromBody] ForgetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email is required.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:EmailKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, request.Email) }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString, Expires = tokenDescriptor.Expires });
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendForgetPasswordLink([FromBody] ForgetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Email and Message are required.");
            }

            //var claims = new List<Claim>
            //{
            //    new Claim(JwtRegisteredClaimNames.Email,request.Email)
            //};
            //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:EmailKey"]));

            //var jwt = new JwtSecurityToken
            //(
            //    claims: claims,
            //    expires: DateTime.UtcNow.AddMinutes(10),
            //    signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            //);

            //var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("UNEO", "jayce@uneotech.com"));
                emailMessage.To.Add(new MailboxAddress("", request.Email));
                emailMessage.Subject = "Reset Password Link (UNEO Bed Alarm System)";
                emailMessage.Body = new TextPart("plain")
                {
                    Text = request.Message
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate("jayce@uneotech.com", "zhsdqwhyikoyjspw");
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }

                return Ok(new { Message = "Email is sent successfully."});
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
