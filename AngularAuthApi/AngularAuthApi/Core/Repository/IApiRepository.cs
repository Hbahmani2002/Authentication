using AngularAuthApi.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace AngularAuthApi.Core.Repository
{
    public interface IApiRepository
    {
        Task<List<Files>> GetAllFiles();
       
    }
}