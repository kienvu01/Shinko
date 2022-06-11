using Haravan.FuncLib;
using log4net;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Haravan.Model;
using System.Data;
using Newtonsoft.Json;
using Haravan.Controllers;

namespace Haravan.ModelsApp
{
    public class WP_ObjCustomer
    {
        public string phone { get; set; }
        public string name { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string email { get; set; }
    }
    public class WP_Order
    {
        public static void UpdateOrder(JObject obj)
        {
            string so_ct = "DL"+((string)obj["number"] ?? "");
            CheckOrderExit checksse = CheckOrder(so_ct);
            if (checksse.status == "ok")
            {
                if (!checksse.check)
                {
                    string sql = CreatSqlOrder(obj);
                    ApiLog.logval("test-val-creat", sql);
                    DAL.DAL_SQL.ExecuteNonquery(sql);
                    UpdateCustomer(obj);
                }
                else
                {
                    string sql = UpdateSqlOrder(obj, checksse.tbl_sufix);
                    DAL.DAL_SQL.ExecuteNonquery(sql);
                    ApiLog.logval("test-val-update", sql);
                    UpdateCustomer(obj);
                }
                Updatetonkho(obj);
                //UpdatetonkhoHaravan(obj);
            }
        }
        public async static void Updatetonkho(JObject obj)
        {
            JArray listitem = (JArray)obj["line_items"];
            for (int i = 0; i < listitem.Count; i++)
            {
                var item = (JObject)listitem[i];
                string ma_vt = ((string)item["sku"] ?? "");
                string product_id = item["product_id"].ToString();
                string variation_id = item["variation_id"].ToString();
                string sql = $"SELECT SUM(isnull(ton13,0)) AS ton FROM cdvt213 WHERE ma_vt ='{ma_vt}' "; 
                decimal sl = 0;
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    sl = Convert.ToDecimal(ds.Tables[0].Rows[0]["ton"].ToString());
                }
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                client.SetAuthorizationBasic(token);
                string host = MyAppData.config.GetValue<string>("WP:url");
                string url = $"{host}/wp-json/wc/v3/products/{product_id}/variations/{variation_id}";
                Object body = new
                {
                    stock_quantity = sl,
                };
                JObject body_temp = JObject.FromObject(body);

                ResponseApiHaravan res = await client.Post_Request_WithBody(url, body_temp.ToString());

            }
        }
        public async static void UpdatetonkhoHaravan(JObject obj)
        {
            var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");
            HttpClientApp client = new HttpClientApp(token);
            string url = "";
            try
            {
                if (Library.IsNullOrEmpty(obj["line_items"])) return;
                JArray listitem = (JArray)obj["line_items"];
                string lst_sp = "";
                for (int i = 0; i < listitem.Count; i++)
                {
                    var item = (JObject)listitem[i];
                    string ma_vtz = ((string)item["sku"] ?? "");
                    lst_sp += $"{ma_vtz},";
                }
                if(lst_sp != "") lst_sp = lst_sp.Remove(lst_sp.Length - 1, 1);

                string sql = $"exec haravan_getTonKho_UpdateHaravan '{lst_sp}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);

                List<Store_UpdateLineItem> lineItem = new List<Store_UpdateLineItem>();

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow r = ds.Tables[0].Rows[i];
                    url = $"https://apis.haravan.com/com/inventory_locations.json?location_ids={MyAppData.ma_kho_Haravan}&variant_ids={r["har_variant"].ToString()}";
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
                            if (ds.Tables[0].Rows.Count > 0) quantity = FuncLib.Library.ConvertToDecimal(ds.Tables[0].Rows[0]["ton13"].ToString()) - before_quantity;
                        }
                        catch (Exception e) { }


                    }

                    lineItem.Add(new Store_UpdateLineItem(r["har_product"].ToString(), r["har_variant"].ToString(), quantity));
                }
                Object body = new
                {
                    inventory = new
                    {
                        location_id = MyAppData.ma_kho_Haravan,
                        type = "adjust",
                        reason = "",
                        note = "",
                        line_items = lineItem
                    }
                };
                JObject temp = JObject.FromObject(body);
                url = $"https://apis.haravan.com/com/inventories/adjustorset.json";
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
                
