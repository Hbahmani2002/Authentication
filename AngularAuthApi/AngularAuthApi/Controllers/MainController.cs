using AngularAuthApi.Context;
using AngularAuthApi.Core.Repository;
using AngularAuthApi.Core.Services;
using AngularAuthApi.Helpers;
using AngularAuthApi.Infrastructure.Services;
using AngularAuthApi.Models;
using AngularAuthApi.Models.Dto;
using AngularAuthApi.UriliryService;
using Azure.Core;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
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
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Drawing;

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
        [Authorize]
        [HttpGet("searchFiles/{searchString}")]
        public async Task<IActionResult> SearchFiles(string searchString)
        {
            try
            {
                string directoryPath = _webHostEnvironment.ContentRootPath + "\\Docs\\";
                List<string> matchingFiles = new List<string>();
                List<string> matchingFilesDocx = new List<string>();
                List<string> filesContainingString = SearchPdfFiles(directoryPath, searchString);

                
                foreach (var filePath in filesContainingString)
                {
                    matchingFiles.Add(filePath);
                }

                
                return Ok(matchingFiles);
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        static List<string> SearchPdfFiles(string directoryPath, string searchString)
        {
            List<string> matchingFiles = new List<string>();

            try
            {
                // Get all PDF files in the directory and its subdirectories
                string[] files = Directory.GetFiles(directoryPath, "*.pdf", SearchOption.AllDirectories);

                foreach (var filePath in files)
                {
                    // Search for the string in the PDF file
                    if (PdfContainsString(filePath, searchString))
                    {
                        matchingFiles.Add(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return matchingFiles;
        }

        static bool PdfContainsString(string filePath, string searchString)
        {
            try
            {
                using (PdfReader pdfReader = new PdfReader(filePath))
                {
                    using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
                    {
                        for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
                        {
                            // Extract text from the PDF page
                            string pageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(pageNum));

                            // Check if the string is present in the page text
                            if (pageText.Contains(searchString))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing PDF file {filePath}: {ex.Message}");
            }

            return false;
        }
       
    }
}
