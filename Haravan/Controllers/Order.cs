using Haravan.FuncLib;
using Haravan.Model;
using Haravan.ModelsApp;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Controllers
{
    [Route("Order")]
    [ApiController]
    public class Order : ControllerBase
    {
        private readonly IConfiguration _config;


        public Order(IConfiguration config)
        {
            _config = config;
        }
        [HttpPost]
        [Route("UpdateAllOrder")]
        public async Task<IActionResult> DongBo_Hoadon([FromBody] Object data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);
                string s = data.ToString();
                JObject obj = JObject.Parse(s);
                DateTime fdate = Convert.ToDateTime((string)obj["fromDate"]);
                DateTime tdate = Convert.ToDateTime((string)obj["toDate"]).AddDays(1);
                string so_ct = (string)obj["so_ct"] ?? "";
                string id = (string)obj["id"] ?? "";
                ModelsApp.Orders cus = new Orders(_config);
                ResponseData res = await cus.UpdateAllOrder(fdate, tdate,so_ct);
                if (res.status == "ok") return Ok(res);
                else
                {
                    ILog log = Logger.GetLog(typeof(Customer));

                    log.Error(res.message);
                    return StatusCode(500);
                }
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));

                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        [Route("GetHooksHistory")]
        public async Task<IActionResult> GEtHooksHistory([FromBody] Object data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);
                string s = data.ToString();
                JObject obj = JObject.Parse(s);
                DateTime fdate = Convert.ToDateTime((string)obj["fromDate"]);
                DateTime tdate = Convert.ToDateTime((string)obj["toDate"]).AddDays(1);
                ModelsApp.Orders cus = new Orders(_config);
                ResponseData res = await cus.Gethistory(fdate, tdate);
                if (res.status == "ok") return Ok(res);
                else
                {
                    ILog log = Logger.GetLog(typeof(Customer));

                    log.Error(res.message);
                    return StatusCode(500);
                }
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));

                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        [Route("GetDsPhuong")]
        public async Task<IActionResult> GetDsPhuong([FromBody] Object data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);
                string sql = "select * from (select ma_tinh,a.ma_quan,a.ten_quan,a.ten_quan2,a.ghn_ma_quan,(select count(ma_phuong) from hrdmphuong where ma_quan =a.ma_quan) as ss from hrdmquan a)t where t.ss=0 and isnull(t.ghn_ma_quan,'')<>''";

                string sql2 = "";
                var token = _config.GetValue<string>("Vanchuyen:GHN:privateToken");
                DataSet ds = DAL.DAL_TEST.ExecuteGetlistDataset(sql);
                foreach(DataRow r in ds.Tables[0].Rows)
                {
                    int count = 0;
                    string har_ma_quan = r["ghn_ma_quan"].ToString();
                    string ma_tinh = r["ma_tinh"].ToString();
                    string ma_quan = r["ma_quan"].ToString();
                    string url = $"https://online-gateway.ghn.vn/shiip/public-api/master-data/ward";
                    HttpClientApp client = new HttpClientApp();
                    client.AddHeader("Token", token);
                    Object body = new
                    {
                        district_id = Convert.ToInt32(har_ma_quan)
                    };
                    JObject temp = JObject.FromObject(body);

                    ResponseApiHaravan res = await client.Get_Request_WithBody(url, temp.ToString());
                    double total = 0;
                    if (res.status == "ok")
                    {
                        JObject root_total = JObject.Parse(res.data);
                        if (!Library.IsNullOrEmpty(root_total["data"]))
                        {
                            JArray lst_phuong = (JArray)root_total["data"];
                            for (int i = 0; i < lst_phuong.Count; i++)
                            {
                                count++;
                                string tempss = ma_quan.Trim() + count.ToString().PadLeft(3, '0');
                                JObject phuong = (JObject)lst_phuong[i];

                                string ten = (string)phuong["WardName"] ?? "";
                                string code = (string)phuong["WardCode"] ?? "";
                                sql2 += Environment.NewLine + $"insert into hrdmphuong2";
                                sql2 += Environment.NewLine + $"select '{ma_tinh}','{ma_quan}','{tempss}',N'{ten}',N'{ten}',0,1,GETDATE(),GETDATE(),NULL,NULL,'{code}',''";
                            }
                        }
                        
                    }
                }
                DAL.DAL_TEST.ExecuteNonquery(sql2);
                return Ok();
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));

                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder([FromBody] API_Data_Order_UpdateOrder data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);
                DateTime mydate = Convert.ToDateTime(data.toDate);
                mydate = Convert.ToDateTime(data.toDate).AddDays(1);
                data.toDate = mydate.ToString("yyyy/MM/dd");
                ModelsApp.Orders cus = new Orders(_config);
                ResponseData res = await cus.UpdateOrderMissing(data);
                if (res.status == "ok") return Ok(res);
                else
                {
                    ILog log = Logger.GetLog(typeof(Customer));

                    log.Error(res.message);
                    return StatusCode(500);
                }
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));

                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }


        [HttpPost]
        [Route("UpdateDiachi")]
        public async Task<IActionResult> UpdateDiachi([FromBody] API_Data_Order_UpdateOrder data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);
                DateTime mydate = Convert.ToDateTime(data.fromDate);
                mydate = Convert.ToDateTime(data.toDate).AddDays(1);
                data.toDate = mydate.ToString("yyyy/MM/dd");
                ModelsApp.Orders cus = new Orders(_config);
                ResponseData res = await cus.UpdateDiaChi(data);
                if (res.status == "ok") return Ok(res);
                else
                {
                    ILog log = Logger.GetLog(typeof(Customer));

                    log.Error(res.message);
                    return StatusCode(500);
                }
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));

                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        [Route("UpdateNoteOrderHaravan")]
        public async Task<IActionResult> UpdateNote([FromBody] Object data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);
                string s = data.ToString();
                JObject obj = JObject.Parse(s);

                string id = (string)obj["id"] ?? "";
                string  note = (string)obj["note"] ?? "";
                ModelsApp.Orders cus = new Orders(_config);
                ResponseData res = await cus.UpdateNote(id, note);
                if (res.status == "ok") return Ok(res);
                else
                {
                    ILog log = Logger.GetLog(typeof(Customer));

                    log.Error(res.message);
                    return StatusCode(500);
                }
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));

                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        [Route("Test")]
        public IActionResult Test([FromBody] Object data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);
