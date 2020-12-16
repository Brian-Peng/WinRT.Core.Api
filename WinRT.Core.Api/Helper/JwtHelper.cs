using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WinRT.Core.Helper
{
    public class JwtHelper
    {
        /// <summary>
        /// 颁发JWT字符串
        /// </summary>
        /// <param name="tokenModel"></param>
        /// <returns></returns>
        public static string IssueJwt(TokenModelJwt tokenModel)
        {
            string iss = Appsettings.app(new string[] { "Audience", "Issuer" });
            string aud = Appsettings.app(new string[] { "Audience", "Audience" });
            string secret = AppSecretConfig.Audience_Secret_String;

            //var claims = new Claim[] //old
            // 创建声明数据
            var claims = new List<Claim>
         {
          /*
          * 特别重要：
            1、这里将用户的部分信息，比如 uid 存到了Claim 中，如果你想知道如何在其他地方将这个 uid从 Token 中取出来，请看下边的SerializeJwt() 方法，或者在整个解决方案，搜索这个方法，看哪里使用了！
            2、你也可以研究下 HttpContext.User.Claims ，具体的你可以看看 Policys/PermissionHandler.cs 类中是如何使用的。
          */
          
         // 下边为Claim的默认配置
         new Claim(JwtRegisteredClaimNames.Jti, tokenModel.Uid.ToString()),
         new Claim(JwtRegisteredClaimNames.Iat, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
         new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}") ,
         //这个就是过期时间，目前是过期1000秒，可自定义，注意JWT有自己的缓冲过期时间
         new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddSeconds(30)).ToUnixTimeSeconds()}"),
         new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(1000).ToString()),

         new Claim(JwtRegisteredClaimNames.Iss,iss),
         new Claim(JwtRegisteredClaimNames.Aud,aud),
         //new Claim("laozhang","laoli"), // 这里的键值对可以随意去定义
         
         // 这个Role是官方UseAuthentication要验证的Role，我们就不用手动设置Role这个属性了
         //new Claim(ClaimTypes.Role,tokenModel.Role),//为了解决一个用户多个角色(比如：Admin,System)，用下边的方法
        };

            // 可以将一个用户的多个角色全部赋予；
            // 作者：DX 提供技术支持；
            claims.AddRange(tokenModel.Role.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));


            //秘钥 (SymmetricSecurityKey 对安全性的要求，密钥的长度太短会报出异常)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 实例化token对象，3+2的形式
            var jwt = new JwtSecurityToken(
                issuer: iss, // 发行人
                claims: claims, // 声明
                signingCredentials: creds // 密钥
                );

            // 生成token
            var jwtHandler = new JwtSecurityTokenHandler();     
            var encodedJwt = jwtHandler.WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// 令牌
        /// </summary>
        public class TokenModelJwt
        {
            /// <summary>
            /// Id
            /// </summary>
            public long Uid { get; set; }
            /// <summary>
            /// 角色
            /// </summary>
            public string Role { get; set; }
            /// <summary>
            /// 职能
            /// </summary>
            public string Work { get; set; }

        }


    }
}
