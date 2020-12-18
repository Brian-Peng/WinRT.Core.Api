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

        [HttpGet]
        [Authorize("Permission")]
        public string Get()
        {
            return "测试Permission";
        }

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
