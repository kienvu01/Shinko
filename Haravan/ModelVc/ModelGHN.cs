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
using Haravan.ModelsApp;

namespace Haravan.ModelVc
{
    public class OrderCreatNew
    {
        public int? payment_type_id { get; set; }
        public string note { get; set; }
        public string required_note { get; set; }


        public string return_name { get; set; }
        public string return_phone { get; set; }
        public string return_address { get; set; }
        public int? return_district_id { get; set; }
        public string return_ward_code { get; set; }
        //-------------------------------------------------------------------------------------------------------------------------------------
        public string from_name { get; set; }
        public string from_phone { get; set; }
        public string from_address { get; set; }
        public int? from_district_id { get; set; }
        public string from_ward_code { get; set; }
        //-------------------------------------------------------------------------------------------------------------------------------------
        public string client_order_code { get; set; }
        public string to_name { get; set; }
        public string to_phone { get; set; }
        public string to_address { get; set; }
        public string to_ward_code { get; set; }
        public int? to_district_id { get; set; }
        //-------------------------------------------------------------------------------------------------------------------------------------
        public int? cod_amount { get; set; }
        public string content { get; set; }

        public int? weight { get; set; }
        public int? length { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------
        public double? deliver_station_id { get; set; }
        public int? insurance_value { get; set; }
        public int? service_id { get; set; }
        public int? service_type_id { get; set; }
        public int? order_value { get; set; }
        public string coupon { get; set; }

        public List<ItemLineGHN> items { get; set; }
        public OrderCreatNew() { }
    }
    public class ItemLineGHN
    {
        public string name { get; set; }
        public string code { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
        public ItemLineGHN() { }
        public ItemLineGHN(string _name, string _code, int _quantity, int _pricee)
        {
            name = _name;
            code = _code;
            quantity = _quantity;
            price = _pricee;
        }
    }
    public class GHNAddress
    {
        public string ma_tinh { get; set; }
        public string ma_quan { get; set; }
        public string ma_phuong { get; set; }
        public string address { get; set; }
        public string phone { get; set; }

        public string status { get; set; }
        public GHNAddress(string _ma_tinh,string _ma_quan,string _ma_phuong,string _address,string _phone,string _status)
        {
            ma_tinh = _ma_tinh;
            ma_quan = _ma_quan;
            ma_phuong = _ma_phuong;
            address = _address;
            phone = _phone;
            status = _status;
        }
    }
    public class ModelGHN
    {
        public IConfiguration config;

        public ModelGHN(IConfiguration _config)
        {
            config = _config;
        }

        public async Task<string> SQLDongBo_Tinh(List<string> lst_tinh, bool auto_insert)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                string sql = "";
                string sql_gettinh = "";
                string sql_condition = "''";
                foreach (string s in lst_tinh)
                {
                    sql_condition += $",'{s}'";
                }
                if (sql_condition == "''") sql_gettinh = $"select * from hrdmtinh";
                else sql_gettinh = $"select * from hrdmtinh where ma_tinh in ({sql_condition})";

                SqlDataAdapter da = new SqlDataAdapter(sql_gettinh, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);

                var token = config.GetValue<string>("Vanchuyen:GHN:privateToken");
                string url = $"https://online-gateway.ghn.vn/shiip/public-api/master-data/province";
                HttpClientApp client = new HttpClientApp();
                client.AddHeader("Token", token);
                ResponseApiHaravan res = await client.Get_Request(url);
                if (res.status == "ok")
                {
                    JObject root_list_tinh = JObject.Parse(res.data);
                    JArray list_tinh = (JArray)root_list_tinh["data"];
                    for (int i = 0; i < list_tinh.Count; i++)
                    {
                        string ghn_ma_tinh = (string)list_tinh[i]["ProvinceID"];
                        List<string> ghn_extension_tinh = list_tinh[i]["NameExtension"].ToObject<List<string>>();
                        bool check = false;
                        foreach (DataRow r in ds.Tables[0].Rows)
                        {
                            string ma_tinh = r["ma_tinh"].ToString();
                            string ten_tinh = r["ten_tinh"].ToString();
                            var temp = ghn_extension_tinh.FirstOrDefault(c =>
                            Library.convertToUnSign(ten_tinh.ToUpper().Replace(" ", String.Empty)) == Library.convertToUnSign(c.ToUpper().Replace(" ", String.Empty)));
                            if (temp != null)
                            {
                                check = true;
                                sql += Environment.NewLine + $"update hrdmtinh set ghn_ma_tinh = '{ghn_ma_tinh}' where ma_tinh='{ma_tinh}' ";
                                string sql_quan = await SQLDongBo_Quan(ghn_ma_tinh, ma_tinh, auto_insert);
                                sql += sql_quan;
                                break;
                            }
                        }
                    }
                }
                return sql;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return "";
            }
        }

