using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT.Core.IServices;
using WinRT.Core.Model.Models;
using WinRT.Core.Model.VeiwModels;
using WinRT.Core.Repository.Base;
using WinRT.Core.Services.Base;

namespace WinRT.Core.Services
{
    public class BlogArticleServices : BaseServices<BlogArticle>, IBlogArticleServices
    {
        private readonly IBaseRepository<BlogArticle> _dal;
        private readonly IMapper _mapper;

        public BlogArticleServices(IBaseRepository<BlogArticle> dal, IMapper mapper)
        {
            _dal = dal;
            base.BaseDal = dal;
            this._mapper = mapper;
        }

        public async Task<BlogViewModels> getBlogDetails(int id)
        {
            var bloglist = await _dal.Query(a => a.bID > 0, a => a.bID);
            var blogArticle = (await _dal.Query(a => a.bID == id)).FirstOrDefault();
            BlogViewModels models = null;

            if (blogArticle != null)
            {
                BlogArticle prevblog;
                BlogArticle nextblog;
                int blogIndex = bloglist.FindIndex(item => item.bID == id);
                if (blogIndex >= 0)
                {
                    try
                    {
                        // 上一篇
                        prevblog = blogIndex > 0 ? (((BlogArticle)(bloglist[blogIndex - 1]))) : null;
                        // 下一篇
                        nextblog = blogIndex + 1 < bloglist.Count() ? (BlogArticle)(bloglist[blogIndex + 1]) : null;

                        // 注意就是这里,mapper
                        models = _mapper.Map<BlogViewModels>(blogArticle);

                        if (nextblog != null)
                        {
                            models.next = nextblog.btitle;
                            models.nextID = nextblog.bID;
                        }
                        if (prevblog != null)
                        {
                            models.previous = prevblog.btitle;
                            models.previousID = prevblog.bID;
                        }
                    }
                    catch (Exception) { }
                }
                blogArticle.btraffic += 1;
                await _dal.Update(blogArticle, new List<string> { "btraffic" });
            }

            return models;
        }
    }
}
