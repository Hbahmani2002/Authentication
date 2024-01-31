using AngularAuthApi.Context;
using AngularAuthApi.Core.Repository;
using AngularAuthApi.Models;
using AngularAuthApi.Models.Dto;

using AngularAuthApi.Helpers;


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
using Microsoft.Extensions.Configuration;


namespace AngularAuthApi.Infrastructure.Repository
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly ApiDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailservice;
        public UserRepository(ApiDbContext _con,IConfiguration configuration, IEmailService emailService) : base(_con)
        {
            _context = _con;
            _configuration = configuration;
            _emailservice = emailService;
        }
        public async Task<User> Authenticate([FromBody] User userObj)
        {
            

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == userObj.UserName);
            user.Token = CreateJwt(user);
            var newAccessToken = user.Token;
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpireTime = DateTime.Now.AddDays(5);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<User> RegisterUser([FromBody] User userObj)
        {
            var user = new User
            {
                UserName = userObj.UserName,
                FirstName = userObj.FirstName,
                LastName = userObj.LastName,
                Password = userObj.Password,
                Email = userObj.Email,
                UserPhoto = userObj.UserPhoto
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;

        }
        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<User> Refresh(string username)
        {

            var user = await _context.Users.FirstOrDefaultAsync(a => a.UserName == username);
            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            _context.Users.UpdateRange(user);
            await _context.SaveChangesAsync();
            return user;



        }
        public async Task<User> Getphotobyusername(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(a => a.UserName == username);

            return user;
        }
        public async Task<User> SendEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(a => a.Email == email);
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);
            user.ResetPasswordToken = emailToken;
            user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
            var from = _configuration["EmailSettings:From"];
            var emailModel = new EmailModel(email, "Reset Password!!", EmailBody.EmailStringBody(email, emailToken));
            _emailservice.sendEmail(emailModel);
            _context.Entry(user).State = EntityState.Modified;

            _context.Users.UpdateRange(user);
            await _context.SaveChangesAsync();
            return user;

        }
        public async Task<User> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var newToken = resetPasswordDto.EmailToken.Replace(" ", "+");
            
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(a => a.Email == resetPasswordDto.Email);
            var tokenCode = user.ResetPasswordToken;
            DateTime emailTokenExpiey = user.ResetPasswordExpiry;
            user.Password = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return user;
        }
        public Task<bool> CheckUsernameExist(string username)
         => _context.Users.AnyAsync(x => x.UserName == username);
        public Task<bool> CheckEmailExist(string email)
           => _context.Users.AnyAsync(x => x.Email == email);
        public string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if (password.Length < 8)
                sb.Append("Minium Password length must be 8" + Environment.NewLine);
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]")))
                sb.Append("Password must be AlphabetNumeric" + Environment.NewLine);
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
            var credential = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
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
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);
            var toknInUser = _context.Users.Any(a => a.RefreshToken == refreshToken);
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
            var principal = tokenHandeler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("This Is Invalid Token");
            return principal;

        }
    }
}
