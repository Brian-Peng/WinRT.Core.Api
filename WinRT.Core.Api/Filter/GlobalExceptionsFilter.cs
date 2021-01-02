﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinRT.Core.Api.Filter
{
    /// <summary>
    /// 全局异常过滤器
    /// </summary>
    public class GlobalExceptionsFilter: IExceptionFilter
    {
      //  private readonly IHostingEnvironment _env;
        private readonly ILogger<GlobalExceptionsFilter> _logger;

        //public GlobalExceptionsFilter(IHostingEnvironment env, ILogger<GlobalExceptionsFilter> logger)
        //{
        //    _env = env;
        //    _logger = logger;
        //}

        public GlobalExceptionsFilter(ILogger<GlobalExceptionsFilter> logger)
        {
            _logger = logger;
        }


        /// <summary>
        ///  在一个动作引发System.Exception之后调用
        /// </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            var json = new JsonErrorResponse();
            json.Message = context.Exception.Message;//错误信息
                                                     //if (_env.IsDevelopment())
                                                     //{
                                                     //    json.DevelopmentMessage = context.Exception.StackTrace;//堆栈信息
                                                     //}

            json.DevelopmentMessage = context.Exception.StackTrace;//堆栈信息
            context.Result = new InternalServerErrorObjectResult(json);

            //采用log4net 进行错误日志记录
            _logger.LogError(json.Message, WriteLog(json.Message, context.Exception));

        }

        /// <summary>
        /// 自定义返回格式
        /// </summary>
        /// <param name="throwMsg"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public string WriteLog(string throwMsg, Exception ex)
        {
            return string.Format("【自定义错误】：{0} \r\n【异常类型】：{1} \r\n【异常信息】：{2} \r\n【堆栈调用】：{3}", new object[] 
            { throwMsg,  ex.GetType().Name, ex.Message, ex.StackTrace });
        }

        public class InternalServerErrorObjectResult : ObjectResult
        {
            public InternalServerErrorObjectResult(object value) : base(value)
            {
                StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
        //返回错误信息
        public class JsonErrorResponse
        {
            /// <summary>
            /// 生产环境的消息
            /// </summary>
            public string Message { get; set; }
            /// <summary>
            /// 开发环境的消息
            /// </summary>
            public string DevelopmentMessage { get; set; }
        }
    }
}
