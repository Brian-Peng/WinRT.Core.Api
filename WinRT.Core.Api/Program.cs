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
    /// <summary>
    /// ���Program�ǳ�������, ������������, ����Ϊasp.net core applicationʵ�ʾ��ǿ���̨����(console application).
    /// </summary>
    public class Program
    {
        /// <summary>
        ///  Main���������������Ҫ���������ú����г���ġ�
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // ʹ�÷��񹤳�����Autofac������ӵ�Host
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Startup�������Program��
                    webBuilder
                    // ��仰��ʾ�ڳ���������ʱ��, ���ǻ����Startup�����.
                    .UseStartup<Startup>()
                    // ע��log4net���񣬶�ILogger�ӿڽ���ʵ��
                    .ConfigureLogging((hostingContext, builder) =>
                    {
                        //���˵�ϵͳĬ�ϵ�һЩ��־
                        builder.AddFilter("System", LogLevel.Error);
                        builder.AddFilter("Microsoft", LogLevel.Error);
                        //builder.AddFilter("Blog.Core.AuthHelper.ApiResponseHandler", LogLevel.Error);

                        //�������ļ�
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "Log4net.config");
                        builder.AddLog4Net(path);
                    });
                });
    }
}
