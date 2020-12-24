using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WinRT.Core.IServices.Base;
using WinRT.Core.Model.Models;
using WinRT.Core.Model.VeiwModels;

namespace WinRT.Core.IServices
{
    public interface IBlogArticleServices: IBaseServices<BlogArticle>
    {
        /// <summary>
        /// 获取视图博客详情信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<BlogViewModels> getBlogDetails(int id);
    }
}
