using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WinRT.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // 使用服务工厂，将Autofac容器添加到Host
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Startup类服务于Program类
                    webBuilder
                    .UseStartup<Startup>()
                    // 注册log4net服务，对ILogger接口进行实现
                    .ConfigureLogging((hostingContext, builder) =>
                    {
                        //过滤掉系统默认的一些日志
                        builder.AddFilter("System", LogLevel.Error);
                        builder.AddFilter("Microsoft", LogLevel.Error);
                        //builder.AddFilter("Blog.Core.AuthHelper.ApiResponseHandler", LogLevel.Error);

                        //可配置文件
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "Log4net.config");
                        builder.AddLog4Net(path);
                    });
                });
    }
}