                ApiLog.logval("log_updatetonkho", res.message);
            }
            catch (Exception e)
            {
                ApiLog.logval("error_updatetonkho", e.Message);

            }
        }

        public static void UpdateCustomer(JObject obj)
        {
            try
            {
                WP_ObjCustomer cus = GetCustomer(obj);
                if(cus.phone != "")
                {
                    string sql = "";
                    sql += Environment.NewLine + $" IF NOT EXISTS(SELECT * FROM dmkh WHERE ma_kh = '{cus.phone}')";
                    sql += Environment.NewLine + "BEGIN";
                    sql += Environment.NewLine + $"insert into dmkh(ma_kh,ten_kh,dien_thoai,dia_chi,ma_tinh,ma_quan,ma_phuong,e_mail) ";
                    sql += Environment.NewLine + $"values('{cus.phone}',N'{cus.name}','{cus.phone}',N'{cus.address_1}','{cus.city}','','',N'{cus.email}')";
                    sql += Environment.NewLine + "END";
                    DAL.DAL_SQL.ExecuteNonquery(sql);

                }
                
            }
            catch(Exception e)
            {

            }
            
        }
        public static CheckOrderExit CheckOrder(string so_ct)
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

        public static CheckOrderExit CheckOrderShinko(string so_ct)
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

        public static WP_ObjCustomer GetCustomer(JObject obj)
        {
            WP_ObjCustomer re = new WP_ObjCustomer();
            re.phone = "";
            try
            {
                if (!Library.IsNullOrEmpty(obj["billing"]["phone"]))
                {
                    JObject billing = (JObject)obj["billing"];
                    re.phone = billing["phone"].ToString();
                    re.name = billing["first_name"].ToString() + billing["last_name"].ToString();
                    re.address_1 = billing["address_1"].ToString();
                    re.address_2 = billing["address_2"].ToString();
                    re.city = billing["city"].ToString();
                    re.country = billing["country"].ToString();
                }
                else
                {
                    if (!Library.IsNullOrEmpty(obj["shipping"]["phone"]))
                    {
                        JObject billing = (JObject)obj["shipping"];
                        re.phone = billing["phone"].ToString();
                        re.name = billing["first_name"].ToString() + billing["last_name"].ToString();
                        re.address_1 = billing["address_1"].ToString();
                        re.address_2 = billing["address_2"].ToString();
                        re.city = billing["city"].ToString();
                        re.country = billing["country"].ToString();
                    }
                }
                return re;
            }
            catch(Exception e)
            {
                return re;
            }
        }
        public static string GetVoucher(JObject obj)
        {
            string re_voucher = "";
            try
            {
                JArray lst_voucher = (JArray)obj["coupon_lines"];
                for(int i = 0; i < lst_voucher.Count; i++)
                {
                    JObject voucher = (JObject)lst_voucher[i];
                    re_voucher += voucher["code"].ToString();
                }
                return re_voucher;
            }
            catch (Exception e)
            {
                return re_voucher;
            }
        }
        public static WP_ObjCustomer GetShipping(JObject obj)
        {
            WP_ObjCustomer re = new WP_ObjCustomer();
            try
            {
                if (!Library.IsNullOrEmpty(obj["shipping"]["phone"]))
                {
                    JObject billing = (JObject)obj["shipping"];
                    re.phone = billing["phone"].ToString();
                    re.name = billing["first_name"].ToString() + billing["last_name"].ToString();
                    re.address_1 = billing["address_1"].ToString();
                    re.address_2 = billing["address_2"].ToString();
                    re.city = billing["city"].ToString();
                    re.country = billing["country"].ToString();
                }

                return re;
            }
            catch (Exception e)
            {
                return re;
            }
        }

        public static string ShinkoCreatSqlOrder(JObject obj)
        {
            try
            {
                string str = String.Empty;

                DateTime myDate = Convert.ToDateTime((string)obj["date_created_gmt"] ?? "");
                myDate = myDate.AddHours(7);
                //var easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                //myDate = TimeZoneInfo.ConvertTime(myDate, easternZone);

                string number_haravan = ((string)obj["id"] ?? "");
                string table_sufix = myDate.ToString("yyyyMM", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                int i = 0;
                string ma_dvcs = "ON01";
                string so_ct = "DL" + ((string)obj["number"] ?? "");
                string number_wp = ((string)obj["id"] ?? "");
                string loai_ct = "CT";
                string ma_gd = "GD";
                string ma_nt = (string)(obj["currency"] ?? "");
                string ngay_ct = myDate.ToString("yyyy/MM/dd", System.Globalization.CultureInfo.GetCultureInfo("en-US"));


                double t_tien2 = Library.ConvertToDouble((string)obj["total"]);
                double t_ck_tt_nt = Library.ConvertToDouble((string)obj["discount_total"]);

                //ma-voucher
                string ma_voucher = GetVoucher(obj);


                string status = (string)obj["status"] ?? "";

                WP_ObjCustomer customer = GetCustomer(obj);
                WP_ObjCustomer shipping = GetShipping(obj);



                str = "declare @q nvarchar(4000) = ''";

                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#master') IS NOT NULL DROP TABLE #master;";
                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#detail') IS NOT NULL DROP TABLE #detail;";
                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#return') IS NOT NULL DROP TABLE #return;";

                str += Environment.NewLine + "select top 0 * into #master from m81$000000";
                str += Environment.NewLine + "select top 0 * into #detail from d81$000000";
                str += Environment.NewLine + "select top 0 * into #return from d816$000000";
                str += Environment.NewLine;


                str += Environment.NewLine + " insert into #master ( stt_rec,ma_dvcs,ma_ct,loai_ct,ma_gd,ngay_lct,ngay_ct,so_ct,ma_nt,ty_gia,ma_kh,status,dept_id,ma_tinh,ma_quan,ma_phuong,to_name,to_phone,ma_voucher,so_ct_post,so_dt,dia_chi)";
                str += Environment.NewLine + $"select '','{ma_dvcs}','DXA','1','1','{ngay_ct}','{ngay_ct}','{so_ct}','VND',1,'{customer.phone}','1','{number_haravan}','{shipping.city}','','',N'{shipping.name}','{shipping.phone}',N'{ma_voucher}','{number_haravan}','{shipping.phone}',N'{shipping.address_1}'";

                str += Environment.NewLine;
                str += $"DECLARE @status VARCHAR(50) set @status = '{status}'";
                str += Environment.NewLine;
                str += "Update #master set status = dbo.GetStatusWPOrderOnline(@status)";

                JArray listitem = (JArray)obj["line_items"];
                for (i = 0; i < listitem.Count; i++)
                {
                    var item = (JObject)listitem[i];
                    string ma_vt = ((string)item["sku"] ?? "");
                    string ma_kho = "";
                    double so_luong = Library.ConvertToDouble((string)item["quantity"]);
                    double gia_nt2 = Library.ConvertToDouble((string)item["price"]);
                    double gia_nt = Library.ConvertToDouble((string)item["price"]);
                    double total = Library.ConvertToDouble((string)item["total"]);
                    double subtotal = Library.ConvertToDouble((string)item["subtotal"]);
                    double ck_nt = 0;
                    double thue_nt = 0;
                    string ds_ma_ck = "";
                    double gia_trc_ck = subtotal / so_luong;
                    double tl_ck = (gia_trc_ck - gia_nt) / gia_nt * 100;

                    str += Environment.NewLine + " insert into #detail ( stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_vt, ma_kho, ma_vv, ma_sp, so_luong,gia2, gia_nt2, thue_nt,ck_nt,ck_tt_nt,tien_nt2,tien2,gia_ban,gia_ban_nt,tl_ck)" + Environment.NewLine +
                                $" select '', '{(i + 1).ToString().PadLeft(3, '0')}', 'DXA', '{ngay_ct}', '{so_ct}', {i + 1}, '{ma_vt}', '{ma_kho}', '', '', {so_luong},{gia_trc_ck},{gia_trc_ck}, {thue_nt},{ck_nt},{ck_nt},{subtotal},{subtotal},{gia_trc_ck},{gia_trc_ck},{tl_ck} ";
                }
                i = 0;

                str += Environment.NewLine + $"declare @table_sufix varchar(6) = '{table_sufix}'";

                str += Environment.NewLine + $" exec haravan_CreateOrderDL @table_sufix ";


                //str += Environment.NewLine + $" exec haravan_UpdateManv @stt_rec,'{ngay_ct}' ";
                return str;
            }
            catch (Exception e)
            {
                string number_haravan = ((string)obj["id"] ?? "");
                ApiLog.log("error/Update order/sql creat", "", e.Message, e.ToString(), number_haravan);
                return "";
            }
        }

        public static string CreatSqlOrder(JObject obj)
        {
            try
            {
                string str = String.Empty;

                DateTime myDate = Convert.ToDateTime((string)obj["date_created_gmt"] ?? "");
                myDate = myDate.AddHours(7);
                //var easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                //myDate = TimeZoneInfo.ConvertTime(myDate, easternZone);

                string number_haravan = ((string)obj["id"] ?? "");
                string table_sufix = myDate.ToString("yyyyMM", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                int i = 0;
                string ma_dvcs = "CTY";//dif
                string so_ct = "DL"+((string)obj["number"] ?? "");
                string number_wp = ((string)obj["id"] ?? "");
                string loai_ct = "CT";
                string ma_gd = "GD";
                string ma_nt = (string)(obj["currency"] ?? "");
                string ngay_ct = myDate.ToString("yyyy/MM/dd", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                string ma_kh_haravan = (string)obj["customer_id"];


                double t_tien2 = Library.ConvertToDouble((string)obj["total"]);
                double t_ck_tt_nt = Library.ConvertToDouble((string)obj["discount_total"]);

                //ma-voucher
                string ma_voucher = GetVoucher(obj);


                string status = (string)obj["status"] ?? "";

                WP_ObjCustomer customer = GetCustomer(obj);
                WP_ObjCustomer shipping = GetShipping(obj);

                //------------------------tinh chi phi van chuyen----------------------------
                double t_cp_vc_nt = 0;
                t_cp_vc_nt = Library.ConvertToDouble((string)obj["shipping_total"]);
                double t_thue = 0;
                t_thue = Library.ConvertToDouble((string)obj["total_tax"]);
                //---------------------------------------------------------------------------
                string delivery_time = "";

                str = "declare @q nvarchar(4000) = ''";

                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#master') IS NOT NULL DROP TABLE #master;";
                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#detail') IS NOT NULL DROP TABLE #detail;";
                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#return') IS NOT NULL DROP TABLE #return;";

                str += Environment.NewLine + "select top 0 * into #master from m81$000000";
                str += Environment.NewLine + "select top 0 * into #detail from d81$000000";
                str += Environment.NewLine + "select top 0 * into #return from d816$000000";
                str += Environment.NewLine;


                str += Environment.NewLine + " insert into #master (" +
                    " stt_rec," +
                    "ma_dvcs,ma_ct," +
                    "loai_ct," +
                    "ma_gd," +
                    "ngay_lct," +
                    "ngay_ct," +
                    "so_ct," +
                    "ma_nt," +
                    "ty_gia," +
                    "ma_kh," +
                    "status," +
                    "dept_id," +
                    "s1,"+
                    "ma_voucher," +
                    "so_ct_post," +
                    "so_dt," +
                    "dia_chi)";
                str += Environment.NewLine + $"select ''," +
                    $"'{ma_dvcs}'" +
                    $",'HDO'" +
                    $",'1'" +
                    $",'1'" +
                    $",'{ngay_ct}'" +
                    $",'{ngay_ct}'" +
                    $",'{so_ct}'" +
                    $",'VND'" +
                    $",1" +
                    $",'{customer.phone}'" +
                    $",'1'" +
                    $",'ON01'" +
                    $",'{ma_kh_haravan}'" +
                    $",'{number_haravan}'" +
                    $",'{shipping.city}'" +
                    $",''" +
                    $",''" +
                    $",N'{shipping.name}'" +
                    $",'{shipping.phone}'" +
                    $",N'{ma_voucher}'" +
                    $",'{number_haravan}'" +
                    $",'{shipping.phone}'" +
                    $",N'{shipping.address_1}'";

                str += Environment.NewLine;
                str += $"DECLARE @status VARCHAR(50) set @status = '{status}'";
                str += Environment.NewLine;
                str += "Update #master set status = dbo.GetStatusWPOrderOnline(@status)";

                JArray listitem = (JArray)obj["line_items"];
                for (i = 0; i < listitem.Count; i++)
                {
                    var item = (JObject)listitem[i];
                    string ma_vt = ((string)item["sku"] ?? "");
                    string ma_kho = "";
                    double so_luong = Library.ConvertToDouble((string)item["quantity"]);
                    double gia_nt2 = Library.ConvertToDouble((string)item["price"]);
                    double gia_nt = Library.ConvertToDouble((string)item["price"]);
                    double total = Library.ConvertToDouble((string)item["total"]);
                    double subtotal = Library.ConvertToDouble((string)item["subtotal"]);
                    double ck_nt = 0;
                    double thue_nt = 0;
                    string ds_ma_ck = "";
                    double gia_trc_ck = subtotal / so_luong;
                    double tl_ck = (gia_trc_ck - gia_nt) / gia_nt * 100;

                    str += Environment.NewLine + " insert into #detail ( stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_vt, ma_kho, ma_vv, ma_sp, so_luong,gia2, gia_nt2, thue_nt,ck_nt,ck_tt_nt,tien_nt2,tien2,gia_ban,gia_ban_nt,tl_ck)" + Environment.NewLine +
                                $" select '', '{(i + 1).ToString().PadLeft(3, '0')}', 'HDO', '{ngay_ct}', '{so_ct}', {i + 1}, '{ma_vt}', '{ma_kho}', '', '', {so_luong},{gia_trc_ck},{gia_trc_ck}, {thue_nt},{ck_nt},{ck_nt},{subtotal},{subtotal},{gia_trc_ck},{gia_trc_ck},{tl_ck} ";
                }
                i = 0;

                str += Environment.NewLine + $"declare @table_sufix varchar(6) = '{table_sufix}'";

                str += Environment.NewLine + $" exec create_CreateOrderDL @table_sufix ";


                //str += Environment.NewLine + $" exec haravan_UpdateManv @stt_rec,'{ngay_ct}' ";
                return str;
            }
            catch (Exception e)
            {
                string number_haravan = ((string)obj["id"] ?? "");
                ApiLog.log("error/Update order/sql creat", "", e.Message, e.ToString(), number_haravan);
                return "";
            }
        }
        public static string UpdateSqlOrder(JObject obj, string table_sufix)
        {
            try
            {
                string confrimstatus = ((string)obj["confirmed_status"] ?? "");
                string cancelled_status = ((string)obj["cancelled_status"] ?? "");
                string close_status = ((string)obj["closed_status"] ?? "");
                string str = String.Empty;

                //var easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                //myDate = TimeZoneInfo.ConvertTime(myDate, easternZone);

                int i = 0;
 
                string so_ct = ((string)obj["order_number"] ?? "");
                string id = ((string)obj["id"] ?? "");
 

                string status = (string)obj["status"] ?? "";





                //------------------------tinh chi phi van chuyen----------------------------
                double t_cp_vc_nt = 0;
                t_cp_vc_nt = Library.ConvertToDouble((string)obj["shipping_total"]);
                double t_thue = 0;
                t_thue = Library.ConvertToDouble((string)obj["total_tax"]);
                //---------------------------------------------------------------------------
                WP_ObjCustomer shipping = GetShipping(obj);

                str = "declare @q nvarchar(4000) = ''";

                str += Environment.NewLine;
                str += $"DECLARE @m_stt_rec char(13)='' ";
                str += $"DECLARE @m_ngay_ct smalldatetime ";
                str += $"select top 1 @m_stt_rec = stt_rec,@m_ngay_ct = ngay_ct from   m81${table_sufix} where LTRIM(RTRIM(so_ct_post)) = LTRIM(RTRIM('{id}')) ";

                str += Environment.NewLine;
                str += $"Update m64${table_sufix} set fnote1 =N'{shipping.address_2}',dia_chi=N'{shipping.address_1}'  where LTRIM(RTRIM(stt_rec)) = LTRIM(RTRIM(@m_stt_rec)) ";

                str += Environment.NewLine;
                str += $"DECLARE @status VARCHAR(50) set @status = '{status}'";
                str += Environment.NewLine;
                str += $"Update m64${table_sufix} set status = dbo.GetStatusWPOrderOnline(@status)  where LTRIM(RTRIM(stt_rec)) = LTRIM(RTRIM(@m_stt_rec))";

                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#master') IS NOT NULL DROP TABLE #master;";
                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#detail') IS NOT NULL DROP TABLE #detail;";
                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#return') IS NOT NULL DROP TABLE #return;";

                str += Environment.NewLine + $"select top 1 * into #master from m81${table_sufix} where LTRIM(RTRIM(stt_rec)) = LTRIM(RTRIM(@m_stt_rec)) ";
                str += Environment.NewLine + $"select  * into #detail from d81${table_sufix} where LTRIM(RTRIM(stt_rec)) = LTRIM(RTRIM(@m_stt_rec))";
                str += Environment.NewLine + $"select top 0 * into #return from d816$000000";
                str += Environment.NewLine;

                #region tao tra lai
                //---------------Tạo phiếu trả hang--------------------------
                //JArray listReturn = (JArray)obj["refunds"];
                //if (listReturn.Count > 0)
                //{
                //    bool check_re = false;
                //    str += Environment.NewLine + $"declare @checkexitre char(1)= '0'";
                //    str += Environment.NewLine + $"declare @linenumber int= 1";
                //    for (int j = 0; j < listReturn.Count; j++)
                //    {
                //        JObject reOrder = (JObject)listReturn[j];
                //        string re_so_ct = (string)reOrder["id"] ?? "";
                //        if (Library.IsNullOrEmpty(reOrder["refund_line_items"])) { }
                //        else
                //        {
                //            JArray re_Listitems = (JArray)reOrder["refund_line_items"];
                //            str += Environment.NewLine + $"set @checkexitre ='0'";
                //            str += Environment.NewLine + $"select top 1 @checkexitre = '1' from d816${table_sufix} WHERE LTRIM(RTRIM(s1))=LTRIM(RTRIM('{re_so_ct}')) ";
                //            str += Environment.NewLine + $"if (@checkexitre ='0')";
                //            str += Environment.NewLine + $"begin";
                //            for (int k = 0; k < re_Listitems.Count; k++)
                //            {

                //                JObject re_item = (JObject)re_Listitems[k];
                //                string ma_kho = "";
                //                string ma_vt = (string)re_item["line_item_id"] ?? "";

                //                double so_luong = Library.ConvertToDouble((string)re_item["quantity"]);
                //                str += Environment.NewLine +
                //               " insert into #return ( stt_rec, stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,ma_sp,ma_kho,so_luong,s1,s4,stt_rec_hd)";
                //                str += Environment.NewLine +
                //                $" select @m_stt_rec, '{(k + 1).ToString().PadLeft(3, '0')}','','01/01/2021','','{ma_vt}','','{ma_kho}',{so_luong},'{re_so_ct}',@linenumber,@m_stt_rec ";
                //                str += Environment.NewLine + $"set @linenumber += 1";
                //            }
                //            str += Environment.NewLine + $"end";
                //        }
                //    }
                //    if (check_re)
                //    {


                //        str += Environment.NewLine + $" declare @total int =0 ";
                //        str += Environment.NewLine + $" select @total =count(stt_rec) from d816$202104 where LTRIM(RTRIM(stt_rec))=LTRIM(RTRIM(@m_stt_rec)) ";
                //        str += Environment.NewLine + $" exec haravan_CreateReturn @total ";


                //        str += Environment.NewLine + "insert into d816$" + table_sufix + " select * from #return";


                //        str += Environment.NewLine + $" exec haravan_AfterReturnCreated '{table_sufix}',@m_stt_rec,@m_ngay_ct ";
                //    }
                //}
                #endregion tao tra lai
                str += Environment.NewLine + $"exec haravan_AfterOrderUpdateDL '{table_sufix}',@m_stt_rec";
                return str;
            }
            catch (Exception e)
            {
                string number_haravan = ((string)obj["id"] ?? "");
                ApiLog.log("error/Update order/sql update", "", e.Message, e.ToString(), number_haravan);
                return "";
            }
        }
    }
}






