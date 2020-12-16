using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WinRT.Core.Repository;
using WinRT.Core.IServices;
using WinRT.Core.Model.Models;
using WinRT.Core.Services.Base;
using WinRT.Core.Repository.Base;
using WinRT.Core.Common.Attributes;

namespace WinRT.Core.Services
{
    public class AdvertisementServices : BaseServices<Advertisement>, IAdvertisementServices
    {
        private readonly IBaseRepository<Advertisement> _dal;

        public AdvertisementServices(IBaseRepository<Advertisement> dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
        }
        
        [Caching(AbsoluteExpiration =10)]
        public async Task<IList<Advertisement>> GetAdvertisement(int id)
        {
            return await _dal.Query(o => o.Id == id);
        }
    }
}
