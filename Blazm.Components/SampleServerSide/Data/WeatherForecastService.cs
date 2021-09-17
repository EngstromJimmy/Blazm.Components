using System;
using System.Linq;
using System.Threading.Tasks;

namespace SampleServerSide.Data
{
    public class WeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private static readonly string[] Locations = new[]
        {
            "Central City", "Duckburg", "Gotham City", "Metropolis", "Star City", "Wakanda", "Åkersberga"
        };

        public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate,int numerofitems=50)
        {
            var rng = new Random();
            return Task.FromResult(Enumerable.Range(1, numerofitems).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)],
                Location = Locations[rng.Next(Locations.Length)]
            }).ToArray());
        }
    }
}
