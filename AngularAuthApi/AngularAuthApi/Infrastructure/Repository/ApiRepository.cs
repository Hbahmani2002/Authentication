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
using AngularAuthApi.Models.Api;


namespace AngularAuthApi.Infrastructure.Repository
{
    public class ApiRepository : BaseRepository<Files>, IApiRepository
    {
        private readonly ApiDbContext _context;
        private readonly IConfiguration _configuration;
        public ApiRepository(ApiDbContext _con, IConfiguration configuration) : base(_con)
        {
            _context = _con;
            _configuration = configuration;
        }
        public async Task<List<Files>> GetAllFiles()
        {
            return await _context.Files.ToListAsync();
        }
        
    }
}
