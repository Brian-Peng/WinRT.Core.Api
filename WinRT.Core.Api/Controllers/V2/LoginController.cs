using Blog.Core.Model;
using Microsoft.AspNetCore.Mvc;
//using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WinRT.Core.Authorizations.Policys;
using WinRT.Core.Common;
using WinRT.Core.Helper;
using WinRT.Core.IServices;
using WinRT.Core.Model.VeiwModels;
using static WinRT.Core.Extensions.CustomApiVersion;
using static WinRT.Core.Helper.JwtHelper;

namespace WinRT.Core.Controllers.V2
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : Controller
    {
        private readonly ISysUserInfoServices _sysUserInfoServices;
        private readonly PermissionRequirement _requirement;
        private readonly IRoleModulePermissionServices _roleModulePermissionServices;


        public LoginController(
            ISysUserInfoServices sysUserInfoServices,
            IRoleModulePermissionServices roleModulePermissionServices,
            PermissionRequirement requirement
            )
        {
            _sysUserInfoServices = sysUserInfoServices;
            _roleModulePermissionServices = roleModulePermissionServices;
            _requirement = requirement;
        }

        /// <summary>
        ///  获取token令牌
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpGet]
        [CustomRoute(ApiVersions.V2, "GetJwtStr")]
        public async Task<object> GetJwtStr(string name, string pass)
        {
            string jwtStr = string.Empty;
            bool suc = false;

            // 获取用户的角色名，请暂时忽略其内部是如何获取的，可以直接用 var userRole="Admin"; 来代替更好理解。
            //var userRole = await _sysUserInfoServices.GetUserRoleNameStr(name, pass);

            var userRole = "Admin";
            if (userRole != null)
            {
                // 将用户id和角色名，作为单独的自定义变量封装进 token 字符串中。
                TokenModelJwt tokenModel = new TokenModelJwt { Uid = 1, Role = userRole };
                jwtStr = JwtHelper.IssueJwt(tokenModel);//登录，获取到一定规则的 Token 令牌
                suc = true;
            }
            else
            {
                jwtStr = "login fail!!!";
            }

            return Ok(new
            {
                success = suc,
                token = jwtStr
            });
        }

        [HttpPost]
        [CustomRoute(ApiVersions.V2, "GetJwtStr3.0")]
        public async Task<object> GetJWTToken3(string name = "", string pass = "")
        {
            string jwtStr = string.Empty;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
            {
                return new MessageModel<TokenInfoViewModel>()
                {
                    success = false,
                    msg = "用户名或密码不能为空",
                };
            }
            pass = MD5Helper.MD5Encrypt32(pass);

            // 验证是否有该用户
            var user = await _sysUserInfoServices.Query(d => d.uLoginName == name && d.uLoginPWD == pass && d.tdIsDelete == false);
            if (user.Count > 0)
            {
                // 查询用户拥有哪些角色
                var userRoles = await _sysUserInfoServices.GetUserRoleNameStr(name, pass);
                var claims = new List<Claim> {
                    // 添加用户
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString())
                };
                // 添加角色
                claims.AddRange(userRoles.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

                var data = await _roleModulePermissionServices.RoleModuleMaps();
                var list = (from item in data
                            where item.IsDeleted == false
                            orderby item.Id
                            select new PermissionItem
                            {
                                Url = item.Module?.LinkUrl,
                                Role = item.Role?.Name.ObjToString(),
                            }).ToList();

                _requirement.Permissions = list;

                try {
                    //  颁发token令牌
                    var token = JwtToken.BuildJwtToken(claims.ToArray(), _requirement);
                    return new MessageModel<TokenInfoViewModel>()
                    {
                        success = true,
                        msg = "获取成功",
                        response = token
                    };
                } catch (Exception ex) { 
                
                }
                return new MessageModel<TokenInfoViewModel>()
                {
                    success = false,
                    msg = "认证失败",
                };
            }
            else
            {
                return new MessageModel<TokenInfoViewModel>()
                {
                    success = false,
                    msg = "认证失败",
                };
            }
        }
    }
}
