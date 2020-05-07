using System;
using System.ComponentModel.DataAnnotations;

namespace SampleServerSide.Data
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        [Display(Name ="In C (From display attribute)")]
        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }
}
