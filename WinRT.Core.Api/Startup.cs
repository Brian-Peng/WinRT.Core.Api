using AspNetCoreRateLimit;
using Autofac;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using WinRT.Core.Api.AutoMapper;
using WinRT.Core.Api.Extensions;
using WinRT.Core.Api.Filter;
using WinRT.Core.Authorizations.Policys;
using WinRT.Core.Common.MemoryCache;
using WinRT.Core.Common.Redis;
using WinRT.Core.Extensions;
using WinRT.Core.Helper;

namespace WinRT.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        ///  用来操作appsettings.json的接口
        /// </summary>
        public IConfiguration Configuration { get; }

        // 将服务添加到容器，服务于Configure()，在Configure()方法之前运行
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 注入服务端Ip限流
            services.AddIpPolicyRateLimitSetup(Configuration);

            // 注入模型映射服务
            services.AddAutoMapperSetup();

            // 注入内存服务
            services.AddScoped<ICaching, MemoryCaching>();
            services.AddSingleton<IMemoryCache>(factory =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());
                return cache;
            });

          
           // 注入Http上下文访问器
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // 或者 services.AddHttpContextAccessor();

            // 注入 appsettings.json操作类
            services.AddSingleton(new Helper.Appsettings(Configuration));
            services.AddSwaggerSetup();

            services.AddRedisCacheSetup();
            services.AddRedisInitMqSetup();

            #region JWT Token Service
            //读取配置文件
            var audienceConfig = Configuration.GetSection("Audience"); // 获取具有指定key的配置子节
            var symmetricKeyAsBase64 = AppSecretConfig.Audience_Secret_String;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            // 令牌验证参数，之前我们都是写在AddJwtBearer里的，这里提出来了
            var tokenValidationParameters = new TokenValidationParameters
            {
                //  3+2的形式 ，验证的格式要与生成jwt令牌的格式一样
                ValidateIssuerSigningKey = true,//验证发行人的签名密钥
                IssuerSigningKey = signingKey,

                ValidateIssuer = true,//验证发行人
                ValidIssuer = audienceConfig["Issuer"],//发行人

                ValidateAudience = true,//验证订阅人
                ValidAudience = audienceConfig["Audience"],//订阅人

                ValidateLifetime = true,//验证生命周期
                ClockSkew = TimeSpan.Zero,//这个是定义的过期的缓存时间

                RequireExpirationTime = true,//是否要求过期
            };
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // 注意使用RESTful风格的接口会更好，因为只需要写一个Url即可，比如：/api/values 代表了Get Post Put Delete等多个。
            // 如果想写死，可以直接在这里写。
            //var permission = new List<PermissionItem> {
            //                  new PermissionItem {  Url="/api/values", Role="Admin"},
            //                  new PermissionItem {  Url="/api/values", Role="System"},
            //                  new PermissionItem {  Url="/api/claims", Role="Admin"},
            //              };

            // 如果要数据库动态绑定，这里先留个空，后边处理器里动态赋值
            var permission = new List<PermissionItem>();

            // 角色与接口的权限要求参数
            var permissionRequirement = new PermissionRequirement(
                "/api/denied",// 拒绝授权的跳转地址（目前无用）
                permission,//这里还记得么，就是我们上边说到的角色地址信息凭据实体类 Permission
                ClaimTypes.Role,//基于角色的授权
                audienceConfig["Issuer"],//发行人
                audienceConfig["Audience"],//订阅人
                signingCredentials,//签名凭据
                expiration: TimeSpan.FromSeconds(60 * 2)//接口的过期时间，注意这里没有了缓冲时间，你也可以自定义，在上边的TokenValidationParameters的 ClockSkew
                );

            // ① 核心之一，配置授权服务，也就是具体的规则，已经对应的权限策略，比如公司不同权限的门禁卡
            services.AddAuthorization(options =>
            {
                //options.AddPolicy("Client",
                //    policy => policy.RequireRole("Client").Build());
                //options.AddPolicy("Admin",
                //    policy => policy.RequireRole("Admin").Build());
                //options.AddPolicy("SystemOrAdmin",
                //    policy => policy.RequireRole("Admin", "System"));

                // 自定义基于策略的授权权限，采用基于RBAC（Role-Based Access Control ）基于角色的访问控制。
                options.AddPolicy("Permission",
                     policy => policy.Requirements.Add(permissionRequirement));
            })
            // ② 核心之二，必需要配置认证服务，这里是jwtBearer默认认证，比如光有卡没用，得能识别他们
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // ③ 核心之三，针对JWT的配置，比如门禁是如何识别的，是放射卡，还是磁卡
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = tokenValidationParameters;
            });

            // 依赖注入，将自定义的授权处理器 匹配给官方授权处理器接口，这样当系统处理授权的时候，就会直接访问我们自定义的授权处理器了。
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            // 将授权必要类注入生命周期内
            services.AddSingleton(permissionRequirement);

            #endregion

            // Cors  跨域
            services.AddCors(c =>
            {
                //一般采用这种方法
                c.AddPolicy("LimitRequests", policy =>
                {
                    policy
                    //  Adds the specified origins to the policy.
                    // 支持多个域名端口，注意端口号后不要带/斜杆：比如localhost:8000/，是错的
                    // 注意，http://127.0.0.1:5401 和 http://localhost:5401 是不一样的，尽量写两个
                    .WithOrigins("https://fiddle.jshell.net")
                    .AllowAnyHeader()//Ensures that the policy allows any header.
                    .AllowAnyMethod();
                });
            });     

            // 把appsettings.json数据绑定到AppSettingDatas对象中，然后注入进容器
            Rootobject rootobject = new Rootobject();
            // json数据匹配到类
            Configuration.Bind(rootobject);
            // 注入到生命周期内
            services.AddSingleton(rootobject);

            // 注入控制器服务
            services.AddControllers(o =>
            {
                // 全局异常过滤
                o.Filters.Add(typeof(GlobalExceptionsFilter));       
            });
        }

        // 注意在Program.CreateHostBuilder，添加Autofac服务工厂
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModuleRegister());
            //直接注册某一个类和接口
            //左边的是实现类，右边的As是接口
            //builder.RegisterType<AdvertisementServices>().As<IAdvertisementServices>();
        }

        // 使用此方法配置HTTP请求管道
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Ip限流,尽量放管道外层
            app.UseIpRateLimiting();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwaggerMildd();

            //将 CORS 中间件添加到 web 应用程序管线中, 以允许跨域请求。
            app.UseCors("LimitRequests");

            app.UseRouting();

            // 开启认证中间件
            app.UseAuthentication();

            // 开启授权中间件
            app.UseAuthorization();

            // 这个是一个短路中间件，表示 http 请求到了这里就不往下走了. 
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


    }
}
