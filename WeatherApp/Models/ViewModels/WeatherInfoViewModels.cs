using ForecastIO;
using Geocoding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WeatherApp.Models.ViewModels
{
    public class AddressViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The Address must be less than 100 characters long.")]
        public string Address { get; set; }
    }

    public class WeatherViewModel
    {
        public ForecastIOResponse CurrentWeather { get; set; }
        public List<ForecastIOResponse> PreviousWeekWeather { get; set; }
        public Address AddressInfo { get; set; }
    }
}