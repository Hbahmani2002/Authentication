using AngularAuthApi.Models;
using AngularAuthApi.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace AngularAuthApi.Core.Services
{
    public interface IUserService
    {
        Task<User> Authenticate([FromBody] User userObj);
        Task<List<User>> GetAllUsers();
        Task<User> Getphotobyusername(string username);
        Task<User> Refresh(string username);
        Task<User> RegisterUser([FromBody] User userObj);
        Task<User> ResetPassword(ResetPasswordDto resetPasswordDto);
        Task<User> SendEmail(string email);
        public Task<bool> CheckUsernameExist(string username);
        public Task<bool> CheckEmailExist(string email);
    }
}