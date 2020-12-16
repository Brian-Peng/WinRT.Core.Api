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
    /// </summary>
    public class LogAOP : IInterceptor
    {
        /// <summary>
        /// 实例化IInterceptor唯一方法 
        /// </summary>
        /// <param name="invocation">包含被拦截方法的信息</param>
        public void Intercept(IInvocation invocation)
        {

            // 事前处理: 在services层中的方法执行之前,做相应的逻辑处理
            var dataIntercept = "" +
                $"【当前执行方法】：{ invocation.Method.Name} \r\n" +
                $"【携带的参数有】： {string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())} \r\n";

            // 执行当前访问的services层中的方法,(注意:如果下边还有其他的AOP拦截器的话,会跳转到其他的AOP里)
            invocation.Proceed();

            // 事后处理: 在services层中的方法被执行了以后,做相应的处理,这里是输出到日志文件
            dataIntercept += ($"【执行完成结果】：{invocation.ReturnValue}");

            // 输出到日志文件
            Parallel.For(0, 1, e =>
            {
               LogLock.OutSql2Log("AOPLog", new string[] { dataIntercept });
            });

        }
    }
}