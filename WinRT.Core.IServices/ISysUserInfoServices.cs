using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WinRT.Core.IServices.Base;
using WinRT.Core.Model.Models;

namespace WinRT.Core.IServices
{
    public interface ISysUserInfoServices: IBaseServices<SysUserInfo>
    {
        Task<string> GetUserRoleNameStr(string loginName, string loginPwd);

    }
}
