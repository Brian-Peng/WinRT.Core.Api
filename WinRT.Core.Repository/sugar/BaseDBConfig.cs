using System;
using System.Collections.Generic;
using System.Text;

namespace WinRT.Core.Repository.sugar
{
    public class BaseDBConfig
    {
        // 我用配置文件的形
        //public static string ConnectionString = File.ReadAllText(@"D:\my-file\dbCountPsw1.txt").Trim();

        //正常格式是

        // public static string ConnectionString = "server=;uid=sa;pwd=sa;database=BlogDB"; 

        //Data Source = (localdb)\\MSSQLLocalDB;Initial Catalog = WMBLOG_MSSQL_2; Integrated Security = True; Connect Timeout = 30; Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False
        public static string ConnectionString = "Server=DESKTOP-UJR0PVS\\MSSQLSERVER01; Database=WMBLOG_MSSQL_1;  Trusted_Connection=SSPI;";
    }
}
