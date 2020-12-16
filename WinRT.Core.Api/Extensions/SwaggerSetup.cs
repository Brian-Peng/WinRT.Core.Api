﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WinRT.Core.Helper;
using static WinRT.Core.Extensions.CustomApiVersion;

namespace WinRT.Core.Extensions
{
    /// <summary>
    /// Swagger 启动服务
    /// </summary>
    public static class SwaggerSetup
    {
        public static void AddSwaggerSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // 获取项目的根路径
            var basePath = AppContext.BaseDirectory;
            // 项目名称
            var ApiName = Appsettings.app(new string[] { "Startup", "ApiName" });

            services.AddSwaggerGen(c =>
            {
                //遍历出全部的版本，做文档信息展示
                typeof(ApiVersions).GetEnumNames().ToList().ForEach(version =>
                {
                    c.SwaggerDoc(version, new OpenApiInfo
                    {
                        Version = version,
                        Title = $"{ApiName} 接口文档——{RuntimeInformation.FrameworkDescription}", 
                        Description = $"{ApiName} HTTP API " + version,
                        //Contact = new OpenApiContact { Name = ApiName, Email = "Blog.Core@xxx.com", Url = new Uri("https://neters.club") },
                        //License = new OpenApiLicense { Name = ApiName + " 官方文档", Url = new Uri("http://apk.neters.club/.doc/") }
                    }); 
                    // 排序
                    c.OrderActionsBy(o => o.RelativePath);
                });

                try
                {
                    //这个就是刚刚配置的xml文件名
                    var xmlPath = Path.Combine(basePath, "WinRT.Core.xml");
                    //默认的第二个参数是false，true是开启controller的注释
                    c.IncludeXmlComments(xmlPath, true);

                    //这个就是Model层的xml文件名
                    //var xmlModelPath = Path.Combine(basePath, "Blog.Core.Model.xml");
                    //c.IncludeXmlComments(xmlModelPath);
                }
                catch (Exception ex)
                {
                    //log.Error("Blog.Core.xml和Blog.Core.Model.xml 丢失，请检查并拷贝。\n" + ex.Message);
                }

                // 开启加权小锁
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                // 在header中添加token，传递到后台
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                // Jwt Bearer 认证，必须是 oauth2
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）\"",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey
                });
            });
        }

        public static void UseSwaggerMildd(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            // 开启服务
            app.UseSwagger();
            // 开启服务UI
            app.UseSwaggerUI(c =>
            {
                //根据版本名称倒序 遍历展示
                var ApiName = Appsettings.app(new string[] { "Startup", "ApiName" });
                typeof(ApiVersions).GetEnumNames().OrderByDescending(e => e).ToList().ForEach(version =>
                {
                    c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{ApiName} {version}");
                });
                // 路径配置，设置为空，表示直接在根域名（localhost:8001）访问该文件,注意localhost:8001/swagger是访问不到的，去launchSettings.json把launchUrl去掉，如果你想换一个路径，直接写名字即可，比如直接写c.RoutePrefix = "doc";
                c.RoutePrefix = "";
            });
        }
    }


    /// <summary>
    /// 自定义版本
    /// </summary>
    public class CustomApiVersion
    {
        /// <summary>
        /// Api接口版本 自定义
        /// </summary>
        public enum ApiVersions
        {
            /// <summary>
            /// V1 版本
            /// </summary>
            V1 = 1,
            /// <summary>
            /// V2 版本
            /// </summary>
            V2 = 2,
        }
    }
}
