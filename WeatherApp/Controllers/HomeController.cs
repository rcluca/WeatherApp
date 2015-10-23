using DotNet.Highcharts;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using ForecastIO;
using ForecastIO.Extensions;
using Geocoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WeatherApp.Models.Services;
using WeatherApp.Models.ViewModels;

namespace WeatherApp.Controllers
{
    public class HomeController : Controller
    {
        #region Fields

        IWeatherInfoService _weatherInfoService;

        #endregion

        #region Constructors

        public HomeController() : this(WeatherInfoService.GetInstance()) { }

        public HomeController(IWeatherInfoService weatherInfoService)
        {
            _weatherInfoService = weatherInfoService;
        }

        #endregion

        #region Actions

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(AddressViewModel addressViewModel)
        {
            if (addressViewModel.Address != null)
            {
                WeatherViewModel weatherViewModel = _weatherInfoService.WeatherInfo(addressViewModel);

                if (weatherViewModel != null)
                    return View("SearchResults", weatherViewModel);
            }

            this.ModelState.AddModelError("", "No weather information could be provided for the given address.  Please try another address.");
            return View(addressViewModel);
        }

        #endregion

        #region PartialViews

        public ActionResult _PreviousWeatherChart(WeatherViewModel weatherViewModel)
        {
            Highcharts chart = _weatherInfoService.WeatherChart(weatherViewModel.PreviousWeekWeather);

            return PartialView(chart);
        }

        public ActionResult ChartTest()
        {
            Highcharts chart = new Highcharts("chart")
                .SetXAxis(new XAxis
                {
                    Categories = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }
                })
                .SetSeries(new Series
                {
                    Data = new Data(new object[] { 29.9, 71.5, 106.4, 129.2, 144.0, 176.0, 135.6, 148.5, 216.4, 194.1, 95.6, 54.4 })
                });

            return View(chart);
        }

        #endregion
    }
}