;
                ModelsApp.Orders cus = new Orders(_config);
                string s = data.ToString();
                JObject obj = JObject.Parse(s);
                string so_ct = ((string)obj["order_number"] ?? "");
                CheckOrderExit res = cus.CheckOrder(so_ct);
                    return Ok(res);
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));

                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        [Route("SSEActiveOrder")]
        public async Task<IActionResult> SSEActiveOrder([FromBody] Object data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);
                string s = data.ToString();
                JObject obj = JObject.Parse(s);

                string stt_rec = (string)obj["stt_rec"] ?? "";
                string tbl_sufix = (string)obj["table_sufix"] ?? "";
                ModelsApp.Orders cus = new Orders(_config);
                ResponseData res = await cus.CreatFulliment(stt_rec, tbl_sufix);
                return Ok(res);

            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));

                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        [Route("CancelOrderHaravan")]
        public async Task<IActionResult> CancelOrderHaravan([FromBody] Object data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);
                string s = data.ToString();
                JObject obj = JObject.Parse(s);

                string stt_rec = (string)obj["stt_rec"] ?? "";
                string tbl_sufix = (string)obj["table_sufix"] ?? "";
                
                ModelsApp.Orders cus = new Orders(_config);
                ResponseData res = await cus.CancelOrderHaravan(stt_rec, tbl_sufix);
                return Ok(res);

            }
            catch (Exception e)
            {

                return StatusCode(500, e.Message);
            }
        }


    }
}
