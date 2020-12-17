using Autofac;
using Autofac.Extras.DynamicProxy;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WinRT.Core.Api.AOP;
using WinRT.Core.Helper;
using WinRT.Core.Repository.Base;

namespace WinRT.Core.Api.Extensions
{
    public class AutofacModuleRegister: Autofac.Module
    {
        public AutofacModuleRegister()
        { 
        
        }
        protected override void Load(ContainerBuilder builder)
        {
            var basePath = AppContext.BaseDirectory;

            #region 带有接口层的服务注入

            var servicesDllFile = Path.Combine(basePath, "WinRT.Core.Services.dll");
            //var repositoryDllFile = Path.Combine(basePath, "WinRT.Core.Repository.dll");

            if (!File.Exists(servicesDllFile)) /* && File.Exists(repositoryDllFile))*/ 
            {
                var msg = "Repository.dll和service.dll 丢失，因为项目解耦了，所以需要先F6编译，再F5运行，请检查 bin 文件夹，并拷贝。";
                throw new Exception(msg);
            }


            // AOP 开关，如果想要打开指定的功能，只需要在 appsettigns.json 对应对应 true 就行。
            var cacheType = new List<Type>();
            if (Appsettings.app(new string[] { "AppSettings", "RedisCachingAOP", "Enabled" }).ObjToBool()) // 按照 appsettigns.json中的层级的顺序，依次写出来

            {
                //builder.RegisterType<BlogRedisCacheAOP>();
                //cacheType.Add(typeof(BlogRedisCacheAOP));
            }
            if (Appsettings.app(new string[] { "AppSettings", "MemoryCachingAOP", "Enabled" }).ObjToBool())
            {
                builder.RegisterType<CacheAOP>();
                cacheType.Add(typeof(CacheAOP));
            }
            if (Appsettings.app(new string[] { "AppSettings", "TranAOP", "Enabled" }).ObjToBool())
            {
                //builder.RegisterType<BlogTranAOP>();
                //cacheType.Add(typeof(BlogTranAOP));
            }
            if (Appsettings.app(new string[] { "AppSettings", "LogAOP", "Enabled" }).ObjToBool())
            {
                builder.RegisterType<LogAOP>();
                cacheType.Add(typeof(LogAOP));
            }

            builder.RegisterGeneric(typeof(BaseRepository<>)).As(typeof(IBaseRepository<>)).InstancePerDependency();//注册仓储

            // 获取 Service.dll 程序集服务，并注册
            var assemblysServices = Assembly.LoadFrom(servicesDllFile);
            builder.RegisterAssemblyTypes(assemblysServices)
                      .AsImplementedInterfaces()
                      .InstancePerDependency()
                      .EnableInterfaceInterceptors()//引用Autofac.Extras.DynamicProxy;
                      //允许将拦截器服务的列表分配给注册。Autofac它只对接口方法 或者 虚virtual方法或者重写方法override才能起拦截作用。  
                      .InterceptedBy(cacheType.ToArray());
                                                          //.InterceptedBy(
                                                          //  typeof(LogAOP), 
                                                          //  typeof(CacheAOP));//可以放一个AOP拦截器集合,拦截器添加到要注入容器的接口或者类之上
                                                          // aop的执行顺序好比挖洞，由上之下，事后处理，想出去，再由下之上
                                                          //.InterceptedBy(cacheType.ToArray());//允许将拦截器服务的列表分配给注册。

            #endregion

        }
    }
}
