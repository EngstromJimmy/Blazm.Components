namespace BlazmBlazorServerDemo.Data;

public class WeatherForecastService
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private static readonly string[] Locations = new[]
    {
        "Central City", "Duckburg", "Gotham City", "Metropolis", "Star City", "Wakanda", "Åkersberga","Gudö"
    };

    public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate, int numerofitems = 50)
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

    WeatherForecast[]? data = null;
    int totalcount = 10000;
    public async Task<WeatherForecast[]> GetForecastAsync(int startIndex, int numberofItems)
    {
        if (data == null)
        {
            data = await GetForecastAsync(DateTime.Now, totalcount);
        }

        return data.Skip(startIndex).Take(numberofItems).ToArray();
    }
    public async Task<int> GetForecastCountAsync()
    {
        if (data == null)
        {
            data = await GetForecastAsync(DateTime.Now, totalcount);
        }

        return data.Count();
    }

}
