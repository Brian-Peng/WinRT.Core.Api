using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinRT.Core.Common.GlobalVar;
using WinRT.Core.Common.Redis;

namespace WinRT.Core.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ValuesController:ControllerBase
    {
        /// <summary>
        /// IHttp上下文访问器接口，提供对当前HttpContext的访问
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IRedisBasketRepository _redisBasketRepository;

        public ValuesController(IHttpContextAccessor httpContextAccessor, IRedisBasketRepository redisBasketRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _redisBasketRepository = redisBasketRepository;
        }

        /// <summary>
        ///  这个数据库有配置权限
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize("Permission")]
        public string Get()
        {
            return "测试Permission";
        }

        /// <summary>
        ///  这个数据库没有配置权限
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Get1")]
        [Authorize("Permission")]
        public string Get1()
        {
            return "测试Permission";
        }

        [HttpGet]
        [Route("Get2")]
        public string Get2()
        {

            return "测试CacheAOP缓存";
        }


        [HttpPost]
        public ActionResult TestCORSForVue()
        {
            return Ok("TestCORS");
        }

        /// <summary>
        ///  定义发布者
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task RedisMq()
        {
            var msg = "这里是一条日志";
            await _redisBasketRepository.ListLeftPushAsync(RedisMqKey.Loging, msg);
        }
    }
}
