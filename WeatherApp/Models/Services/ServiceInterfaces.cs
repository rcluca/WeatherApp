using DotNet.Highcharts;
using ForecastIO;
using Geocoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WeatherApp.Models.ViewModels;

namespace WeatherApp.Models.Services
{
    public interface IWeatherInfoService
    {
        WeatherViewModel WeatherInfo(AddressViewModel addressViewModel);
        Highcharts WeatherChart(List<ForecastIOResponse> previousWeekWeather);
    }
}