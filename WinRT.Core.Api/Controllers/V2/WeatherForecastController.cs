using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinRT.Core.Common.Redis;
using WinRT.Core.Helper;
using WinRT.Core.IServices;
using WinRT.Core.Model.Models;
//using WinRT.Core.Services;
using static WinRT.Core.Extensions.CustomApiVersion;

namespace WinRT.Core.Controllers.V2
{
    /// <summary>
    /// 天气预报接口
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly IAdvertisementServices _advertisementServices;


        private readonly IRedisBasketRepository _redisBasketRepository;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger, 
            IAdvertisementServices advertisementServices,
            IRedisBasketRepository redisBasketRepository)
        {
            _logger = logger;
            _advertisementServices = advertisementServices;
            _redisBasketRepository = redisBasketRepository;
        }

        /// <summary>
        ///  获取天气
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [CustomRoute(ApiVersions.V2, "GetWeatherForecast")]
        // 授权
        //[Authorize] //无状态授权
        // [Authorize(Roles ="Admin")] 基于角色授权
        // [Authorize(Policy = "Admin")] // 基于策略授权
        //[Authorize(Policy = "SystemOrAdmin")]    
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }


        [HttpGet]
        [CustomRoute(ApiVersions.V2, "GetAdvertisement1")]
        public async Task<IList<Advertisement>> GetAdvertisement1(int id)
        {
            return await _advertisementServices.GetAdvertisement(id);
        }

        [HttpGet]
        [CustomRoute(ApiVersions.V2, "GetAdvertisement2")]
        public async Task<IList<Advertisement>> GetAdvertisement2(int id)
        {
            return await _advertisementServices.GetAdvertisement(id);
        }

        [HttpGet]
        [CustomRoute(ApiVersions.V2, "GetAdvertisementInvolveRedis")]
        public async Task<IList<Advertisement>> GetAdvertisementInvolveRedis(int id)
        {
            IList<Advertisement> advertisementList = new List<Advertisement>();
            string key = "Redis.Advertisement" + id.ToString();
            var value = await _redisBasketRepository.Get<IList<Advertisement>>(key);
            if (value != null)
            {
                advertisementList = value;
            }
            else
            {
                advertisementList = await _advertisementServices.GetAdvertisement(id);
                await _redisBasketRepository.Set(key, advertisementList, TimeSpan.FromHours(2));//缓存2小时
            }
            return advertisementList;
        }
    }
}
