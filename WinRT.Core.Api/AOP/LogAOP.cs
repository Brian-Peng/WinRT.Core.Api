using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinRT.Core.Common;

using WinRT.Core.Common.LogHelper;

namespace WinRT.Core.Api.AOP
{
    /// <summary>
    ///   1、继承接口IInterceptor
    ///   2、实例化接口IINterceptor的唯一方法Intercept
    ///   3、void Proceed(); 表示执行当前的方法
    ///   4、执行后，输出到日志文件。
    ///   原理是采用动态代理，依赖注入注入的服务，我们动态代理就是要代理这些服务,在获取到这些服务之后，就可以控制这些服务了（代理就是控制服务）
    /// </summary>
    public class LogAOP : IInterceptor
    {
        /// <summary>
        /// 实例化IInterceptor唯一方法 
        /// </summary>
        /// <param name="invocation">包含被拦截方法的信息</param>
        public void Intercept(IInvocation invocation)
        {

            // 事前方法: 在services层中的方法执行之前,做相应的逻辑处理
            var dataIntercept = "" +
                $"【当前执行方法】：{ invocation.Method.Name} \r\n" +
                $"【携带的参数有】： {string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())} \r\n";

            // 执行当前访问的services层中的方法,(注意:如果下边还有其他的AOP拦截器的话,会跳转到其他的AOP里)
          //  invocation.Proceed();

            try
            {
                invocation.Proceed();
            }
            catch (Exception e)
            {
                //基于AOP，捕获service 中的异常
                dataIntercept += ($"方法执行中出现异常：{e.Message + e.InnerException}");
            }

            // 事后方法: 在services层中的方法被执行了以后,做相应的处理,这里是输出到日志文件
            dataIntercept += ($"【执行完成结果】：{invocation.ReturnValue}");

            // 输出到日志文件
            Parallel.For(0, 1, e =>
            {
               LogLock.OutSql2Log("AOPLog", new string[] { dataIntercept });
            });

        }
    }
}