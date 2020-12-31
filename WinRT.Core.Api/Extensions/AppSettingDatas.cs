using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinRT.Core.Api.Extensions
{
    public class Rootobject
    {
        public Logging Logging { get; set; }
        public string AllowedHosts { get; set; }
        public Startup Startup { get; set; }
        public Audience Audience { get; set; }
        public Appsettings AppSettings { get; set; }
        public Redis Redis { get; set; }
    }

    public class Logging
    {
        public Loglevel LogLevel { get; set; }
    }

    public class Loglevel
    {
        public string Default { get; set; }
        public string Microsoft { get; set; }
        public string MicrosoftHostingLifetime { get; set; }
    }

    public class Startup
    {
        public string ApiName { get; set; }
    }

    public class Audience
    {
        public string Secret { get; set; }
        public string SecretFile { get; set; }
        public string Issuer { get; set; }
        //  public string Audience { get; set; }
    }

    public class Appsettings
    {
        public Rediscachingaop RedisCachingAOP { get; set; }
        public Memorycachingaop MemoryCachingAOP { get; set; }
        public Logaop LogAOP { get; set; }
    }

    public class Rediscachingaop
    {
        public bool Enabled { get; set; }
        public string ConnectionString { get; set; }
    }

    public class Memorycachingaop
    {
        public bool Enabled { get; set; }
    }

    public class Logaop
    {
        public bool Enabled { get; set; }
    }

    public class Redis
    {
        public string ConnectionString { get; set; }
    }

}
