using Haravan.FuncLib;
using log4net;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json;
using Haravan.Controllers;
using Haravan.Haravan.Model;
using Haravan.Haravan.Common;

namespace Haravan.Haravan.DataAcess
{
    public class Orders3
    {
        public ResponseData orders_create(Object data)
        {
            try
            {
                string temp = data.ToString();
                JObject obj = JObject.Parse(temp);
                UpdateOrder(obj);

                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public ResponseData orders_updated(Object data)
        {
            try
            {
                string temp = data.ToString();
                JObject obj = JObject.Parse(temp);
                UpdateOrder(obj);
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public ResponseData orders_paid(Object data)
        {
            try
            {
                string temp = data.ToString();
                JObject obj = JObject.Parse(temp);
                UpdateOrder(obj);
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public ResponseData orders_cancelled(Object data)
        {
            try
            {
                string temp = data.ToString();
                JObject obj = JObject.Parse(temp);
                CancelOrder(obj);
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public ResponseData orders_fulfilled(Object data)
        {
            try
            {
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public ResponseData orders_delete(Object data)
        {
            try
            {
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }

        public async Task<ResponseData> UpdateNote(string id, string note)
        {
            try
            {
                var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");
                string url = $"https://apis.haravan.com/com/orders/{id}.json";
                Object body = new
                {
                    order = new
                    {
                        note = note,
                    }
                };
                JObject temp = JObject.FromObject(body);
                HttpClientApp client = new HttpClientApp(token);
                ResponseApiHaravan res = await client.Put_Request_WithBody(url, temp.ToString());
                if (res.status == "ok") return new ResponseData("ok", "", "");
                else
                {
                    ILog log = Logger.GetLog(typeof(Orders));
                    log.Error(res.message);
                    return new ResponseData("err", res.message, "");
                }
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public async Task<ResponseData> UpdateAllOrder(DateTime fromDate, DateTime toDate, string so_ct)
        {
            try
            {
                int countok = 0;
                int counterr = 0;
                List<string> lst_ok = new List<string>();
                List<string> lst_err = new List<string>();
                string fDate = fromDate.ToString("yyyy/MM/dd");
                string tDate = toDate.ToString("yyyy/MM/dd");
                var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");
                bool check = true;
                int page = 0;
                while (check)
                {
                    page += 1;
                    string url = $"https://apis.haravan.com/com/orders.json?created_at_min={fDate}&created_at_max={tDate}&page={page}&limit=50";
                    if (so_ct == null || so_ct.Trim() == "") { }
                    else
                    {
                        url += $"&query=filter=(ordernumber:order=%23{so_ct})";
                    }
                    HttpClientApp client = new HttpClientApp(token);
                    ResponseApiHaravan res = await client.Get_Request(url);
                    JObject data = JObject.Parse(res.data);
                    JArray arrOrders = (JArray)data["orders"];
                    for (int i = 0; i < arrOrders.Count; i++)
                    {
                        JObject obj = (JObject)arrOrders[i];
                        string number = (string)obj["order_number"] ?? "";
                        int up = await UpdateOrder(obj);
                        if (up == 0)
                        {
                            lst_ok.Add(number);
                            countok += 1;
                        }
                        else
                        {
                            lst_err.Add(number);
                            counterr += 1;
                        }
                    }
                    if (arrOrders.Count >= 50)
                        check = true;
                    else check = false;
                }
                Object datare = new
                {
                    count_ok = countok,
                    count_err = counterr,
                    ds_order_ok = lst_ok,
                    ds_order_err = lst_err

                };
                return new ResponseData("ok", "", datare);
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }

        }

        public async Task<ResponseData> Gethistory(DateTime fromDate, DateTime toDate)
        {
            try
            {
                int countok = 0;
                int counterr = 0;
                string fDate = fromDate.ToString("yyyy/MM/dd");
                string tDate = toDate.ToString("yyyy/MM/dd");
                var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");
                bool check = true;
                int page = 0;
                string sql = "delete from FailedBillHistory";
                string lst_ct_post = "";
                List<OrderHarDetail> ds_har = new List<OrderHarDetail>();
                while (check)
                {
                    page += 1;
                    string url = $"https://apis.haravan.com/com/orders.json?created_at_min={fDate}&created_at_max={tDate}&page={page}&limit=50";
                    HttpClientApp client = new HttpClientApp(token);
                    ResponseApiHaravan res = await client.Get_Request(url);
                    JObject data = JObject.Parse(res.data);
                    JArray arrOrders = (JArray)data["orders"];


                    for (int i = 0; i < arrOrders.Count; i++)
                    {
                        JObject obj = (JObject)arrOrders[i];
                        bool checkorder = DTOOrder.checkOrder(obj);

                        if (checkorder)
                        {
                            string id = (string)obj["id"] ?? "";
                            lst_ct_post += $"'{id.Trim()}',";
                            string pos = (string)obj["source_name"] ?? "";
                            string number = (string)obj["order_number"] ?? "";
                            string ma_khs = "";
                            if (!Library.IsNullOrEmpty(obj["customer"]["phone"]))
                                ma_khs = ((string)obj["customer"]["phone"] ?? "");
                            OrderHarDetail ttt = new OrderHarDetail();
                            ttt.id = id;
                            ttt.number = number;
                            ttt.job = pos;
                            ttt.ma_kh = ma_khs;
                            ds_har.Add(ttt);
                        }
                    }
                    if (arrOrders.Count >= 50)
                        check = true;
                    else check = false;
                }
                lst_ct_post += "'_'";
                DateTime myDate = Convert.ToDateTime(fDate);
                //myDate = myDate.AddHours(7);
                //var easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                //myDate = TimeZoneInfo.ConvertTime(myDate, easternZone);
                string table_sufix = myDate.ToString("yyyyMM", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                string checkexit = $"select * from m81${table_sufix} where LTRIM(RTRIM(so_ct_post)) in({lst_ct_post} ) ";

                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(checkexit);
                foreach (OrderHarDetail har in ds_har)
                {
                    bool c = false;
                    foreach (DataRow r in ds.Tables[0].Rows)
                    {
                        string so_ct_post = r["so_ct_post"].ToString();
                        if (so_ct_post.Trim() == har.id)
                        {
                            c = true;
                            break;
                        }
                    }
                    if (c) { countok += 1; }
                    else
                    {
                        counterr += 1;
                        sql += Environment.NewLine + $"insert into FailedBillHistory values('{har.id}','{har.number}','{fDate}','{har.job}','','{har.ma_kh}')";
                    }
                }
                DAL.DAL_SQL.ExecuteNonquery(sql);
                Object datare = new
                {
                    fromdate = fDate,
                    toDate = tDate,
                    count_ok = countok,
                    count_err = counterr,
                };
                return new ResponseData("ok", "", datare);
            }
            catch (Exception e)
            {
                ApiLog.logval("log_Gethistory", e.Message);
                return new ResponseData("err", e.Message, "");
            }

        }

        public async Task<ResponseData> UpdateOrderMissing(API_Data_Order_UpdateOrder data_api)
        {
            try
            {
                string sql = "";
                var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");
                bool check = true;
                int page = 0;
                List<HisotryOrder> lst_history = new List<HisotryOrder>();
                int t = 0;
                List<string> strinqueryorder = new List<string>();
                string strorder = $"&query=filter=(ordernumber:order in ";
                for (int k = 0; k < data_api.so_ct.Count - 1; k++)
                {
                    t++;
                    if (t < 49)
                    {
                        string temp = data_api.so_ct[k].Trim().Replace("#", "%23");
                        strorder += $"{temp},";
                    }
                    else
                    {
                        t = 0;
                        string temp = data_api.so_ct[k].Trim().Replace("#", "%23");
                        strorder += $"{temp} )";
                        strinqueryorder.Add(strorder);
                        strorder = $"&query=filter=(ordernumber:order in ";
                    }
                }
                string temp2 = data_api.so_ct[data_api.so_ct.Count - 1].Trim().Replace("#", "%23");
                strorder += $"{temp2} )";
                strinqueryorder.Add(strorder);

                HttpClientApp client = new HttpClientApp(token);
                foreach (string lst_order in strinqueryorder)
                {
                    string url = $"https://apis.haravan.com/com/orders.json?created_at_min={data_api.fromDate}&created_at_max={data_api.toDate}&page=1&limit=50{lst_order}";
                    ResponseApiHaravan res = await client.Get_Request(url);
                    JObject data = JObject.Parse(res.data);
                    JArray arrOrders = (JArray)data["orders"];
                    for (int i = 0; i < arrOrders.Count; i++)
                    {
                        JObject obj = (JObject)arrOrders[i];
                        string number = (string)obj["order_number"] ?? "";
                        int up = await UpdateOrder(obj);
                        if (up == 0)
                        {
                            sql += Environment.NewLine + $"delete from FailedBillHistory where rtrim(ltrim(so_ct))='{number}'";
                        }
                    }
                }
                DAL.DAL_SQL.ExecuteNonquery(sql);

                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                ApiLog.log("err-UpdateOrderMissing", "", e.Message, "", "");
                return new ResponseData("err", e.Message, "");
            }

        }


        public async Task<ResponseData> UpdateDiaChi(API_Data_Order_UpdateOrder data_api)
        {
            try
            {
                string sql = "";
                var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");

                bool check = true;
                int page = 0;
                HttpClientApp client = new HttpClientApp(token);
                while (check)
                {
                    page += 1;
                    string url = $"https://apis.haravan.com/com/orders.json?created_at_min={data_api.fromDate}&created_at_max={data_api.toDate}&page={page}&limit=50";
                    ResponseApiHaravan res = await client.Get_Request(url);
                    JObject data = JObject.Parse(res.data);
                    JArray arrOrders = (JArray)data["orders"];
                    for (int i = 0; i < arrOrders.Count; i++)
                    {
                        JObject obj = (JObject)arrOrders[i];
                        string id = (string)obj["id"] ?? "";
                        JObject dcgh1 = (JObject)obj["shipping_address"];
                        if (dcgh1 != null)
                        {
                            string to_thanh_pho = "";
                            string to_quan_huyen = "";
                            string to_phuong_xa = "";
                            if (dcgh1["province_code"] != null) to_thanh_pho = DiaChi.getmatinhbyharavan((string)dcgh1["province_code"] ?? "");
                            if (dcgh1["district_code"] != null) to_quan_huyen = DiaChi.getmahuyenbyharavan((string)dcgh1["district_code"] ?? "");
                            if (dcgh1["ward_code"] != null) to_phuong_xa = DiaChi.getmaphuongbyharavan((string)dcgh1["ward_code"] ?? "", (string)dcgh1["ward"] ?? "");
                            sql += Environment.NewLine + $" update m81$202107 set ma_tinh='{to_thanh_pho}',ma_quan='{to_quan_huyen}',ma_phuong=N'{to_phuong_xa}' where rtrim(ltrim(so_ct_post))='{id.Trim()}'";
                        }
                    }
                    if (arrOrders.Count >= 50)
                        check = true;
                    else check = false;
                }
                DAL.DAL_SQL.ExecuteNonquery(sql);

                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                ApiLog.log("err-UpdateDiaChi", "", e.Message, "", "");
                return new ResponseData("err", e.Message, "");
            }

        }

        public async void AddOrder(JObject obj)
        {
            string token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");
            try
            {
                string sql = await DTOOrder.CreatSqlOrder(obj, token,"3");
                DAL.DAL_SQL.ExecuteNonquery(sql);
            }
            catch (Exception e)
            {
                ApiLog.log("err-ModelApp-AddOrder", "", e.Message, "", "");
            }
        }
        public async void confrimHD(string id)
        {
            try
            {
                var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");
                string url = $"https://apis.haravan.com/com/orders/{id}/confirm.json";
                HttpClientApp client = new HttpClientApp(token);
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, "");

                ILog log = Logger.GetLog(typeof(Orders));
                log.Error(id);
                log.Info(res);
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Orders));
                log.Error(e.Message);
            }
        }

        public async void cancelHD(string id)
        {
            try
            {
                var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");
                string url = $"https://apis.haravan.com/com/orders/{id}/cancel.json";
                HttpClientApp client = new HttpClientApp(token);
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, "");

                await UpdateNote(id, "Đã hết hàng");

                ILog log = Logger.GetLog(typeof(Orders));
                log.Error(id);
                log.Info(res);
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Orders));
                log.Error(e.Message);
            }
        }
        public async void UpdateTonKho_WP(JObject obj)
        {
            try
            {
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                client.SetAuthorizationBasic(token);
                string url = "";
                string host = MyAppData.config.GetValue<string>("WP:url");


                JArray listitem = (JArray)obj["line_items"];
                string lst_sp = "";
                for (int i = 0; i < listitem.Count; i++)
                {
                    var item = (JObject)listitem[i];
                    string ma_vtz = ((string)item["barcode"] ?? "");
                    lst_sp += $"{ma_vtz},";
                }
                if (lst_sp != "") lst_sp = lst_sp.Remove(lst_sp.Length - 1, 1);

                string sql = $"exec haravan_getTonKho_UpdateWP '{lst_sp}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    url = $"{host}/wp-json/wc/v3/products/{r["wp_product"].ToString().Trim()}/variations/{r["wp_variant"].ToString().Trim()}";
                    Object body_var = new
                    {
                        stock_quantity = Library.ConvertToDecimal(r["ton13"].ToString()),
                    };
                    JObject body_temp2 = JObject.FromObject(body_var);

                    ResponseApiHaravan res2 = await client.Put_Request_WithBody(url, body_temp2.ToString());

                }
            }
            catch (Exception e)
            {
                ApiLog.logval("error_updatetonkhohar_wp", e.Message);

            }
        }
        public async void UpdateTonKho(JObject obj)
        {
            var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");
            HttpClientApp client = new HttpClientApp(token);
            string url = "";
            try
            {
                if (Library.IsNullOrEmpty(obj["line_items"])) return;
                JArray listitem = (JArray)obj["line_items"];
                string lst_sp = "";
                List<HaravanItem> lst_item = new List<HaravanItem>();

                List<string> lst_barcodeitems = new List<string>();
                for (int i = 0; i < listitem.Count; i++)
                {

                    var item = (JObject)listitem[i];

                    string ma_vtz = ((string)item["barcode"] ?? "");
                    string product_id = (string)item["product_id"];
                    string variant_id = (string)item["variant_id"];

                    HaravanItem har_items = new HaravanItem();
                    har_items.ma_vt = ma_vtz;
                    har_items.product_id = product_id;
                    har_items.variant_id = variant_id;
                    lst_item.Add(har_items);
                    lst_sp += $"{ma_vtz},";

                }
                lst_sp = lst_sp.Remove(lst_sp.Length - 1, 1);

                string sql = $"exec haravan_getTonKho_UpdateHaravan '{lst_sp}','3'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);

                string ma_kho = "";
                string locationId = "";
                string ma_vt = "";
                List<Store_UpdateLineItem> lineItem = new List<Store_UpdateLineItem>();

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow r = ds.Tables[0].Rows[i];
                    string har_product = ds.Tables[0].Rows[0]["har_product"].ToString();
                    string har_variant = ds.Tables[0].Rows[0]["har_variant"].ToString();
                    decimal so_luong = (decimal)r["ton13"];

                    url = $"https://apis.haravan.com/com/inventory_locations.json?location_ids={MyAppData.ma_kho_Haravan3}&variant_ids={har_variant}";
                    decimal quantity = 0;
                    ResponseApiHaravan res_inven = await client.Get_Request(url);
                    if (res_inven.status == "ok")
                    {
                        try
                        {
                            decimal before_quantity = 0;
                            JObject root_total_inven = JObject.Parse(res_inven.data);
                            JArray arr = (JArray)root_total_inven["inventory_locations"];
                            before_quantity = Library.ConvertToDecimal(arr[0]["qty_available"].ToString());
                            if (ds.Tables[0].Rows.Count > 0) quantity = so_luong - before_quantity;
                        }
                        catch (Exception e) { }


                    }

                    lineItem.Add(new Store_UpdateLineItem(har_product, har_variant, quantity));
                }
                if (ds.Tables[0].Rows.Count > 0)
                {
                    Object body = new
                    {
                        inventory = new
                        {
                            location_id = MyAppData.ma_kho_Haravan3,
                            type = "adjust",
                            reason = "",
                            note = "",
                            line_items = lineItem
                        }
                    };
                    JObject temp = JObject.FromObject(body);
                    url = $"https://apis.haravan.com/com/inventories/adjustorset.json";
                    ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
                }
                ApiLog.logval("log_updatetonkho", "");
            }
            catch (Exception e)
            {
                ApiLog.logval("error_updatetonkho", e.Message);

            }
        }
        public void CancelOrder(JObject obj)
        {
            string ordernumber = obj["order_number"].ToString();
            string sql = $"exec haravan_cancel_order '{ordernumber}'";
            DAL.DAL_SQL.ExecuteNonquery(sql);
        }

        public async Task<int> UpdateOrder(JObject obj)
        {
            var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");
            try
            {
                DateTime myDate = Convert.ToDateTime((string)obj["created_at"] ?? "");
                myDate = myDate.AddHours(7);
                //var easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                //myDate = TimeZoneInfo.ConvertTime(myDate, easternZone);
                string table_sufix = myDate.ToString("yyyyMM", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                string so_ct = ((string)obj["order_number"] ?? "");

                string pos = "";
                bool checkorder = DTOOrder.checkOrder(obj);

                if (!checkorder) return 0;

                string number_haravan = ((string)obj["id"] ?? "");

                CheckOrderExit checksse = CheckOrder(so_ct);
                if (checksse.status == "ok")
                {
                    if (!checksse.check)
                    {
                        string sql = await DTOOrder.CreatSqlOrder(obj, token,"3");
                        DAL.DAL_SQL.ExecuteNonquery(sql);

                        JObject customer = (JObject)obj["customer"];
                        Customers cus = new Customers();
                        cus.CreatNewKH(customer);
                        UpdateTonKho(obj);

                        //Orders2 or2 = new Orders2();
                        //or2.UpdateTonKho(obj);
                        //Orders or = new Orders();
                        //or.UpdateTonKho(obj);

                        UpdateTonKho_WP(obj);
                        confrimHD(number_haravan);
                        return 0;
                    }
                    else
                    {
                        string sql = await DTOOrder.UpdateSqlOrder(obj, token, checksse.tbl_sufix);
                        DAL.DAL_SQL.ExecuteNonquery(sql);
                        UpdateTonKho(obj);

                        //Orders2 or2 = new Orders2();
                        //or2.UpdateTonKho(obj);
                        //Orders or = new Orders();
                        //or.UpdateTonKho(obj);

                        UpdateTonKho_WP(obj);
                        return 0;
                    }
                }
                else return 1;

            }
            catch (Exception e)
            {
                string id = ((string)obj["id"] ?? "");
                ApiLog.log("error/Update Order", "", e.Message, e.ToString(), id);
                return 1;
            }
        }

        public async Task<ResponseData> CheckDisCount(string ma_km, string stt_rec, DateTime ngay_lct)
        {
            if (ma_km == null || ma_km.Trim() == "")
                return new ResponseData("err", "Không tồn tại mã giảm giá", "");
            if (stt_rec == null || stt_rec.Trim() == "")
                return new ResponseData("err", "Không tồn tại hóa đơn " + stt_rec, "");
            var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");
            try
            {
                string table_sufix = ngay_lct.ToString("yyyyMM", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                string checkexit = $"exec haravan_getdetailorder '{table_sufix}','{stt_rec}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(checkexit);
                if (ds.Tables[0].Rows.Count > 0)
                {

                    DataTable myTable = ds.Tables[0];
                    List<LineItem> line_items = new List<LineItem>();
                    foreach (DataRow row in myTable.Rows)
                    {
                        string barcode = (string)row["barcode"];
                        double so_luong = (double)row["so_luong"];
                        Products p = new Products();
                        ResponseData res_product = await p.GETVarianFromBarCode(barcode);
                        string product_id = res_product.data.product_id;
                        string product_variant_id = res_product.data.product_variant_id;
                        double price = res_product.data.price;
                        double price_after = (double)row["gia_haravan"];
                        line_items.Add(new LineItem(product_variant_id, product_id, price_after, so_luong, price));
                    }
                    List<object> list_discount = new List<object>();
                    list_discount.Add(new
                    {
                        code = ma_km,
                        amount = 1,
                        is_coupon_code = true
                    });
                    Object body = new
                    {
                        order = new
                        {
                            email = "sse.net.vn@example.com",
                            discount_codes = list_discount,
                            note = "Thêm bằng SSE",
                            line_items = line_items
                        }
                    };
                    JObject temp = JObject.FromObject(body);
                    string url = $"https://apis.haravan.com/com/orders.json";
                    HttpClientApp client = new HttpClientApp(token);
                    ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
                    if (res.status == "ok")
                    {
                        return new ResponseData("ok", "Mã khuyến mãi hợp lệ", "");
                    }
                    else
                    {
                        return new ResponseData("err", "Mã khuyến mãi không hợp lệ", "");
                    }
                }
                else
                {
                    return new ResponseData("err", "Không tồn tại hóa đơn " + stt_rec, "");
                }
                return new ResponseData("err", "", "");


            }
            catch (Exception e)
            {
                ApiLog.log("err-CheckDisCount", "", e.Message, "", "");
                return new ResponseData("err", e.Message, "");
            }
        }
        public CheckOrderExit CheckOrder(string so_ct)
        {
            try
            {
                string sql = $"select  * from c81$000000 where rtrim(ltrim(so_ct))='{so_ct.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    return new CheckOrderExit("ok", false, "");
                }
                else
                {
                    DataRow r = ds.Tables[0].Rows[0];
                    DateTime date = (DateTime)r["ngay_ct"];
                    string tbl_sufix = date.ToString("yyyyMM", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                    return new CheckOrderExit("ok", true, tbl_sufix.Trim());
                }
            }
            catch (Exception e)
            {
                ApiLog.logval("checkorder/error", e.Message);
                return new CheckOrderExit("err", false, "");
            }
        }

        public async void GetPromotionforOrder(DataRow row)
        {
            var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");
            string url = $"https://apis.haravan.com/com/promotions.json";
            HttpClientApp client = new HttpClientApp(token);
            ResponseApiHaravan res = await client.Get_Request(url);
            JObject data = JObject.Parse(res.data);
            JArray ListPromotion = (JArray)data["promotions"];


            string barcode = (string)row["barcode"];
            double so_luong = (double)row["so_luong"];
            Products p = new Products();
            ResponseData res_product = await p.GETVarianFromBarCode(barcode);
            if (res_product.status == "ok")
            {
                if (res_product.data.not_allow_promotion) return;
                string product_id = res_product.data.product_id;
                string product_variant_id = res_product.data.product_variant_id;
                double price = res_product.data.price;
                double max_price = 0;
                string magg = "";
                string magg_name = "";
                for (int i = 0; i < ListPromotion.Count; i++)
                {
                    var promotion = ListPromotion[i];
                    var ends_at = promotion["ends_at"];
                    if (ends_at == null || ends_at.Type == JTokenType.Null) { }
                    else
                    {
                        if (Library.ConvertDatetime((string)ends_at ?? "") <= DateTime.Now) return;
                    }
                    if (((string)promotion["status"] ?? "") != "enabled") return;
                    string applies_to_resource = (string)promotion["applies_to_resource"] ?? "";
                    string applies_to_id = (string)promotion["applies_to_id"] ?? "";
                    if (applies_to_resource == "product" && applies_to_id != product_id) return;
                    if (applies_to_resource == "collection")
                    {
                        string urlcolection = $"https://apis.haravan.com/com/collects.json?collection_id={applies_to_id}";
                        HttpClientApp client2 = new HttpClientApp(token);
                        ResponseApiHaravan res2 = await client2.Get_Request(urlcolection);
                        JObject data2 = JObject.Parse(res2.data);
                        JArray ListCollect = (JArray)data2["collects"];
                        var colect = ListCollect.FirstOrDefault(c => (string)c["product_id"] == product_id);
                        if (colect == null) return;
                    }
                    if (applies_to_resource == "product_variant")
                    {
                        JArray listVaran = (JArray)promotion["variants"];
                        var varan = listVaran.FirstOrDefault(c => ((string)c["variant_id"] == product_variant_id));
                        if (varan == null) return;
                    }
                    string promotion_apply_type = (string)promotion["promotion_apply_type"];
                    if (promotion_apply_type == "1")
                    {
                        double total_discount = 0;
                        double applies_to_quantity = (double)promotion["applies_to_quantity: "];
                        double value_discount = (double)promotion["value"];
                        string discount_type = (string)promotion["discount_type"];
                        if (so_luong < applies_to_quantity) return;
                        if (discount_type == "fixed_amount") total_discount = value_discount;
                        if (discount_type == "percentage") total_discount = price * value_discount / 100;
                        if (discount_type == "same_price") total_discount = price - value_discount;
                        if (total_discount >= max_price)
                        {
                            max_price = total_discount;
                            magg = (string)promotion["code"];
                            magg_name = (string)promotion["name"];
                        }
                    }
                }
            }
        }

        public async Task<ResponseData> CreatFulliment(string stt_rec, string tbl_sufix)
        {
            string sql = $"select * from m81${tbl_sufix} where stt_rec='{stt_rec}'";
            sql += Environment.NewLine + $"select a.*,b.s1 AS haravan_location from d81${tbl_sufix} a LEFT JOIN dmkho b ON a.ma_kho =b.ma_kho where stt_rec='{stt_rec}'";

            var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");
            HttpClientApp client = new HttpClientApp(token);
            DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow master = ds.Tables[0].Rows[0];
                    string id = master["so_ct_post"].ToString().Trim();


                    List<LineOrderDetail> lst_line = new List<LineOrderDetail>();
                    ResponseData res_order = await GetLineOrder(id);
                    if (res_order.status == "err") return res_order;
                    else
                    {
                        JObject root_products = JObject.Parse(res_order.data);
                        JObject JOrder = (JObject)root_products["order"];
                        JArray JLine = (JArray)JOrder["line_items"];
                        foreach (JObject l in JLine)
                        {
                            LineOrderDetail tempLine = new LineOrderDetail();
                            tempLine.id = l["id"].ToString();
                            tempLine.product_id = l["product_id"].ToString();
                            tempLine.barcode = l["barcode"].ToString();
                            tempLine.variant_id = l["variant_id"].ToString();
                            lst_line.Add(tempLine);
                        }
                    }

                    string url = $"https://apis.haravan.com/com/orders/{id}/fulfillments.json";
                    List<ProductItemFulliment> lst_item = new List<ProductItemFulliment>();
                    string locaton = "";
                    foreach (DataRow r in ds.Tables[1].Rows)
                    {
                        string ma_vt = r["ma_vt"].ToString().Trim();
                        decimal sl = (decimal)r["so_luong"];
                        locaton = r["haravan_location"].ToString();

                        foreach (LineOrderDetail line in lst_line)
                        {
                            if (ma_vt == line.barcode.Trim())
                            {
                                ProductItemFulliment p = new ProductItemFulliment();
                                p.id = line.id;
                                p.quantity = Decimal.ToInt32(sl);
                                lst_item.Add(p);
                                break;
                            }
                        }
                    }
                    Object body = new
                    {
                        fulfillment = new
                        {
                            tracking_number = "",
                            location_id = locaton,
                            line_items = lst_item
                        }
                    };
                    JObject temp = JObject.FromObject(body);
                    ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
                    if (res.status == "ok")
                    {
                        return new ResponseData("ok", "", "");
                    }
                    else
                    {
                        return new ResponseData("err", res.data, "");
                    }

                }
                else return new ResponseData("err", "Không tồn tại hóa đơn", "");
            }
            else
            {
                return new ResponseData("err", "Không tồn tại hóa đơn", "");
            }
        }
        public async Task<ResponseData> GetLineOrder(string id)
        {
            try
            {
                if (id == null || id == "")
                    return new ResponseData("err", "Không có hóa đơn này trên haravan 1", "");
                var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");

                string url = $"https://apis.haravan.com/com/orders/{id}.json";
                HttpClientApp client = new HttpClientApp(token);
                ResponseApiHaravan res = await client.Get_Request(url);
                if (res.status == "ok") return new ResponseData("ok", "", res.data);
                else return new ResponseData("err", res.data, "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }

        public async Task<ResponseData> CancelOrderHaravan(string stt_rec, string tbl)
        {
            try
            {
                string sql = $"select stt_rec,isnull(so_ct_post,'') as so_ct_post from m81${tbl} where stt_rec='{stt_rec}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0) return new ResponseData("err", "Không tồn tại hóa đơn", "");
                string order_id = ds.Tables[0].Rows[0]["so_ct_post"].ToString().Trim();
                if (order_id == "") return new ResponseData("err", "Không tồn tại hóa đơn trên haravan", "");

                var token = MyAppData.config.GetValue<string>("config_Haravan3:private_token");

                string url = $"https://apis.haravan.com/com/orders/{order_id}/canel.json ";
                HttpClientApp client = new HttpClientApp(token);

                ResponseApiHaravan res = await client.Post_Request_WithBody(url, "");
                if (res.status == "ok") return new ResponseData("ok", "", res.data);
                else return new ResponseData("err", res.data, "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }

    }

}
