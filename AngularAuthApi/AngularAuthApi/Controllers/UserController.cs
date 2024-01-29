using AngularAuthApi.Context;
using AngularAuthApi.Helpers;
using AngularAuthApi.Models;
using AngularAuthApi.Models.Dto;
using AngularAuthApi.UriliryService;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AngularAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailservice;
        public UserController(ApiDbContext apiDbContext,IConfiguration configuration,IEmailService emailService)
        {
            _context = apiDbContext;
            _configuration = configuration;
            _emailservice = emailService;
        }
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if(userObj == null)
                return BadRequest();
            var user= await _context.Users.FirstOrDefaultAsync(x=> x.UserName==userObj.UserName);
            if(user == null)
                return NotFound(new { Message = "User not found!" });
            if (!PasswordHasher.VerifyPassword(userObj.Password,user.Password))
                return BadRequest(new { Message = "Password is Incorrect" });
            user.Token = CreateJwt(user);
            var newAccessToken = user.Token;
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpireTime = DateTime.Now.AddDays(5);
            await _context.SaveChangesAsync();
            return Ok(new tokenApiDto
            {
                AccessToken =newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest(new
                {
                    message = "your request is wrong"
                });
            //check username
            if (await CheckUsernameExist(userObj.UserName))
                return BadRequest(new
                {
                    message = "this username exist in your database"
                });

            //check email 

            if (await CheckEmailExist(userObj.Email))
                return BadRequest(new
                {
                    message = "this Email exist in your database"
                });

            //check password strength
            var pass = CheckPasswordStrength(userObj.Password);
            if(!string.IsNullOrEmpty(pass))
                return BadRequest(new
                {
                    message = pass
                });
            userObj.Password = PasswordHasher.HashPassword(userObj.Password);
            userObj.Role = "User";
            userObj.Token = "";
            await _context.Users.AddAsync(userObj);
            await _context.SaveChangesAsync();
            return Ok(new {
                message = "user Registered!!"
            });
        }
        [Authorize] 
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            return  Ok(await _context.Users.ToListAsync());
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(tokenApiDto tokenapidto)
        {
            if (tokenapidto == null)
                return BadRequest("Invalid Client Request");
            var accessToken = tokenapidto.AccessToken;
            var refreshToken = tokenapidto.RefreshToken;
            var principal = GetPrincipleFromExpiredToken(accessToken);
            var username = principal.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(a => a.UserName == username);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpireTime <= DateTime.Now)
                return BadRequest("Invalid Request");
            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _context.SaveChangesAsync();
            return Ok(new tokenApiDto
            {
                AccessToken =newAccessToken,
                RefreshToken= newRefreshToken
            });



        }
        [Authorize]
        [HttpPost("getphotobyusername/{username}")]
        public async Task<IActionResult> Getphotobyusername(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(a => a.UserName == username);
            if (user is null)
                return BadRequest(new
                {
                    StatusCode = 404,
                    message = "USer Not Found!!!"
                });
            return Ok(new
            {
                StatusCode = 200,
                message = user.UserName
            });
        }


        [HttpPost("send-reset-email/{email}")]
        public async Task<IActionResult> SendEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(a => a.Email == email);
            if (user is null)
                return BadRequest(new
                {
                    statusCode = 404,
                    message = "Email does not exist"
                });
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);
            user.ResetPasswordToken = emailToken;
            user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
            var from = _configuration["EmailSettings:From"];
            var emailModel = new EmailModel(email, "Reset Password!!", EmailBody.EmailStringBody(email, emailToken));
            _emailservice.sendEmail(emailModel);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Email Sent !!"
            }); 

        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var newToken = resetPasswordDto.EmailToken.Replace(" ", "+");
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(a => a.Email == resetPasswordDto.Email);
            if (user is null)
                return BadRequest(new
                {
                    statusCode = 404,
                    Message = "Email does not exist"
                });
            var tokenCode = user.ResetPasswordToken;
            DateTime emailTokenExpiey = user.ResetPasswordExpiry;
            if(tokenCode != resetPasswordDto.EmailToken || emailTokenExpiey<DateTime.Now)
            {
                return BadRequest(new
                {
                    StatusCode=400,
                    Message = "invalid reset link"
                });
            }
            user.Password = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new
            {
                StatusCode=200,
                Message = "Password Resdet Successfull!!"
            });
        }
        private Task<bool> CheckUsernameExist(string username)
            => _context.Users.AnyAsync(x => x.UserName == username);
        private Task<bool> CheckEmailExist(string email)
           => _context.Users.AnyAsync(x => x.Email == email);
        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if(password.Length < 8)
               sb.Append("Minium Password length must be 8"+Environment.NewLine);
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password,"[A-Z]") && Regex.IsMatch(password,"[0-9]")))
                sb.Append("Password must be AlphabetNumeric"+Environment.NewLine);
            if (!Regex.IsMatch(password, "[!,@,#,$,%,^,&,*,(,),+,-,_,?]"))
                sb.Append("Password must be contain special characters" + Environment.NewLine);
            return sb.ToString();
              

        }
        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("9aE$5sG#2vP!1qW&8mZ*4cX@7oL%3iB+6dY");
            var Identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role,user.Role),
                new Claim(ClaimTypes.Name,user.UserName)
            });
            var credential = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = Identity,
                Expires = DateTime.Now.AddSeconds(10),
                SigningCredentials = credential
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }
        private string CreateRefreshToken()
        {
            var tokenBytes =RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);
            var toknInUser = _context.Users.Any(a=> a.RefreshToken==refreshToken);
            if (toknInUser)
                return CreateRefreshToken();
            return refreshToken;
        }
        private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes("9aE$5sG#2vP!1qW&8mZ*4cX@7oL%3iB+6dY");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)

            };
            var tokenHandeler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandeler.ValidateToken(token,tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("This Is Invalid Token");
            return principal;

        }
    }
}