        public async Task<string> SQLDongBo_Quan(string ghn_ma_tinh, string ma_tinh, bool auto_insert)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                string sql = "";

                int index = 0;

                string sql_getindex = $"select MAX(REPLACE(ma_quan,ma_tinh,'')) as num from hrdmquan where ma_tinh = '{ma_tinh}'  group by ma_tinh";
                SqlDataAdapter da = new SqlDataAdapter(sql_getindex, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0) index = Convert.ToInt32(ds.Tables[0].Rows[0]["num"].ToString());


                string sql_getquan = $"select * from hrdmquan where ma_tinh='{ma_tinh}'";
                da = new SqlDataAdapter(sql_getquan, conn);
                ds = new DataSet();
                da.Fill(ds);


                var token = config.GetValue<string>("Vanchuyen:GHN:privateToken");
                string url = $"https://online-gateway.ghn.vn/shiip/public-api/master-data/district?province_id={ghn_ma_tinh}";
                HttpClientApp client = new HttpClientApp();
                client.AddHeader("Token", token);
                ResponseApiHaravan res = await client.Get_Request(url);
                if (res.status == "ok")
                {
                    JObject root_list_quan = JObject.Parse(res.data);
                    JArray list_quan = (JArray)root_list_quan["data"];
                    for (int i = 0; i < list_quan.Count; i++)
                    {
                        string ghn_ma_quan = (string)list_quan[i]["DistrictID"];
                        if (Library.IsNullOrEmpty(list_quan[i]["NameExtension"])) { }
                        else
                        {
                            List<string> ghn_extension_quan = list_quan[i]["NameExtension"].ToObject<List<string>>();
                            bool check = false;
                            foreach (DataRow r in ds.Tables[0].Rows)
                            {
                                string ma_quan = r["ma_quan"].ToString();
                                string ten_quan = r["ten_quan"].ToString();
                                var temp = ghn_extension_quan.FirstOrDefault(c =>
                                Library.convertToUnSign(ten_quan.ToUpper().Replace(" ", String.Empty)) == Library.convertToUnSign(c.ToUpper().Replace(" ", String.Empty)));
                                if (temp != null)
                                {
                                    check = true;
                                    sql += Environment.NewLine + $"update hrdmquan set ghn_ma_quan = '{ghn_ma_quan}' where ma_tinh='{ma_tinh}' and ma_quan='{ma_quan}' ";
                                    string sql_phuong = await SQLDongBo_PhuongXa(ghn_ma_quan, ma_quan, ma_tinh, auto_insert);
                                    sql += sql_phuong;
                                    break;
                                }
                            }
                            if (!check && auto_insert)
                            {
                                string ten = (string)list_quan[i]["DistrictName"];
                                index += 1;
                                string creat_ma_quan = ma_tinh.Trim() + index.ToString("D2");
                                sql += Environment.NewLine + $"insert into hrdmquan values('{ma_tinh}','{creat_ma_quan}',N'{ten.Replace("'","''")}','',0,'1',GETDATE(),GETDATE(),1,1,'{ghn_ma_quan}')";
                                string sql_phuong = await SQLDongBo_PhuongXa(ghn_ma_quan, ghn_ma_quan, ma_tinh, auto_insert);
                                sql += sql_phuong;
                            }
                        }
                    }
                }
                return sql;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return "";
            }
        }

        public async Task<string> SQLDongBo_PhuongXa(string ghn_ma_quan, string ma_quan, string ma_tinh, bool auto_insert)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                string sql = "";
                int index = 0;

                string sql_getindex = $"select MAX(REPLACE(ma_phuong,ma_quan,'')) as num from hrdmphuong where ma_tinh = '{ma_tinh}' and ma_quan ='{ma_quan}' group by ma_tinh,ma_quan";
                SqlDataAdapter da = new SqlDataAdapter(sql_getindex, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0) index =Convert.ToInt32(ds.Tables[0].Rows[0]["num"]);


                string sql_getphuong = $"select * from hrdmphuong where ma_tinh='{ma_tinh}' and ma_quan='{ma_quan}'";
                da = new SqlDataAdapter(sql_getphuong, conn);
                ds = new DataSet();
                da.Fill(ds);


                var token = config.GetValue<string>("Vanchuyen:GHN:privateToken");
                string url = $"https://online-gateway.ghn.vn/shiip/public-api/master-data/ward?district_id={ghn_ma_quan}";
                HttpClientApp client = new HttpClientApp();
                client.AddHeader("Token", token);
                ResponseApiHaravan res = await client.Get_Request(url);
                if (res.status == "ok")
                {
                    JObject root_list_phuong = JObject.Parse(res.data);
                    JArray list_phuong = (JArray)root_list_phuong["data"];
                    for (int i = 0; i < list_phuong.Count; i++)
                    {
                        string ghn_ma_phuong = (string)list_phuong[i]["WardCode"];
                        if (Library.IsNullOrEmpty(list_phuong[i]["NameExtension"])) { }
                        else
                        {
                            List<string> ghn_extension_phuong = list_phuong[i]["NameExtension"].ToObject<List<string>>();
                            bool check = false;
                            foreach (DataRow r in ds.Tables[0].Rows)
                            {
                                string ma_phuong = r["ma_phuong"].ToString();
                                string ten_phuong = r["ten_phuong"].ToString();
                                var temp = ghn_extension_phuong.FirstOrDefault(c =>
                                Library.convertToUnSign(ten_phuong.ToUpper().Replace(" ", String.Empty)) == Library.convertToUnSign(c.ToUpper().Replace(" ", String.Empty)));
                                if (temp != null)
                                {
                                    check = true;
                                    sql += Environment.NewLine + $"update hrdmphuong set ghn_ma_phuong = '{ghn_ma_phuong}' where ma_tinh='{ma_tinh}' and ma_quan='{ma_quan}' and ma_phuong='{ma_phuong}' ";
                                    break;
                                }
                            }
                            if (!check && auto_insert)
                            {
                                string ten = (string)list_phuong[i]["WardName"];
                                index += 1;
                                string creat_ma_phuong = ma_quan.Trim() + index.ToString("D3");
                                sql += Environment.NewLine + $"insert into hrdmphuong values('{ma_tinh}','{ma_quan}','{creat_ma_phuong}',N'{ten.Replace("'", "''")}','',0,'1',GETDATE(),GETDATE(),1,1,'{ghn_ma_phuong}')";
                            }
                        }
                    }
                }
                return sql;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return "";
            }
        }

        public async Task<int> GetService(string from_ma_quan, string to_ma_quan, int? service_type_id)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                if (from_ma_quan != "" && to_ma_quan != "")
                {
                    var token = config.GetValue<string>("Vanchuyen:GHN:privateToken");
                    var shopid = config.GetValue<string>("Vanchuyen:GHN:ShopId");
                    string url = $"https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/available-services?shop_id={shopid}&from_district={from_ma_quan}&to_district={to_ma_quan}";
                    HttpClientApp client = new HttpClientApp();
                    client.AddHeader("Token", token);
                    ResponseApiHaravan res = await client.Get_Request(url);
                    int service = 53320;
                    if (res.status == "ok")
                    {
                        JObject root_servies = JObject.Parse(res.data);
                        JArray list_servies = (JArray)root_servies["data"];
                        for (int j = 0; j < list_servies.Count; j++)
                        {
                            int t_service_type_id = (int)list_servies[j]["service_type_id"];
                            int t_service_id = (int)list_servies[j]["service_id"];
                            if (t_service_type_id == service_type_id)
                            {
                                service = t_service_id;
                                break;
                            }
                        }
                    }
                    return service;
                }
                else return 53320;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return 53320;
            }
        }



        public ResponseData addnewm99(GiaoHang data , OrderCreatNew order,string ma_ghn,string tggk,decimal tien_vc,DataRow master) 
        {
            var constring = MyAppData.config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                conn.Open();
                string ma_dvcs = master["ma_dvcs"].ToString();
                string ma_kh = master["ma_kh"].ToString();
                string ma_nt = master["ma_nt"].ToString();
                decimal ty_gia = (decimal)master["ty_gia"];
                string to_tinh_thanh = master["ma_tinh"].ToString();

                string table_sufix =Convert.ToDateTime(master["ngay_ct"].ToString()).ToString("yyyyMM", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                int? cho_xem_hang = data.cho_xem_hang;
                int? cho_thu_hang = data.cho_thu_hang;

                string str = "";
                str = "declare @q nvarchar(4000) = ''";

                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#master') IS NOT NULL DROP TABLE #master;";
                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#detail') IS NOT NULL DROP TABLE #detail;";
                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#return') IS NOT NULL DROP TABLE #return;";

                str += Environment.NewLine + "select top 0 * into #master from m99$000000";
                str += Environment.NewLine + "select top 0 * into #detail from d99$000000";
                str += Environment.NewLine;
                str += Environment.NewLine + " insert into #master (stt_rec,ma_ct,ma_gh,t_vc,ma_dvcs,ma_kh,ma_nt,ty_gia, ma_hd , ma_ghn , dv_vc , pt_vc , nguoi_tra,cho_xem_hang,cho_thu_hang,tg_du_kien," +
                    "trong_luong,chieu_dai,chieu_rong,chieu_cao,boi_thuong_dh,content,to_phone,to_name,to_address,to_tinh_thanh,to_quan_huyen,to_phuong_xa)" + Environment.NewLine +
                       $" select '','','',{tien_vc},'{ma_dvcs}','{ma_kh}','{ma_nt}',{ty_gia},'{order.client_order_code}','{ma_ghn}',1,{order.service_type_id},{order.payment_type_id},{cho_xem_hang},{cho_thu_hang},'{tggk}'," +
                       $"{order.weight},{order.length},{order.width},{order.height},{order.insurance_value},'{order.note}','{order.to_phone}',N'{order.to_name}',N'{order.to_address}','{to_tinh_thanh}','{order.to_district_id}','{order.to_ward_code}' ";
                int lineid = 0;
                foreach(ItemLineGHN items in order.items)
                {
                    lineid += 1;
                    str += Environment.NewLine + " insert into #detail (stt_rec,ma_ct,so_ct,ma_sp,stt_rec0 ,ngay_ct, ma_vt , so_luong , gia_nt,line_nbr)" + Environment.NewLine +
                        $"select '','','','','{(lineid + 1).ToString().PadLeft(3, '0')}','','{items.code}',{items.quantity},{items.price},{lineid}";
                }


                str += Environment.NewLine + "declare @wsID varchar(1), @stt_rec char(13), @action varchar(10) = 'New' select @wsID = rtrim(val) from options where upper(name) = 'M_WS_ID'";
                str += Environment.NewLine + "create table #idNumber (stt_rec varchar(32))";
                str += Environment.NewLine + "insert into #idNumber exec fs_GetIdentityNumber @wsID, 'SPI', c99$000000";
                str += Environment.NewLine + "select @stt_rec = stt_rec from #idNumber drop table #idNumber";
                str += Environment.NewLine + "update #master set stt_rec = @stt_rec";
                str += Environment.NewLine + "update #detail set stt_rec = @stt_rec";

                str += Environment.NewLine + $" exec haravan_CreateVanChuyen ";

                str += Environment.NewLine + "insert into m99$" + table_sufix + " select * from #master";
                str += Environment.NewLine + "insert into d99$" + table_sufix + " select * from #detail";
                str += Environment.NewLine + $" exec haravan_AfterCreateVanChuyen '{table_sufix}',@stt_rec ";

                SqlCommand sql_cmnd = new SqlCommand(str, conn);
                sql_cmnd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return new ResponseData("err", e.Message,"");
            }
        }

        public GHNAddress GetAddressByKho(string ma_kho)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                string sql = "";
                string sql_getphuong = $"select * from dmkho where ma_kho ='{ma_kho.Trim()}'";
                SqlDataAdapter da = new SqlDataAdapter(sql_getphuong, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count == 0) return new GHNAddress("", "", "", "", "", "KHONGCOKHO");
                int ma_quan = GetGHN_Ma_quan(ds.Tables[0].Rows[0]["ma_quan"].ToString());
                string ma_tinh = GetGHN_Ma_tinh(ds.Tables[0].Rows[0]["ma_tinh"].ToString());
                string ma_phuong = GetGHN_Ma_phuong(ds.Tables[0].Rows[0]["ma_phuong"].ToString());
                string so_dt = ds.Tables[0].Rows[0]["dien_thoai"].ToString();
                string address = ds.Tables[0].Rows[0]["dia_chi"].ToString();
                return new GHNAddress(ma_tinh, ma_quan.ToString(), ma_phuong, address, so_dt,"ok");
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return new GHNAddress("", "", "", "", "", "KHONGCOKHO");
            }
        }

        public GHNAddress GetAddressByCuaHang(string ma_kho)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                string sql = "";
                string sql_getphuong = $"select top 1 * from dmkho where ma_bp ='{ma_kho.Trim()}'";
                SqlDataAdapter da = new SqlDataAdapter(sql_getphuong, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count == 0) return new GHNAddress("", "", "", "", "", "KHONGCOKHO");
                int ma_quan = GetGHN_Ma_quan(ds.Tables[0].Rows[0]["ma_quan"].ToString());
                string ma_tinh = GetGHN_Ma_tinh(ds.Tables[0].Rows[0]["ma_tinh"].ToString());
                string ma_phuong = GetGHN_Ma_phuong(ds.Tables[0].Rows[0]["ma_phuong"].ToString());
                string so_dt = ds.Tables[0].Rows[0]["dien_thoai"].ToString();
                string address = ds.Tables[0].Rows[0]["ten_kho2"].ToString();
                return new GHNAddress(ma_tinh, ma_quan.ToString(), ma_phuong, address, so_dt, "ok");
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return new GHNAddress("", "", "", "", "", "KHONGCOKHO");
            }
        }

        public string GetReturnPhone(string ma_dvcs)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                string sql = "";
                string sql_getphuong = $"select top 1 * from cdmbp where ma_dvcs = '{ma_dvcs}'";
                SqlDataAdapter da = new SqlDataAdapter(sql_getphuong, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                string re = ds.Tables[0].Rows[0]["s1"].ToString();
                return re;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return "";
            }
        }
        public GHNAddress GetAddressByDVCS(string ma_kho)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                string sql = "";
                string sql_getphuong = $"select top 1 * from dmkho where ma_dvcs ='{ma_kho.Trim()}'";
                SqlDataAdapter da = new SqlDataAdapter(sql_getphuong, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count == 0) return new GHNAddress("", "", "", "", "", "KHONGCOKHO");
                int ma_quan = GetGHN_Ma_quan(ds.Tables[0].Rows[0]["ma_quan"].ToString());
                string ma_tinh = GetGHN_Ma_tinh(ds.Tables[0].Rows[0]["ma_tinh"].ToString());
                string ma_phuong = GetGHN_Ma_phuong(ds.Tables[0].Rows[0]["ma_phuong"].ToString());
                string so_dt = ds.Tables[0].Rows[0]["dien_thoai"].ToString();
                string address = ds.Tables[0].Rows[0]["ten_kho2"].ToString();
                return new GHNAddress(ma_tinh, ma_quan.ToString(), ma_phuong, address, so_dt, "ok");
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return new GHNAddress("", "", "", "", "", "KHONGCOKHO");
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        public int GetGHN_Ma_quan(string ma_quan)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                string sql = "";
                string sql_getphuong = $"select top 1 * from hrdmquan where ma_quan='{ma_quan}' ";
                SqlDataAdapter da = new SqlDataAdapter(sql_getphuong, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                int ghn_ma_quan = Convert.ToInt32( ds.Tables[0].Rows[0]["ghn_ma_quan"].ToString());
                return ghn_ma_quan;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return -1;
            }
        }
        public string GetGHN_Ma_phuong(string ma_phuong)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                string sql = "";
                string sql_getphuong = $"select top 1 * from hrdmphuong where ma_phuong='{ma_phuong}' ";
                SqlDataAdapter da = new SqlDataAdapter(sql_getphuong, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                string ghn_ma_phuong = ds.Tables[0].Rows[0]["ghn_ma_phuong"].ToString();
                return ghn_ma_phuong;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return "";
            }
        }
        public string GetGHN_Ma_tinh(string ma_tinh)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                string sql = "";
                string sql_getphuong = $"select top 1 * from hrdmtinh where ma_tinh='{ma_tinh}' ";
                SqlDataAdapter da = new SqlDataAdapter(sql_getphuong, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                string ghn_ma_tinh = ds.Tables[0].Rows[0]["ghn_ma_tinh"].ToString();
                return ghn_ma_tinh;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return "";
            }
        }
        public static bool CheckOrderClose(string stt_rec,string tbl)
        {
            var constring =  MyAppData.config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            conn.Open();
            try
            {
                string sql = $"SELECT ISNULL(status,0) AS status FROM m81${tbl} WHERE stt_rec='{stt_rec}'";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count == 0) return false;
                int status = Convert.ToInt32(ds.Tables[0].Rows[0]["status"].ToString());
                if (status == 4) return true;
                else return false;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return false;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        public static bool CheckCodAmont(string stt_rec,string tbl,decimal tien_thu_ho)
        {
            var constring = MyAppData.config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                string sql = $"select * from v03${tbl} where stt_rec='{stt_rec}'";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count == 0) return false;
                decimal cod = (decimal)ds.Tables[0].Rows[0]["tt_cod_nt"];
                if (cod - tien_thu_ho == 0) return true;
                else return false;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return false;
            }
        }

        public async Task<string> GetOrderCreate(string order_code)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                var token = config.GetValue<string>("Vanchuyen:GHN:privateToken");
                var shopid = config.GetValue<string>("Vanchuyen:GHN:ShopId");
                string url = $"https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/detail?order_code={order_code}";
                HttpClientApp client = new HttpClientApp();
                client.AddHeader("Token", token);
                ResponseApiHaravan res = await client.Get_Request(url);
                if (res.status == "ok")
                {
                    JObject root_data = JObject.Parse(res.data);
                    string d = (string)root_data["data"]["order_date"];
                    DateTime order_date = Convert.ToDateTime(d);
                    string y = order_date.Year.ToString();
                    string m = order_date.Month.ToString("D2");
                    return y + m;
                }
                return "";
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return "";
            }
        }

        public static void UpdateStatusOrder(string stt_rec)
        {
            try
            {
                string sql_up = $"exec ghn_Fastdelivery '{stt_rec.Trim()}','',''";
                DAL.DAL_SQL.ExecuteNonquery(sql_up);
            }
            catch (Exception e)
            {
            }
        }
        public GiaoHang ConvertToGiaoHang(JObject obj)
        {
            try
            {
                GiaoHang gh = new GiaoHang();
                gh.stt_rec = obj["stt_rec"].ToString();
                gh.tablesufix = obj["tablesufix"].ToString();
                gh.pt_vc = Convert.ToInt32(obj["pt_vc"].ToString());
                gh.note = obj["note"].ToString();
                gh.nguoi_tra = Convert.ToInt32(obj["nguoi_tra"].ToString());
                gh.cho_xem_hang = Convert.ToInt32(obj["cho_xem_hang"].ToString());
                gh.cho_thu_hang = Convert.ToInt32(obj["cho_thu_hang"].ToString());
                gh.to_name = obj["to_name"].ToString();
                gh.to_phone = obj["to_phone"].ToString();
                gh.chieu_cao = Convert.ToInt32(obj["chieu_cao"].ToString());
                gh.chieu_dai = Convert.ToInt32(obj["chieu_dai"].ToString());
                gh.trong_luong = Convert.ToInt32(obj["trong_luong"].ToString());
                gh.chieu_rong = Convert.ToInt32(obj["chieu_rong"].ToString());
                gh.tien_boi_thuong = Convert.ToInt32(obj["tien_boi_thuong"].ToString());
                gh.tien_thu_ho = Convert.ToInt32(obj["tien_thu_ho"].ToString());
                gh.ma_km = obj["ma_km"].ToString();
                return gh;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

        }
    }
}
