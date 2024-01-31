using AngularAuthApi.Core.Repository;
using AngularAuthApi.Core.Services;
using AngularAuthApi.Helpers;
using AngularAuthApi.Models;
using AngularAuthApi.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;

namespace AngularAuthApi.Infrastructure.Services
{
    public class UserService : IUserService
    {

        private readonly IUserRepository userRepository;

        public UserService(IUserRepository _user)
        {
            userRepository = _user;
            
        }
        public async Task<User> Authenticate([FromBody] User userObj)
        {
            var user = await userRepository.Authenticate(userObj);
            return user;
        }
        public async Task<User> RegisterUser([FromBody] User userObj)
        {
            var user = await userRepository.RegisterUser(userObj);
            return user;
        }
        public async Task<List<User>> GetAllUsers()
        {
            return await userRepository.GetAllUsers();
        }
        public async Task<User> Refresh(string username)
        {
            return await userRepository.Refresh(username);
        }
        public async Task<User> Getphotobyusername(string username)
        {
            return await userRepository.Getphotobyusername(username);
        }
        public async Task<User> SendEmail(string email)
        {
            return await userRepository.SendEmail(email);

        }
        public async Task<User> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            return await userRepository.ResetPassword(resetPasswordDto);
        }
        public Task<bool> CheckUsernameExist(string username)
        => userRepository.CheckUsernameExist(username);
        public Task<bool> CheckEmailExist(string email)
           => userRepository.CheckEmailExist(email);
    }
}
