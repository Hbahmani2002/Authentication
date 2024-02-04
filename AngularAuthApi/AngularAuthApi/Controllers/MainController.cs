using AngularAuthApi.Context;
using AngularAuthApi.Core.Repository;
using AngularAuthApi.Core.Services;
using AngularAuthApi.Helpers;
using AngularAuthApi.Infrastructure.Services;
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
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AngularAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        
        private readonly IApiService _apiservice;
            private readonly IApiRepository _apirepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<MainController> _logger;
        public MainController(ILogger<MainController> logger,IApiService apiservice,IApiRepository apirepository, IWebHostEnvironment webHostEnvironment)
        {
          
            _apiservice = apiservice;
            _apirepository = apirepository;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;

        }
        
        [Authorize] 
        [HttpGet]
        public async Task<IActionResult> GetAllFiles()
        {
            return  Ok(await _apiservice.GetAllFiles());
        }
        [Authorize]
        [HttpGet("downloadFile/{path}")]
        public async  Task<IActionResult> DownloadFile(string path)
        {
            path = _webHostEnvironment.ContentRootPath+"\\Docs\\"+path;
            if (System.IO.File.Exists(path))
            {
                var fileBytes = System.IO.File.ReadAllBytes(path);
                return File(fileBytes, "application/pdf", "file.pdf");
            }
            else
            {
                return NotFound();
            }
        }


    }
}
