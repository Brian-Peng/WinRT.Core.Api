using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinRT.Core.IServices;

namespace WinRT.Core.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BlogController
    {
        private readonly IBlogArticleServices _blogArticleServices;
        /// <summary>
        /// IHttp上下文访问器接口，提供对当前HttpContext的访问
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BlogController(IBlogArticleServices blogArticleServices, IHttpContextAccessor httpContextAccessor)
        {
            _blogArticleServices = blogArticleServices;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("{id}", Name = "Get")]
        public async Task<object> Get(int id)
        {
            var model = await _blogArticleServices.getBlogDetails(id);//调用该方法
            var data = new { success = true, data = model };
            return data;
        }

    }
}
