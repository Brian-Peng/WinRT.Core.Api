using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinRT.Core.Model.Models;
using WinRT.Core.Model.VeiwModels;

namespace WinRT.Core.Api.AutoMapper
{
    /// <summary>
    /// services.AddAutoMapper是会自动找到所有继承了Profile的类然后进行配置，如果不想一个一个的配置，可以用接口的形式，批量导入
    /// </summary>
    public class CustomProfile: Profile
    {
        /// <summary>
        /// 配置构造函数，用来创建关系映射
        /// </summary>
        public CustomProfile()
        {
            // 第一个参数是原对象，第二个是目的对象，
            CreateMap<BlogArticle, BlogViewModels>();
        }
    }
}
