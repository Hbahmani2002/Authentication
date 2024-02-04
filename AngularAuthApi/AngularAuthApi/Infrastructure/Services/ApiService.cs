using AngularAuthApi.Core.Repository;
using AngularAuthApi.Core.Services;
using AngularAuthApi.Helpers;
using AngularAuthApi.Models;
using AngularAuthApi.Models.Api;
using AngularAuthApi.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;

namespace AngularAuthApi.Infrastructure.Services
{
    public class ApiService : IApiService
    {

        private readonly IApiRepository apiRepository;

        public ApiService(IApiRepository _api)
        {
            apiRepository = _api;

        }

        public async Task<List<Files>> GetAllFiles()
        {
            return await apiRepository.GetAllFiles();
        }
   

    }
}
