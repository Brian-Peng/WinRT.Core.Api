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
            // ע���ڴ����
            services.AddScoped<ICaching, MemoryCaching>();
            services.AddSingleton<IMemoryCache>(factory =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());
                return cache;
            });

            services.AddHttpContextAccessor();

            // ע�� appsettings.json������
            services.AddSingleton(new Appsettings(Configuration));
            services.AddSwaggerSetup();

            // ע��redis�ӿں���
            services.AddTransient<IRedisBasketRepository, RedisBasketRepository>();
            // ��������Redis������Ȼ����Ӱ����Ŀ�����ٶȣ����ǲ��������е�ʱ�򱨴������Ǻ����
            services.AddSingleton(sp =>
            {
                //��ȡ�����ַ���
                string redisConfiguration = Appsettings.app(new string[] { "Redis", "ConnectionString" });

                var configuration = ConfigurationOptions.Parse(redisConfiguration, true);

                configuration.ResolveDns = true;

                return ConnectionMultiplexer.Connect(configuration);
            });


            // 1����Ȩ���ô����ǲ�����controller�У�д��� roles ��
            // ��ͬ�Ľ�ɫ������ͬ�Ĳ���
            // Ȼ����ôд [Authorize(Policy = "Admin")]
            //services.AddAuthorization(options =>
            //{
            //    // ����Claim����Ľ�ɫ��
            //    options.AddPolicy("Client", policy => policy.RequireRole("Client").Build());
            //    options.AddPolicy("Admin", policy => policy.RequireRole("Admin").Build());
            //    options.AddPolicy("SystemOrAdmin", policy => policy.RequireRole("Admin", "System").Build());
            //    options.AddPolicy("SystemAndAdmin", policy => policy.RequireRole("Admin").RequireRole("System").Build());

            //    // ����������Claim�����

            //    // ������Ҫ��Requirement����ȫ�Զ���
            //});

            #region JWT Token Service
            //��ȡ�����ļ�
            var audienceConfig = Configuration.GetSection("Audience");
            var symmetricKeyAsBase64 = AppSecretConfig.Audience_Secret_String;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            // ������֤������֮ǰ���Ƕ���д��AddJwtBearer��ģ������������
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,//��֤�����˵�ǩ����Կ
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,//��֤������
                ValidIssuer = audienceConfig["Issuer"],//������
                ValidateAudience = true,//��֤������
                ValidAudience = audienceConfig["Audience"],//������
                ValidateLifetime = true,//��֤��������
                ClockSkew = TimeSpan.Zero,//����Ƕ���Ĺ��ڵĻ���ʱ��
                RequireExpirationTime = true,//�Ƿ�Ҫ�����

            };
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // ע��ʹ��RESTful���Ľӿڻ���ã���Ϊֻ��Ҫдһ��Url���ɣ����磺/api/values ������Get Post Put Delete�ȶ����
            // �����д��������ֱ��������д��
            //var permission = new List<PermissionItem> {
            //                  new PermissionItem {  Url="/api/values", Role="Admin"},
            //                  new PermissionItem {  Url="/api/values", Role="System"},
            //                  new PermissionItem {  Url="/api/claims", Role="Admin"},
            //              };

            // ���Ҫ���ݿ⶯̬�󶨣������������գ���ߴ������ﶯ̬��ֵ
            var permission = new List<PermissionItem>();

            // ��ɫ��ӿڵ�Ȩ��Ҫ�����
            var permissionRequirement = new PermissionRequirement(
                "/api/denied",// �ܾ���Ȩ����ת��ַ��Ŀǰ���ã�
                permission,//���ﻹ�ǵ�ô�����������ϱ�˵���Ľ�ɫ��ַ��Ϣƾ��ʵ���� Permission
                ClaimTypes.Role,//���ڽ�ɫ����Ȩ
                audienceConfig["Issuer"],//������
                audienceConfig["Audience"],//������
                signingCredentials,//ǩ��ƾ��
                expiration: TimeSpan.FromSeconds(60 * 2)//�ӿڵĹ���ʱ�䣬ע������û���˻���ʱ�䣬��Ҳ�����Զ��壬���ϱߵ�TokenValidationParameters�� ClockSkew
                );

            // �� ����֮һ��������Ȩ����Ҳ���Ǿ���Ĺ����Ѿ���Ӧ��Ȩ�޲��ԣ����繫˾��ͬȨ�޵��Ž���
            services.AddAuthorization(options =>
            {
                //options.AddPolicy("Client",
                //    policy => policy.RequireRole("Client").Build());
                //options.AddPolicy("Admin",
                //    policy => policy.RequireRole("Admin").Build());
                //options.AddPolicy("SystemOrAdmin",
                //    policy => policy.RequireRole("Admin", "System"));

                // �Զ�����ڲ��Ե���ȨȨ��
                options.AddPolicy("Permission",
                     policy => policy.Requirements.Add(permissionRequirement));
            })
            // �� ����֮��������Ҫ������֤����������jwtBearerĬ����֤��������п�û�ã�����ʶ������
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // �� ����֮�������JWT�����ã������Ž������ʶ��ģ��Ƿ��俨�����Ǵſ�
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = tokenValidationParameters;
            });


            // ����ע�룬���Զ������Ȩ������ ƥ����ٷ���Ȩ�������ӿڣ�������ϵͳ������Ȩ��ʱ�򣬾ͻ�ֱ�ӷ��������Զ������Ȩ�������ˡ�
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            // ����Ȩ��Ҫ��ע������������
            services.AddSingleton(permissionRequirement);

            #endregion


            services.AddCors(c =>
            {
                //һ��������ַ���
                c.AddPolicy("LimitRequests", policy =>
                {
                    policy
                    //  Adds the specified origins to the policy.
                    // ֧�ֶ�������˿ڣ�ע��˿ںź�Ҫ��/б�ˣ�����localhost:8000/���Ǵ��
                    // ע�⣬http://127.0.0.1:5401 �� http://localhost:5401 �ǲ�һ���ģ�����д����
                    .WithOrigins("https://fiddle.jshell.net")
                    .AllowAnyHeader()//Ensures that the policy allows any header.
                    .AllowAnyMethod();
                });
            });

            //��ȡ�����ļ�
            //var audienceConfig = Configuration.GetSection("Audience");
            //var symmetricKeyAsBase64 = AppSecretConfig.Audience_Secret_String;
            //var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            //var signingKey = new SymmetricSecurityKey(keyByteArray);

            //var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            //// ������֤����
            //// ������֤����
            //var tokenValidationParameters = new TokenValidationParameters
            //{  //  3+2����ʽ

            //    ValidateIssuerSigningKey = true, // ������֤��Կ
            //    IssuerSigningKey = signingKey, // ��֤��Կ

            //    ValidateIssuer = true,
            //    ValidIssuer = audienceConfig["Issuer"],//��֤������

            //    ValidateAudience = true,
            //    ValidAudience = audienceConfig["Audience"],//��֤������

            //    // ע�����ǻ������ʱ�䣬�ܵ���Чʱ��������ʱ�����jwt�Ĺ���ʱ�䣬��������ã�Ĭ����5����
            //    ClockSkew = TimeSpan.FromSeconds(30),
            //    ValidateLifetime = true,// �Ƿ���֤��ʱ  ������exp��nbfʱ��Ч ͬʱ����ClockSkew 
            //    RequireExpirationTime = true, // Ҫ����֤����ʱ��
            //};

            ////2.1����֤����core�Դ��ٷ�JWT��֤
            //// ����Bearer��֤
            //// services.AddAuthentication("Bearer")
            //services.AddAuthentication(x =>
            //{
            //    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //})
            // // ���JwtBearer����
            // .AddJwtBearer(o =>
            // {
            //     o.TokenValidationParameters = tokenValidationParameters;
            //     o.Events = new JwtBearerEvents
            //     {
            //         OnAuthenticationFailed = context =>
            //         {
            //             // ������ڣ����<�Ƿ����>��ӵ�������ͷ��Ϣ��
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


        // ע����Program.CreateHostBuilder�����Autofac���񹤳�
        public void ConfigureContainer(ContainerBuilder builder)
        {
            //builder.RegisterType<LogAOP>();//����ֱ���滻������������һ��Ҫ������������ע��
            //builder.RegisterType<CacheAOP>();//����ֱ���滻������������һ��Ҫ������������ע��

            builder.RegisterModule(new AutofacModuleRegister());

            //ֱ��ע��ĳһ����ͽӿ�
            //��ߵ���ʵ���࣬�ұߵ�As�ǽӿ�
            //builder.RegisterType<AdvertisementServices>().As<IAdvertisementServices>();
        }

        ///// <summary>
        /////  Autofac ����
        ///// </summary>
        ///// <param name="builder"></param>
        //public void ConfigureContainer(ContainerBuilder builder)
        //{
        //    // ���Ͳִ����������ִ�
        //    builder.RegisterGeneric(typeof(BaseRepository<>)).As(typeof(IBaseRepository<>)).InstancePerDependency();//ע��ִ�

        //    // ע�����з��񣬷������ע�뷽ʽ ���� δ����
        //    var assemblysServices = Assembly.Load("WinRT.Core.Services");

        //    builder.RegisterAssemblyTypes(assemblysServices)
        //              .AsImplementedInterfaces()
        //              .InstancePerDependency()
        //              .EnableInterfaceInterceptors();//����Autofac.Extras.DynamicProxy;


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

            //�� CORS �м����ӵ� web Ӧ�ó��������, �������������
            app.UseCors("LimitRequests");

            app.UseRouting();

            // ������֤�м��
            app.UseAuthentication();

            // ������Ȩ�м��
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
