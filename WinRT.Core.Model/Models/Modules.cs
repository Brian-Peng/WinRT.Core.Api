using System;
using System.Collections.Generic;
using System.Text;

namespace WinRT.Core.Model.Models
{
    /// <summary>
    /// 接口API地址信息表
    /// </summary>
    public class Modules
    {
        public int Id { get; set; }
        /// <summary>
        /// 父ID
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// API链接地址
        /// </summary>
        public string LinkUrl { get; set; }
        /// <summary>
        /// 控制器名称
        /// </summary>
        public string Controller { get; set; }
        /// <summary>
        /// Action名称
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 菜单编号
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderSort { get; set; }
        /// <summary>
        /// /描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///获取或设置是否禁用，逻辑上的删除，非物理删除
        /// </summary>
        public bool? IsDeleted { get; set; }
        // 等等其他属性，具体的可以看我的Code，或者自己自定义...
    }
}
