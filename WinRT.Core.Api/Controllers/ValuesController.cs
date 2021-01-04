using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinRT.Core.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]


    public class ValuesController:ControllerBase
    {
        public ValuesController()
        { 
        
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
    }
}
