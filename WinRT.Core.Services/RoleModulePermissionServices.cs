using System;
using System.Collections.Generic;
using System.Text;
using WinRT.Core.Services.Base;
using WinRT.Core.Model.Models;
using WinRT.Core.IServices;
using WinRT.Core.Repository.Base;
using System.Threading.Tasks;
using System.Linq;
using SqlSugar;

namespace WinRT.Core.Services
{
    public class RoleModulePermissionServices : BaseServices<RoleModulePermission>, IRoleModulePermissionServices
    {
        private readonly IBaseRepository<RoleModulePermission> _dal;
        private readonly IBaseRepository<Role> _roleRepository;
        private readonly IBaseRepository<Modules> _moduleRepository;
        
        // 将多个仓储接口注入
        public RoleModulePermissionServices(
            IBaseRepository<RoleModulePermission> dal,
            IBaseRepository<Role> roleRepository,
           IBaseRepository<Modules> moduleRepository)
        {
            this._dal = dal;
            this._moduleRepository = moduleRepository;
            this._roleRepository = roleRepository;
            base.BaseDal = dal;
        }

        /// <summary>
        /// 获取全部 角色接口(按钮)关系数据 注意我使用咱们之前的AOP缓存，很好的应用上了
        /// </summary>
        /// <returns></returns>
        // [Caching(AbsoluteExpiration = 10)]
        public async Task<List<RoleModulePermission>> GetRoleModule()
        {
            var roleModulePermissions = await base.Query(a => a.IsDeleted == false); // 
            var roles = await _roleRepository.Query(a => a.IsDeleted == false);
            var modules = await _moduleRepository.Query(a => a.IsDeleted == false);

            if (roleModulePermissions.Count > 0)
            {
                foreach (var item in roleModulePermissions)
                {
                    item.Role = roles.FirstOrDefault(d => d.Id == item.RoleId);
                    item.Module = modules.FirstOrDefault(d => d.Id == item.ModuleId);
                }

            }
            return roleModulePermissions;
        }

        public async Task<List<RoleModulePermission>> RoleModuleMaps()
        {
            return await QueryMuch<RoleModulePermission, Modules, Role, RoleModulePermission>(
               (rmp, m, r) => new object[] {
                    JoinType.Left, rmp.ModuleId == m.Id,
                    JoinType.Left,  rmp.RoleId == r.Id
               },

               (rmp, m, r) => new RoleModulePermission()
               {
                   Role = r,
                   Module = m,
                   IsDeleted = rmp.IsDeleted
               },

               (rmp, m, r) => rmp.IsDeleted == false && m.IsDeleted == false && r.IsDeleted == false
               );
        }
    }
}
