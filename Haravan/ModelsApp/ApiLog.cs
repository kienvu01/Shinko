using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.ModelsApp
{
    public class ApiLog
    {
        public static void log(string title, object header, object body, object noidung,string key)
        {
            try
            { 
                string h = Newtonsoft.Json.JsonConvert.SerializeObject(header);
                string sql = $"insert into apilog values(GETDATE(),N'{title}',N'{h}',N'{body.ToString().Replace("'", "")}',N'{noidung.ToString().Replace("'","")}','{key}','')";
                DAL.DAL_SQL_SYS.ExecuteNonquery(sql);
            }
            catch (Exception e)
            {
            }
        }
        public static void logval(string title, string val)
        {
            try
            {
                string sql = $"insert into apilog values(GETDATE(),N'{title}',N'{val.Replace("'", "")}','','','','')";
                DAL.DAL_SQL_SYS.ExecuteNonquery(sql);
            }
            catch (Exception e)
            {
            }
        }
    }
}
