using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinRT.Core.Models
{
    /// <summary>
    /// 接口、角色的中间表（以后可以把按钮设计进来）
    /// </summary>
    public class RoleModulePermission
    {
        public int Id { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// 菜单ID，这里就是api地址的信息
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// 按钮ID
        /// </summary>
        public int? PermissionId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 获取或设置是否禁用，逻辑上的删除，非物理删除
        /// </summary>
        public bool? IsDeleted { get; set; }

        // 等等，还有其他属性，其他的可以参考Code，或者自定义...
    }
}
