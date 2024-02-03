
using AngularAuthApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static AngularAuthApi.Models.WeatherForecast;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AngularAuthApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetWeatherForecast/{city}")]
        public async Task<WeatherData> GetWeatherForecast(string city)
        {
            WeatherData weatherData = null;
            var _client = new HttpClient();
            string prova = "https://api.openweathermap.org/data/2.5/weather?q=" + city + "&appid=fb9d1da8f6364b6f2b28f5010b25a9fa";
            var response = await _client.GetAsync(prova);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                weatherData = JsonConvert.DeserializeObject<WeatherData>(content);
            }
            

            return weatherData;
        }
    }
}
