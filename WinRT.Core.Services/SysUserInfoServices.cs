using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT.Core.Repository.Base;
using WinRT.Core.IServices;
using WinRT.Core.Model.Models;
using WinRT.Core.Services.Base;

namespace WinRT.Core.Services
{
    public class SysUserInfoServices: BaseServices<SysUserInfo>, ISysUserInfoServices
    {
        private readonly IBaseRepository<SysUserInfo> _dal;
        private readonly IBaseRepository<Role> _roleRepository;
        private readonly IBaseRepository<UserRole> _userRoleRepository;

        public SysUserInfoServices(
            IBaseRepository<SysUserInfo> dal,
            IBaseRepository<Role> roleRepository,
            IBaseRepository<UserRole> userRoleRepository
            )
        {
            _dal = dal;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            base.BaseDal = dal;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="loginPwd"></param>
        /// <returns></returns>
        public async Task<string> GetUserRoleNameStr(string loginName, string loginPwd)
        {
            string roleName = "";
            // 验证用户是否存在
            var user = (await base.Query(a => a.uLoginName == loginName && a.uLoginPWD == loginPwd)).FirstOrDefault();
           
            var roleList = await _roleRepository.Query(a => a.IsDeleted == false);
            if (user != null)
            {
                var userRoles = await _userRoleRepository.Query(ur => ur.UserId == user.uID);
                if (userRoles.Count > 0)
                {
                    var arr = userRoles.Select(ur => ur.RoleId.ObjToString()).ToList();
                    var roles = roleList.Where(d => arr.Contains(d.Id.ObjToString()));

                    roleName = string.Join(",", roles.Select(r => r.Name).ToArray());
                }
            }
            return roleName;
        }

    }
}
