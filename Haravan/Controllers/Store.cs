using Haravan.FuncLib;
using Haravan.Model;
using Haravan.ModelsApp;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Controllers
{
    public class Store_UpdateLineItem
    {
        public string product_id { get; set; }
        public string product_variant_id { get; set; }
        public decimal quantity { get; set; }
        public Store_UpdateLineItem(string s1,string s2, decimal s3)
        {
            product_id = s1;
            product_variant_id = s2;
            quantity = s3;
        }

    }
    [Route("Store")]
    [ApiController]
    public class Store : ControllerBase
    {
        private readonly IConfiguration _config;


        public Store(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        [Route("UpdateItem")]
        public async Task<IActionResult> DongBo_KhoTheoItem([FromBody] Res_UpdateItem data)
        {
            bool check = Library.CheckAuthentication(_config, this);
            if (!check) return StatusCode(401);
            var constring = _config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            conn.Open();
            try
            {
                if (data.ma_kho == null || data.ma_kho.Trim() == "") return Ok(new ResponseData("err", "Yêu cầu nhập kho", ""));
                if (data.line_items == null || data.line_items.Count == 0) return Ok(new ResponseData("err", "Yêu cầu nhập danh sách vật tư", ""));
                ResponseData reLocation = Library.GetIdStoreHaravan(data.ma_kho);
                if (reLocation.status == "err") return Ok(reLocation);
                string locationId = reLocation.data;

                Products p = new Products(_config);

               
                List<Store_UpdateLineItem> lineItem = new List<Store_UpdateLineItem>();
               
                foreach(string item in data.line_items)
                {
                    ResponseData res_product = await p.GETVarianFromBarCode(item);
                    if (res_product.status == "ok")
                    {
                        JObject data_product = JObject.FromObject(res_product.data);
                        decimal quantity = Library.GetTonKhoTheoItem( item,data.ma_kho);
                        lineItem.Add(new Store_UpdateLineItem(((string)data_product["product_id"] ?? ""),
                            ((string)data_product["product_variant_id"] ?? ""), quantity));
                    }
                }
                Object body = new
                {
                    inventory = new 
                     {
                        location_id = locationId,
                        type = "set",
                        reason = "shrinkage",
                        note = "",
                        line_items = lineItem
                    }
                };
                JObject temp = JObject.FromObject(body);
                var token = _config.GetValue<string>("config_Haravan:private_token");
                string url = $"https://apis.haravan.com/com/inventories/adjustorset.json";
                HttpClientApp client = new HttpClientApp(token);
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString()); 
                if (res.status == "ok") return Ok(res);
                else
                {
                    ILog log = Logger.GetLog(typeof(Customer));
                    log.Error(res.message);
                    return StatusCode(500);
                }
                conn.Close();
                conn.Dispose();
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                ILog log = Logger.GetLog(typeof(Customer));

                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        [Route("UpdateItemByStore")]
        public async Task<IActionResult> DongBo_Kho([FromBody] Object data)
        {
            bool checkf = Library.CheckAuthentication(_config, this);
            if (!checkf) return StatusCode(401);
            try
            {
                string s = data.ToString();
                JObject obj = JObject.Parse(s);
                //string type = ((string)obj["type"] ?? "set");

                //string note = ((string)obj["note"] ?? "");
                Products p = new Products(_config);

                string ma_kho = (string)obj["ma_kho"] ?? "";
                string locationId = _config.GetValue<string>("config_Haravan:store_online");
                string sqlgetitem = "";
                if (ma_kho.Trim() == "")
                {
                    sqlgetitem = "SELECT  distinct  a.ma_vt,isnull(b.s1,'')s1,isnull(b.s2,'')s2 FROM cdvt213 a left join dmvt b on a.ma_vt=b.ma_vt  where  ISNULL(a.ma_kho,'')<>'' AND a.ma_kho LIKE 'TP_CH%'  and a.ma_kho <>'TP_CH015'";
                    sqlgetitem += Environment.NewLine + $"select * from dmkho a where a.ma_kho LIKE 'TP_CH%' and a.ma_kho<> 'TP_TPA'  and a.ma_kho <>'TP_CH015' ";
                }
                else
                {
                    List<string> listkho = ma_kho.Split(",").ToList();
                    if (listkho.Count == 0) return StatusCode(200, new ResponseData("err", "Danh sách kho nhập không hợp lệ", ""));
                    string strkho = " and a.ma_kho in (";
                    for( int k = 0; k < listkho.Count; k++)
                    {
                        strkho += $"'{listkho[k].Trim()}',";
                    }
                    strkho += $"'{listkho[listkho.Count - 1].Trim()}')";
                    sqlgetitem = $"SELECT  distinct  a.ma_vt,isnull(b.s1,'')s1,isnull(b.s2,'')s2 FROM cdvt213 a left join dmvt b on a.ma_vt=b.ma_vt  where  ISNULL(a.ma_kho,'')<>'' AND a.ma_kho LIKE 'TP_CH%' and a.ma_kho<> 'TP_TPA'  and a.ma_kho <>'TP_CH015' {strkho} ";
                    sqlgetitem += Environment.NewLine + $"select * from dmkho a where a.ma_kho  LIKE 'TP_CH%' and a.ma_kho<> 'TP_TPA'  and a.ma_kho <>'TP_CH015'  {strkho}";
                }


                List<Res_GETVarian> lst_varian = new List<Res_GETVarian>();
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sqlgetitem);
                string update_vt = "";
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    Res_GETVarian temp_v = new Res_GETVarian();
                    temp_v.ma_vt = r["ma_vt"].ToString().Trim();

                    if (r["s1"].ToString().Trim()=="" || r["s1"].ToString().Trim() == "")
                    {
                        ResponseData res_product = await p.GETVarianFromBarCode(temp_v.ma_vt);
                        if (res_product.status == "ok")
                        {
                            JObject data_product = JObject.FromObject(res_product.data);
                            temp_v.product_variant_id = (string)data_product["product_variant_id"] ?? "";
                            temp_v.product_id = (string)data_product["product_id"] ?? "";
                            lst_varian.Add(temp_v);
                            update_vt += Environment.NewLine + $"update dmvt set s1='{temp_v.product_variant_id}',s2='{temp_v.product_id}' where ma_vt='{temp_v.ma_vt}'";
                        }
                    }
                    else
                    {
                        temp_v.product_variant_id = r["s1"].ToString().Trim();
                        temp_v.product_id = r["s2"].ToString().Trim();
                        lst_varian.Add(temp_v);
                    }
                }
                if (update_vt !="") DAL.DAL_SQL.ExecuteNonquery(update_vt);
                var token = _config.GetValue<string>("config_Haravan:private_token");
                string url = "";
                HttpClientApp client = new HttpClientApp(token);
              
                List<string> dskho_ok = new List<string>();
                List<string> dskho_err = new List<string>();
                foreach (DataRow r in ds.Tables[1].Rows)
                {
                    string kho = r["ma_kho"].ToString().Trim();
                    string idkho = r["s1"].ToString().Trim();
                    List<List<Store_UpdateLineItem>> lineItems = new List<List<Store_UpdateLineItem>>();
                    List<Store_UpdateLineItem> temp_line = new List<Store_UpdateLineItem>();

                    string sqlcdvt = $"exec haravan_getTonKho_ByKho '{kho}'";
                    DataSet ds_cdvt = DAL.DAL_SQL.ExecuteGetlistDataset(sqlcdvt);

                    foreach (DataRow cdvt in ds_cdvt.Tables[0].Rows)
                    {
                        string barcode = cdvt["ma_vt"].ToString().Trim();
                        decimal quantity = (decimal)cdvt["ton13"];
                        for (int j = 0; j < lst_varian.Count; j++)
                        {
                            if (barcode.Trim() == lst_varian[j].ma_vt.Trim())
                            {
                                if (temp_line.Count < 200)
                                    temp_line.Add(new Store_UpdateLineItem(lst_varian[j].product_id, lst_varian[j].product_variant_id, quantity));
                                else
                                {
                                    lineItems.Add(temp_line);
                                    temp_line = new List<Store_UpdateLineItem>();
                                    temp_line.Add(new Store_UpdateLineItem(lst_varian[j].product_id, lst_varian[j].product_variant_id, quantity));
                                }
                            }
                        }
                    }
                    lineItems.Add(temp_line);
                    ResponseApiHaravan ress = new ResponseApiHaravan("ok", "", "");
                    foreach (List<Store_UpdateLineItem> line in lineItems)
                    {
                        url = $"https://apis.haravan.com/com/inventories/adjustorset.json";
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            Object body = new
                            {
                                inventory = new
                                {
                                    location_id = idkho,
                                    type = "set",
                                    reason = "shrinkage",
                                    note = "Update by SSE",
                                    line_items = line
                                }
                            };
                            JObject temp = JObject.FromObject(body);

                            ress = await client.Post_Request_WithBody(url, temp.ToString());
                            if (ress.status == "err")
                            {
                                System.Threading.Thread.Sleep(2000);
                                ress = await client.Post_Request_WithBody(url, temp.ToString());
                            }
                        }
                    }
                    if (ress.status == "ok") dskho_ok.Add(kho);
                    else dskho_err.Add(kho);
                }
                var bodyre = new
                {
                    ds_ok =dskho_ok,
                    ds_err = dskho_err
                };
                return StatusCode(200, new ResponseData("ok", "ok", bodyre));
            }
            catch (Exception e)
            {
                ApiLog.logval("log_UpdateItemByKho", e.Message);
                ILog log = Logger.GetLog(typeof(Customer));

                log.Error(Request.Path);
                return StatusCode(200, new ResponseData("err", e.Message, ""));
            }
        }

        //[HttpPost]
        //[Route("UpdateInventoryListItem")]
        //public async Task<IActionResult> UpdateInventoryListItem([FromBody] Req_UpdateInventoryItem data)
        //{
        //    bool checkf = Library.CheckAuthentication(_config, this);
        //    if (!checkf) return StatusCode(401);
        //    try
        //    {
        //        string lst_vt = "";
        //        foreach (string temp in data.lst_ma_vt) lst_vt += temp + ",";

        //        var token = _config.GetValue<string>("config_Haravan:private_token");
        //        string sqlgetitem = "";
        //        if (lst_vt == "")
        //            sqlgetitem = "SELECT  distinct  a.ma_vt,isnull(b.s1,'')s1,isnull(b.s2,'')s2,c.s1 as id_location,a.ton13 FROM cdvt213 a left join dmvt b on a.ma_vt=b.ma_vt left join dmkho c on a.ma_kho = c.ma_kho  where  ISNULL(c.s1,'')<>'' order by a.ma_kho";
        //        else
        //        {
        //            lst_vt += "''";
        //            sqlgetitem = $"SELECT  distinct  a.ma_vt,isnull(b.s1,'')s1,isnull(b.s2,'')s2,c.s1 as id_location,a.ton13 FROM cdvt213 a left join dmvt b on a.ma_vt=b.ma_vt left join dmkho c on a.ma_kho = c.ma_kho  where  ISNULL(c.s1,'')<>'' and a.ma_vt in ({lst_vt}) order by a.ma_kho";
        //        }
        //        List<Res_GETVarian> lst_varian = new List<Res_GETVarian>();
        //        DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sqlgetitem);

        //        string url = "";
        //        HttpClientApp client = new HttpClientApp(token);
        //        List<List<Store_UpdateLineItem>> lineItems = new List<List<Store_UpdateLineItem>>();
        //        string ma_kho = "";
        //        ResponseApiHaravan ress = new ResponseApiHaravan("ok", "", "");
        //        foreach (DataRow r in ds.Tables[0].Rows)
        //        {
        //            // Đổi kho thì update inventory 
        //            if(r["ma_kho"].ToString().Trim() != ma_kho && ma_kho !="")
        //            {
        //                foreach (List<Store_UpdateLineItem> line in lineItems)
        //                {
        //                    url = $"https://apis.haravan.com/com/inventories/adjustorset.json";                   
        //                    Object body = new
        //                    {
        //                        inventory = new
        //                        {
        //                            location_id = ma_kho,
        //                            type = "set",
        //                            reason = "shrinkage",
        //                            note = "Update by SSE",
        //                            line_items = line
        //                        }
        //                    };
        //                    JObject temp = JObject.FromObject(body);
        //                    ress = await client.Post_Request_WithBody(url, temp.ToString());
        //                    //Tránh trường hợp ngẽn cổ chai api ->Sleep 2s rồi gởi lại
        //                    if (ress.status == "err")
        //                    {
        //                        System.Threading.Thread.Sleep(2000);
        //                        ress = await client.Post_Request_WithBody(url, temp.ToString());
        //                    }
        //                    if (ress.status == "ok")
        //                    {

        //                    }
        //                }

        //            }
        //            Res_GETVarian temp_v = new Res_GETVarian();
        //            temp_v.ma_vt = r["ma_vt"].ToString().Trim();

        //            if (r["s1"].ToString().Trim() == "" || r["s2"].ToString().Trim() == "")
        //            { }
        //            else
        //            {
        //                temp_v.product_variant_id = r["s1"].ToString().Trim();
        //                temp_v.product_id = r["s2"].ToString().Trim();
        //                lst_varian.Add(temp_v);
        //            }
        //        }
        //        var token = _config.GetValue<string>("config_Haravan:private_token");
        //        string url = "";
        //        HttpClientApp client = new HttpClientApp(token);

        //        List<string> dsitem_ok = new List<string>();
        //        List<string> dsitem_err = new List<string>();
        //        foreach (DataRow r in ds.Tables[1].Rows)
        //        {
        //            string kho = r["ma_kho"].ToString().Trim();
        //            string idkho = r["s1"].ToString().Trim();
        //            List<List<Store_UpdateLineItem>> lineItems = new List<List<Store_UpdateLineItem>>();
        //            List<Store_UpdateLineItem> temp_line = new List<Store_UpdateLineItem>();

        //            string sqlcdvt = $"exec haravan_getTonKho_ByKho '{kho}'";
        //            DataSet ds_cdvt = DAL.DAL_SQL.ExecuteGetlistDataset(sqlcdvt);

        //            foreach (DataRow cdvt in ds_cdvt.Tables[0].Rows)
        //            {
        //                string barcode = cdvt["ma_vt"].ToString().Trim();
        //                decimal quantity = (decimal)cdvt["ton13"];
        //                for (int j = 0; j < lst_varian.Count; j++)
        //                {
        //                    if (barcode.Trim() == lst_varian[j].ma_vt.Trim())
        //                    {
        //                        if (temp_line.Count < 200)
        //                            temp_line.Add(new Store_UpdateLineItem(lst_varian[j].product_id, lst_varian[j].product_variant_id, quantity));
        //                        else
        //                        {
        //                            lineItems.Add(temp_line);
        //                            temp_line = new List<Store_UpdateLineItem>();
        //                            temp_line.Add(new Store_UpdateLineItem(lst_varian[j].product_id, lst_varian[j].product_variant_id, quantity));
        //                        }
        //                    }
        //                }
        //            }
        //            lineItems.Add(temp_line);
        //            ResponseApiHaravan ress = new ResponseApiHaravan("ok", "", "");
        //            foreach (List<Store_UpdateLineItem> line in lineItems)
        //            {
        //                url = $"https://apis.haravan.com/com/inventories/adjustorset.json";
        //                if (ds.Tables[0].Rows.Count > 0)
        //                {
        //                    Object body = new
        //                    {
        //                        inventory = new
        //                        {
        //                            location_id = idkho,
        //                            type = "set",
        //                            reason = "shrinkage",
        //                            note = "Update by SSE",
        //                            line_items = line
        //                        }
        //                    };
        //                    JObject temp = JObject.FromObject(body);

        //                    ress = await client.Post_Request_WithBody(url, temp.ToString());
        //                    if (ress.status == "err")
        //                    {
        //                        System.Threading.Thread.Sleep(2000);
        //                        ress = await client.Post_Request_WithBody(url, temp.ToString());
        //                    }
        //                }
        //            }
        //            if (ress.status == "ok") dskho_ok.Add(kho);
        //            else dskho_err.Add(kho);
        //        }
        //        var bodyre = new
        //        {
        //            ds_ok = dskho_ok,
        //            ds_err = dskho_err
        //        };
        //        return StatusCode(200, new ResponseData("ok", "ok", bodyre));
        //    }
        //    catch (Exception e)
        //    {
        //        ApiLog.logval("log_UpdateItemByKho", e.Message);
        //        ILog log = Logger.GetLog(typeof(Customer));

        //        log.Error(Request.Path);
        //        return StatusCode(200, new ResponseData("err", e.Message, ""));
        //    }
        //}

        [HttpPost]
        [Route("UpdateStoreIdHaravan")]
        public async Task<IActionResult> DongBoKhoHaravan()
        {
            bool check = Library.CheckAuthentication(_config, this);
            if (!check) return StatusCode(401);
            try
            {             
                var token = _config.GetValue<string>("config_Haravan:private_token");
                string url = $"https://apis.haravan.com/com/locations.json";
                HttpClientApp client = new HttpClientApp(token);
                ResponseApiHaravan res = await client.Get_Request(url);
                JObject data = JObject.Parse(res.data);
                JArray ListLocations = (JArray)data["locations"];
                List<LocationHaravan> lst_haravan = new List<LocationHaravan>();
                for (int i = 0; i < ListLocations.Count; i++)
                {
                    JObject location = (JObject) ListLocations[i];  
                    LocationHaravan temp = new LocationHaravan();
                    temp.id = (string)location["id"] ?? "";
                    temp.name = (string)location["name"] ?? "";
                    lst_haravan.Add(temp);
                }
                string sqlrun = "";

                string sql = $" SELECT * FROM dmkho WHERE ma_kho LIKE 'TP_%' ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    string makho = r["ma_kho"].ToString();
                    string ma_dvcs = r["ma_dvcs"].ToString();
                    foreach(LocationHaravan l in lst_haravan)
                    {
                        string[] arrstr = l.name.Split("-");
                        if (arrstr.Count() >= 3)
                        {
                            if (arrstr[0].Trim() == ma_dvcs.Trim() && arrstr[arrstr.Count()-1].Trim() == "Kho hàng bán")
                            {
                                sqlrun += Environment.NewLine + $" update dmkho set  s1='{l.id.Trim()}' where ma_kho='{makho.Trim()}'";
                                break;
                            }
                        }
                    }
                }
                DAL.DAL_SQL.ExecuteNonquery(sqlrun);

                return Ok(new ResponseData("ok", "ok", ""));
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));

                log.Error(Request.Path);
                return StatusCode(500, new ResponseData("err", e.Message, ""));
            }
        }

        //[HttpPost]
        //[Route("SetKhoDL")]
        //public async Task<IActionResult> SetKhoDL(ResUpdateDL data)
        //{
        //    bool check = Library.CheckAuthentication(_config, this);
        //    if (!check) return StatusCode(401);
        //    try
        //    {
        //        string sql = "";
        //        var token = _config.GetValue<string>("config_Haravan:private_token");
        //        string url = $"https://apis.haravan.com/com/locations.json";
        //        HttpClientApp client = new HttpClientApp(token);
        //        List<string> lst_kho = data.ma_kho.Split(",").ToList();

        //        foreach (string ma_kho in lst_kho)
        //        {
        //            string sql_kho = $"select * from tbl_temps where loc_id='{ma_kho}' and qty_available <>0";
        //            DataSet ds = DAL.DAL_TEST.ExecuteGetlistDataset(sql_kho);
        //            List<List<Store_UpdateLineItem>> lineItems = new List<List<Store_UpdateLineItem>>();
        //            List<Store_UpdateLineItem> temp_line = new List<Store_UpdateLineItem>();
        //            foreach (DataRow r in ds.Tables[0].Rows)
        //            {
        //                string proc_id = r["pro_id"].ToString();
        //                string varian_id = r["variant_id"].ToString();
        //                if (temp_line.Count < 200)
        //                    temp_line.Add(new Store_UpdateLineItem(proc_id, varian_id, 0));
        //                else
        //                {
        //                    lineItems.Add(temp_line);
        //                    temp_line = new List<Store_UpdateLineItem>();
        //                    temp_line.Add(new Store_UpdateLineItem(proc_id, varian_id, 0));
        //                }
        //            }
        //            lineItems.Add(temp_line);
        //            ResponseApiHaravan ress = new ResponseApiHaravan("ok", "", "");
        //            foreach (List<Store_UpdateLineItem> line in lineItems)
        //            {
        //                url = $"https://apis.haravan.com/com/inventories/adjustorset.json";
        //                if (ds.Tables[0].Rows.Count > 0)
        //                {
        //                    Object body = new
        //                    {
        //                        inventory = new
        //                        {
        //                            location_id = ma_kho,
        //                            type = "set",
        //                            reason = "shrinkage",
        //                            note = "Update by SSE",
        //                            line_items = line
        //                        }
        //                    };
        //                    JObject temp = JObject.FromObject(body);

        //                    ress = await client.Post_Request_WithBody(url, temp.ToString());
        //                    if (ress.status == "err")
        //                    {
        //                        System.Threading.Thread.Sleep(2000);
        //                        ress = await client.Post_Request_WithBody(url, temp.ToString());
        //                    }
        //                }
        //            }

        //        }
        //        int iawds = 0;
        //        return Ok(sql);
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(500, new ResponseData("err", e.Message, ""));
        //    }
        //}

        //[HttpPost]
        //[Route("GetKhoDL")]
        //public async Task<IActionResult> GetKhoDL(ResUpdateDL data)
        //{
        //    bool check = Library.CheckAuthentication(_config, this);
        //    if (!check) return StatusCode(401);
        //    try
        //    {
        //        string sql = "";
        //        var token = _config.GetValue<string>("config_Haravan:private_token");
        //        string url = "";
        //        HttpClientApp client = new HttpClientApp(token);
        //        string sql_vt = "select * from dmvt";
        //        DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql_vt);
        //        int countvt = 1;
        //        string lst_vt = "";
        //        List<ListDL> ds_DL = new List<ListDL>();
        //        foreach (DataRow r in ds.Tables[0].Rows)
        //        {
        //            if (r["s1"] == null || r["s2"] == null) { }
        //            else
        //            {
        //                if (r["s1"].ToString().Trim() == "" || r["s2"].ToString().Trim() == "") { }
        //                else
        //                {
        //                    if (countvt == 49)
        //                    {
        //                        string xtemp = lst_vt;
        //                        lst_vt += "1";
        //                        int page = 0;
        //                        bool checkss = true;
        //                        string since_id = "0";
        //                        while (checkss)
        //                        {
        //                            page += 1;
        //                            url = $"https://apis.haravan.com/com/inventory_locations.json?limit=50&since_id={since_id}&direction=asc&order=id&location_ids=673628&variant_ids={lst_vt}";
        //                            ResponseApiHaravan res_kho;
        //                            ResponseApiHaravan res_kho_te = await client.Get_Request(url);
        //                            if (res_kho_te.status == "ok")
        //                            {
        //                                res_kho = res_kho_te;
        //                            }
        //                            else
        //                            {
        //                                System.Threading.Thread.Sleep(2000);
        //                                res_kho = await client.Get_Request(url);
        //                            }
        //                            JObject root_products = JObject.Parse(res_kho.data);
        //                            JArray ListInven = (JArray)root_products["inventory_locations"];
        //                            for (int i = 0; i < ListInven.Count; i++)
        //                            {
        //                                var product = (JObject)ListInven[i];
        //                                string variant_id = (string)product["variant_id"] ?? "";
        //                                string pro_id = (string)product["product_id"] ?? "";
        //                                string loc_id = (string)product["loc_id"] ?? "";
        //                                decimal qty_onhand = (decimal)product["qty_onhand"];
        //                                decimal qty_commited = (decimal)product["qty_commited"];
        //                                decimal qty_incoming = (decimal)product["qty_incoming"];
        //                                decimal qty_available = (decimal)product["qty_available"];
        //                                if (qty_onhand !=0 || qty_commited !=0 || qty_incoming !=0 || qty_available !=0)
        //                                sql += Environment.NewLine + $"insert into tbl_temps values('{variant_id}','{pro_id}','{loc_id}',{qty_onhand},{qty_commited},{qty_incoming},{qty_available})";
        //                            }
        //                            if (ListInven.Count >= 50)
        //                            {
        //                                checkss = true;
        //                                since_id = ListInven[ListInven.Count - 1]["id"].ToString();
        //                            }
        //                            else checkss = false;
        //                        }
        //                        countvt = 1;
        //                        string ma_vt = r["s1"].ToString();
        //                        lst_vt = ma_vt.Trim() + ",";

        //                    }
        //                    else
        //                    {
        //                        countvt++;
        //                        string ma_vt = r["s1"].ToString();
        //                        lst_vt = lst_vt + ma_vt.Trim() + ",";
        //                    }
        //                }
        //            }
        //        }
        //        int iawds = 0;
        //        return Ok(sql);
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(500, new ResponseData("err", e.Message, ""));
        //    }
        //}

        [HttpPost]
        [Route("UpdateHaravanId")]
        public async Task<IActionResult> UpdateHaravanId()
        {
            bool check = Library.CheckAuthentication(_config, this);
            if (!check) return StatusCode(401);
            try
            {
                string sql = "";
                var token = _config.GetValue<string>("config_Haravan:private_token");
                string url = $"https://apis.haravan.com/com/products.json";
                HttpClientApp client = new HttpClientApp(token);
                int page = 0;
                bool checkss = true;
                while (checkss)
                {
                    page += 1;
                    url = $"https://apis.haravan.com/com/products.json?limit=50&page={page}";

                    ResponseApiHaravan res = await client.Get_Request(url);

                    bool ckeckaction = false;
                    if (res.status == "ok")
                    {
                        ckeckaction = true;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(2000);
                        res = await client.Get_Request(url);
                        if (res.status == "ok") { ckeckaction = true; }
                        else ckeckaction = false;
                    }
                    if (ckeckaction)
                    {
                        JObject root_products = JObject.Parse(res.data);
                        if (Library.IsNullOrEmpty(root_products["products"]))
                        {
                            checkss = false;
                        }
                        else
                        {
                            JArray ListProduct = (JArray)root_products["products"];
                            for (int i = 0; i < ListProduct.Count; i++)
                            {
                                var product = (JObject)ListProduct[i];
                                string id_p = (string)product["id"];
                                if (Library.IsNullOrEmpty(product["variants"])) break;
                                JArray arrVarian = (JArray)product["variants"];
                                for (int j = 0; j < arrVarian.Count; j++)
                                {
                                    var varian = (JObject)arrVarian[j];
                                    if (!Library.IsNullOrEmpty(varian["barcode"]) && !Library.IsNullOrEmpty(varian["id"]))
                                    {
                                        string ma_vt = (string)varian["barcode"] ?? "";
                                        if (ma_vt== "8935292713129")
                                        {
                                            int ssss = 123;
                                        }
                                        string variant_id = (string)varian["id"] ?? "";
                                        sql += Environment.NewLine + $"update dmvt set s1='{variant_id.Trim()}',s2='{id_p.Trim()}' where ma_vt='{ma_vt.Trim()}'";
                                    }
                                }
                            }
                            if (ListProduct.Count >= 50)
                                checkss = true;
                            else checkss = false;
                        }
                    }
                    else
                    {
                        checkss = false;
                    }
                                    
                }

                int tt= 0;
                return Ok(page);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ResponseData("err", e.Message, ""));
            }
        }
    }
}
