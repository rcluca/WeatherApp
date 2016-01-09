using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ForecastIO;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Web.Helpers;
using Geocoding;
using Geocoding.Google;
using WeatherApp.Models.ViewModels;
using DotNet.Highcharts;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Helpers;
using ForecastIO.Extensions;
using System.Web.Configuration;

namespace WeatherApp.Models.Services
{
    public class WeatherInfoService : IWeatherInfoService
    {
        private static WeatherInfoService _instance = null;
        private Dictionary<string, Address> _geoCodes = new Dictionary<string, Address>();

        // Get api keys from secured Web.config file
        private string _weatherApiKey = WebConfigurationManager.AppSettings["WeatherApiKey"];
        private string _googleApiKey = WebConfigurationManager.AppSettings["GoogleApiKey"];

        // Singleton pattern used to allow for cached results through common _geoCode dictionary
        // WeatherInfo is not cached because it changes based on time
        private WeatherInfoService() { }

        public static WeatherInfoService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new WeatherInfoService();
            }

            return _instance;
        }

        public WeatherViewModel WeatherInfo(AddressViewModel addressViewModel, bool extendHourly)
        {
            WeatherViewModel weatherViewModel = null;

            ForecastIOResponse currentWeather = GetWeatherInfo(addressViewModel.Address, extendHourly);

            if (currentWeather != null)
            {
                // Get address info
                Address addressInfo = GetGeoInfo(addressViewModel.Address);

                // Get weather info for previous week
                List<ForecastIOResponse> previousWeekWeather = new List<ForecastIOResponse>();
                for (int i = 7; i > 0; i--)
                {
                    ForecastIOResponse previousWeather = GetWeatherInfo(addressViewModel.Address, DateTime.Now.AddDays(-i));
                    previousWeekWeather.Add(previousWeather);
                }

                // Setup return view model
                weatherViewModel = new WeatherViewModel()
                {
                    CurrentWeather = currentWeather,
                    PreviousWeekWeather = previousWeekWeather,
                    AddressInfo = addressInfo
                };
            }

            return weatherViewModel;
        }

        public Highcharts WeatherChart(List<ForecastIOResponse> previousWeekWeather)
        {
            // Setup Categories for chart
            string[] days = new string[7];
            for (int i = 0; i < previousWeekWeather.Count; i++)
            {
                days[i] = previousWeekWeather[i].currently.time.ToDateTime().DayOfWeek.ToString();
            }

            // Setup Temperature for chart
            object[] temps = new object[7];
            for (int i = 0; i < previousWeekWeather.Count; i++)
            {
                temps[i] = Convert.ToInt32(previousWeekWeather[i].currently.temperature);
            }

            // Create chart
            Highcharts chart = new Highcharts("chart")
                .SetTitle(new Title
                {
                    Text = ""
                })
                .SetXAxis(new XAxis
                {
                    Categories = days,
                    Title = new XAxisTitle { Text = "Week Day" }
                })
                .SetYAxis(new YAxis
                {
                    Title = new YAxisTitle { Text = "Fahrenheit" }
                })
                .SetSeries(new Series
                {
                    Data = new Data(temps),
                    Name = "Fahrenheit"
                });

            return chart;
        }

        private ForecastIOResponse GetWeatherInfo(string address, bool extendHourly)
        {
            // Get Latitude and Longitude for given address
            Address addressInfo = GetGeoInfo(address);
            ForecastIOResponse response = null;

            // If the address was found (had Latitude and Longitude)
            if (addressInfo != null)
            {
                ForecastIORequest request;
                if (extendHourly)
                {
                    Extend[] extendBlocks = new Extend[]
                    { Extend.hourly };
                    request = new ForecastIORequest(_weatherApiKey, Convert.ToSingle(addressInfo.Coordinates.Latitude), Convert.ToSingle(addressInfo.Coordinates.Longitude), Unit.us, extendBlocks);
                    response = request.Get();
                }
                else
                {
                    // Get weather info from forecast API
                    request = new ForecastIORequest(_weatherApiKey, Convert.ToSingle(addressInfo.Coordinates.Latitude), Convert.ToSingle(addressInfo.Coordinates.Longitude), Unit.us);
                    response = request.Get();
                }
            }

            return response;
        }

        private ForecastIOResponse GetWeatherInfo(string address, DateTime dateTime)
        {
            // Get Latitude and Longitude for given address
            Address addressInfo = GetGeoInfo(address);
            ForecastIOResponse response = null;

            // If the address was found (had Latitude and Longitude)
            if (addressInfo != null)
            {
                // Get weather info from forecast API
                ForecastIORequest request = new ForecastIORequest(_weatherApiKey, Convert.ToSingle(addressInfo.Coordinates.Latitude), Convert.ToSingle(addressInfo.Coordinates.Longitude), dateTime, Unit.us);
                response = request.Get();
            }

            return response;
        }

        private Address GetGeoInfo(string address)
        {
            Address addressInfo = null;

            // If the geocode has not already be retrieved for this address
            if (!_geoCodes.ContainsKey(address))
            {
                IGeocoder geocoder = new GoogleGeocoder() { ApiKey = _googleApiKey };

                // Get addresses found matching address given
                IEnumerable<Address> addresses = geocoder.Geocode(address);

                // Set addressInfo to the first matching address
                addressInfo = addresses.FirstOrDefault();

                _geoCodes.Add(address, addressInfo);
            }
            else
            {
                _geoCodes.TryGetValue(address, out addressInfo);
            }

            return addressInfo;
        }
    }
}