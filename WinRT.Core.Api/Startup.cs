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
        ///  ��������appsettings.json�Ľӿ�
        /// </summary>
        public IConfiguration Configuration { get; }

        // ��������ӵ�������������Configure()����Configure()����֮ǰ����
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // ע������Ip����
            services.AddIpPolicyRateLimitSetup(Configuration);

            // ע��ģ��ӳ�����
            services.AddAutoMapperSetup();

            // ע���ڴ����
            services.AddScoped<ICaching, MemoryCaching>();
            services.AddSingleton<IMemoryCache>(factory =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());
                return cache;
            });

          
           // ע��Http�����ķ�����
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // ���� services.AddHttpContextAccessor();

            // ע�� appsettings.json������
            services.AddSingleton(new Helper.Appsettings(Configuration));
            services.AddSwaggerSetup();

            services.AddRedisCacheSetup();
            services.AddRedisInitMqSetup();

            #region JWT Token Service
            //��ȡ�����ļ�
            var audienceConfig = Configuration.GetSection("Audience"); // ��ȡ����ָ��key�������ӽ�
            var symmetricKeyAsBase64 = AppSecretConfig.Audience_Secret_String;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            // ������֤������֮ǰ���Ƕ���д��AddJwtBearer��ģ������������
            var tokenValidationParameters = new TokenValidationParameters
            {
                //  3+2����ʽ ����֤�ĸ�ʽҪ������jwt���Ƶĸ�ʽһ��
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

                // �Զ�����ڲ��Ե���ȨȨ�ޣ����û���RBAC��Role-Based Access Control �����ڽ�ɫ�ķ��ʿ��ơ�
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

            // Cors  ����
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

            // ��appsettings.json���ݰ󶨵�AppSettingDatas�����У�Ȼ��ע�������
            Rootobject rootobject = new Rootobject();
            // json����ƥ�䵽��
            Configuration.Bind(rootobject);
            // ע�뵽����������
            services.AddSingleton(rootobject);

            // ע�����������
            services.AddControllers(o =>
            {
                // ȫ���쳣����
                o.Filters.Add(typeof(GlobalExceptionsFilter));       
            });
        }

        // ע����Program.CreateHostBuilder�����Autofac���񹤳�
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModuleRegister());
            //ֱ��ע��ĳһ����ͽӿ�
            //��ߵ���ʵ���࣬�ұߵ�As�ǽӿ�
            //builder.RegisterType<AdvertisementServices>().As<IAdvertisementServices>();
        }

        // ʹ�ô˷�������HTTP����ܵ�
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Ip����,�����Źܵ����
            app.UseIpRateLimiting();

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

            // �����һ����·�м������ʾ http ����������Ͳ���������. 
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


    }
}
