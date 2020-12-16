using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WinRT.Core.IServices.Base;
using WinRT.Core.Model.Models;

namespace WinRT.Core.IServices
{
    public interface IAdvertisementServices: IBaseServices<Advertisement>
    {
        Task<IList<Advertisement>> GetAdvertisement(int id);
    }
}
