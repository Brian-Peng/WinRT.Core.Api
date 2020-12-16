using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WinRT.Core.IServices.Base;
using WinRT.Core.Model.Models;

namespace WinRT.Core.IServices
{
    public interface IRoleModulePermissionServices: IBaseServices<RoleModulePermission>
    {
        /// <summary>
        /// 获取全部 角色接口(按钮)关系数据 注意我使用咱们之前的AOP缓存，很好的应用上了
        /// </summary>
        /// <returns></returns>
        Task<List<RoleModulePermission>> GetRoleModule();

        /// <summary>
        /// 角色权限Map
        /// RoleModulePermission, Module, Role 三表联合
        /// 第四个类型 RoleModulePermission 是返回值
        /// </summary>
        /// <returns></returns>
        Task<List<RoleModulePermission>> RoleModuleMaps();
    }
}
