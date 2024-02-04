using AngularAuthApi.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace AngularAuthApi.Core.Services
{
    public interface IApiService
    {
        Task<List<Files>> GetAllFiles();
        
    }
}