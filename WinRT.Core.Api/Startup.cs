using Autofac;
using Autofac.Extras.DynamicProxy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WinRT.Core.Extensions;
using WinRT.Core.Authorizations.Policys;
using WinRT.Core.Helper;

using WinRT.Core.IServices;
using WinRT.Core.Api.Extensions;
using WinRT.Core.Api.AOP;
using WinRT.Core.Common.MemoryCache;
using Microsoft.Extensions.Caching.Memory;
using WinRT.Core.Common.Redis;
using StackExchange.Redis;

namespace WinRT.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 注入内存服务
            services.AddScoped<ICaching, MemoryCaching>();
            services.AddSingleton<IMemoryCache>(factory =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());
                return cache;
            });

            services.AddHttpContextAccessor();

            // 注入 appsettings.json操作类
            services.AddSingleton(new Appsettings(Configuration));
            services.AddSwaggerSetup();

            // 注入redis接口和类
            services.AddTransient<IRedisBasketRepository, RedisBasketRepository>();
            // 配置启动Redis服务，虽然可能影响项目启动速度，但是不能在运行的时候报错，所以是合理的
            services.AddSingleton(sp =>
            {
                //获取连接字符串
                string redisConfiguration = Appsettings.app(new string[] { "Redis", "ConnectionString" });

                var configuration = ConfigurationOptions.Parse(redisConfiguration, true);

                configuration.ResolveDns = true;

                return ConnectionMultiplexer.Connect(configuration);
            });


            // 1【授权，好处就是不用在controller中，写多个 roles 。
            // 不同的角色建立不同的策略
            // 然后这么写 [Authorize(Policy = "Admin")]
            //services.AddAuthorization(options =>
            //{
            //    // 基于Claim数组的角色的
            //    options.AddPolicy("Client", policy => policy.RequireRole("Client").Build());
            //    options.AddPolicy("Admin", policy => policy.RequireRole("Admin").Build());
            //    options.AddPolicy("SystemOrAdmin", policy => policy.RequireRole("Admin", "System").Build());
            //    options.AddPolicy("SystemAndAdmin", policy => policy.RequireRole("Admin").RequireRole("System").Build());

            //    // 基于声明的Claim数组的

            //    // 基于需要的Requirement，完全自定义
            //});

            #region JWT Token Service
            //读取配置文件
            var audienceConfig = Configuration.GetSection("Audience");
            var symmetricKeyAsBase64 = AppSecretConfig.Audience_Secret_String;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            // 令牌验证参数，之前我们都是写在AddJwtBearer里的，这里提出来了
            var tokenValidationParameters = new TokenValidationParameters
            {
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

                // 自定义基于策略的授权权限
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

            //读取配置文件
            //var audienceConfig = Configuration.GetSection("Audience");
            //var symmetricKeyAsBase64 = AppSecretConfig.Audience_Secret_String;
            //var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            //var signingKey = new SymmetricSecurityKey(keyByteArray);

            //var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            //// 配置认证服务
            //// 令牌验证参数
            //var tokenValidationParameters = new TokenValidationParameters
            //{  //  3+2的形式

            //    ValidateIssuerSigningKey = true, // 开启验证密钥
            //    IssuerSigningKey = signingKey, // 验证密钥

            //    ValidateIssuer = true,
            //    ValidIssuer = audienceConfig["Issuer"],//验证发行人

            //    ValidateAudience = true,
            //    ValidAudience = audienceConfig["Audience"],//验证订阅人

            //    // 注意这是缓冲过期时间，总的有效时间等于这个时间加上jwt的过期时间，如果不配置，默认是5分钟
            //    ClockSkew = TimeSpan.FromSeconds(30),
            //    ValidateLifetime = true,// 是否验证超时  当设置exp和nbf时有效 同时启用ClockSkew 
            //    RequireExpirationTime = true, // 要求验证过期时间
            //};

            ////2.1【认证】、core自带官方JWT认证
            //// 开启Bearer认证
            //// services.AddAuthentication("Bearer")
            //services.AddAuthentication(x =>
            //{
            //    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //})
            // // 添加JwtBearer服务
            // .AddJwtBearer(o =>
            // {
            //     o.TokenValidationParameters = tokenValidationParameters;
            //     o.Events = new JwtBearerEvents
            //     {
            //         OnAuthenticationFailed = context =>
            //         {
            //             // 如果过期，则把<是否过期>添加到，返回头信息中
            //             if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            //             {
            //                 context.Response.Headers.Add("Token-Expired", "true");
            //             }
            //             return Task.CompletedTask;
            //         }
            //     };
            // });

            services.AddControllers();
        }


        // 注意在Program.CreateHostBuilder，添加Autofac服务工厂
        public void ConfigureContainer(ContainerBuilder builder)
        {
            //builder.RegisterType<LogAOP>();//可以直接替换其他拦截器！一定要把拦截器进行注册
            //builder.RegisterType<CacheAOP>();//可以直接替换其他拦截器！一定要把拦截器进行注册

            builder.RegisterModule(new AutofacModuleRegister());

            //直接注册某一个类和接口
            //左边的是实现类，右边的As是接口
            //builder.RegisterType<AdvertisementServices>().As<IAdvertisementServices>();
        }

        ///// <summary>
        /////  Autofac 容器
        ///// </summary>
        ///// <param name="builder"></param>
        //public void ConfigureContainer(ContainerBuilder builder)
        //{
        //    // 泛型仓储来代替具体仓储
        //    builder.RegisterGeneric(typeof(BaseRepository<>)).As(typeof(IBaseRepository<>)).InstancePerDependency();//注册仓储

        //    // 注入所有服务，服务程序集注入方式 —— 未解耦
        //    var assemblysServices = Assembly.Load("WinRT.Core.Services");

        //    builder.RegisterAssemblyTypes(assemblysServices)
        //              .AsImplementedInterfaces()
        //              .InstancePerDependency()
        //              .EnableInterfaceInterceptors();//引用Autofac.Extras.DynamicProxy;


        //    //var assemblysRepository = Assembly.Load("WinRT.Core.Repository");

        //    //builder.RegisterAssemblyTypes(assemblysRepository)
        //    //            .AsImplementedInterfaces()
        //    //           .InstancePerDependency();

        //}
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
