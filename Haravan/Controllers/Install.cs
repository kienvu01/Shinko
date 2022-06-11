using Haravan.ModelsApp;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Haravan.FuncLib;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Haravan.Model;
using System.Security.Claims;
using System.Net;
using System.Net.Http;
using Haravan.ModelVc;
using Microsoft.Data.SqlClient;
using System.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Haravan.Controllers
{ 
    [Route("install")]
    [ApiController]
    public class Install : ControllerBase
    {
        //protected AppDBContext db;
        private readonly IConfiguration _config;

        public Install(IConfiguration config)
        {
            _config = config;
        }
        // GET: api/<Install>
        [HttpGet]
        [Route("login")]
        public IActionResult Get()
        {
            Object obj = new
            {
                test = "aaa",
            };
            ILog log = Logger.GetLog(typeof(Install));
            log.Info("User Login Get");
            log.Info(Request.Body);
            log.Info(Request.Headers);
            ResponseData res = new ResponseData("ok", "ok", obj);
            return res.Ok(this);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Post([FromForm] string code, [FromForm] string id_token, [FromForm] string scope, [FromForm] string session_state)
        {
            Object obj = new
            {
                test = "aaa login",
            };
            ILog log = Logger.GetLog(typeof(Install));
            log.Info("User Login Post");
            log.Info(Request.Query);
            log.Info(Request.Headers);
            log.Info(Request.Form);
            log.Info(id_token);
            log.Info(code);
            ResponseData res = new ResponseData("ok", "ok", obj);
            return res.Ok(this);
        }
        [HttpGet]
        [Route("subsribe")]
        public async Task<IActionResult> SubcribeWebhook()
        {
            string token = _config.GetValue<string>("config_Haravan:access_token");
            string url =  _config.GetValue<string>("config_Haravan:webhook:subscribe");
            HttpClientApp client = new HttpClientApp(token);

            ResponseApiHaravan res = await client.Post_Request_WithBody(url,"");
            if(res.status == "ok")
                return Ok(new ResponseData("ok","",""));
            else
                return Ok(new ResponseData("err", res.message, res.data));
        }

        [HttpPost]
        [Route("osystemervice")]
        public IActionResult Osystemervice()
        {
            Object obj = new
            {
                test = "aaa GetOsystemervice",
            };
            ILog log = Logger.GetLog(typeof(Install));
            log.Info("User GeOsystemervice Post");
            log.Info(Request.ContentType);
            log.Info(Request.Query);
            ResponseData res = new ResponseData("ok", "ok", obj);
            return res.Ok(this);
        }
        [HttpGet]
        [Route("osystemervice")]
        public  IActionResult GetOsystemervice()
        {
            Object obj = new
            {
                test = "aaa get GetOsystemervice",
            };
            ILog log = Logger.GetLog(typeof(Install));
            log.Info("User GetOsystemervice");
            log.Info(Request.ContentType);
            log.Info(Request.Query);
            ResponseData res = new ResponseData("ok", "ok", obj);
            return res.Ok(this);
        }
        //[HttpGet]
        //[Route("test2")]
        //public IActionResult Test2(HttpRequestMessage request)
        //{
        //    string hostName = Dns.GetHostName();
        //    var tt = Dns.GetHostByName(hostName).AddressList;
        //    string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
        //    ILog log = Logger.GetLog(typeof(Install));
        //    log.Info("-------------------------Test Get IP--------------------");
        //    log.Info("host:"+hostName);
        //    log.Info("ip:"+myIP);
        //    for (int i = 0; i < tt.Length; i++)
        //    {
        //        log.Info("add"+i+":" + tt[i].ToString());
        //    }
        //    //int s = (int)Request.Host.Port;
        //    string s2 = Request.Protocol;
        //    var s3 = Request.HttpContext.Connection.RemoteIpAddress;
        //    log.Info(s2);
        //    log.Info(s3);
        //    log.Info(Request.Headers);
        //    ResponseData res = new ResponseData("ok", "ok", "");
        //    return res.Ok(this);
        //}

        // GET api/<Install>/5
        [Route("ConvertDate")]
        [HttpPost]
        public IActionResult ConvertDate(Object data)
        {
            ApiLog.logval("test","day thanh cong");
            return Ok();
        }
        [Route("zzzz")]
        [HttpPost]
        public async Task<IActionResult> zzzz([FromBody]Object val)
        {
            try
            {
                JObject temp = JObject.FromObject(val);
                var token = _config.GetValue<string>("x_sse_code");
                string url = $"https://api-eva_haravan.sse.net.vn/install/ConvertDate";
                HttpClientApp client = new HttpClientApp();
                client.AddHeader("x-sse-code", token);
                ResponseApiHaravan res =await client.Post_Request_WithBody(url, temp.ToString());
                return Ok(res);
            }
            catch(Exception e)
            {
                return Ok(e.Message);
            }
        }
        [Route("test")]
        [HttpGet]
        public async Task<IActionResult> GetTest()
        {
            //var token = _config.GetValue<string>("config_Haravan:private_token");
            //string url = $"https://apis.haravan.com/com/orders/1212388646.json";
            //HttpClientApp client = new HttpClientApp(token);
            //ResponseApiHaravan res = await client.Get_Request(url);
            //JObject data = JObject.Parse(res.data);
            //JObject order = (JObject)data["order"];

            //DateTime myDate = Convert.ToDateTime((string)order["created_at"] ?? "");
            //var easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            //myDate = TimeZoneInfo.ConvertTimeFromUtc(myDate, easternZone);

            //string ngay_ct = myDate.ToString("yyyy/MM/dd hh:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));

            //return Ok(ngay_ct);


            return Ok("Phiên bản 19/10/2021 : 1.0.35");
        }
        [Route("CheckConnect")]
        [HttpGet]
        public async Task<IActionResult> CheckConnectss()
        {
            string name = "ConnectionDBAPP";
            string name2 = "ConnectionDBSYS";
            try
            {
                using (SqlConnection conn = new SqlConnection(MyAppData.config.GetConnectionString(name)))
                {
                    conn.Open();
                    object obj = new
                    {
                        ok = "ok",
                        constring = ""
                    };
                    return Ok(obj);
                }
            }
            catch (Exception ex)
            {
                object obj = new
                {
                    ok = ex.Message,
                    ss = ex.ToString(),
                    constring =""
                };
                return Ok(obj);
            }


            return Ok("Phiên bản 21/08/2021 : 1.1.29");
        }
        [Route("test2")]
        [HttpGet]
        public async Task<IActionResult> GetTest2()
        {
            DAL.DAL_SQL_SYS.SetConnectionString(MyAppData.config.GetConnectionString("ConnectionDBAPP"));

            string sql = "select top 1 val from tbl_test";
            DataSet ds =  DAL.DAL_SQL_SYS.ExecuteGetlistDataset(sql);
            string s = ds.Tables[0].Rows[0]["val"].ToString();

            return Ok(s);
        }
    }
}
