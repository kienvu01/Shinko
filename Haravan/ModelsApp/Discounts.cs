using Haravan.FuncLib;
using Haravan.Model;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.ModelsApp
{
    public class DisCountDetail
    {
        public dynamic applies_once { get; set; }
        public dynamic applies_to_id { get; set; }
        public dynamic code { get; set; }
        public dynamic ends_at { get; set; }
        public dynamic id { get; set; }
        public dynamic minimum_order_amount { get; set; }
        public dynamic starts_at { get; set; }
        public dynamic status { get; set; }
        public dynamic usage_limit { get; set; }
        public dynamic value { get; set; }
        public dynamic discount_type { get; set; }
        public dynamic times_used { get; set; }
        public dynamic is_promotion { get; set; }
        public dynamic applies_to_resource { get; set; }
        public dynamic variants { get; set; }
        public dynamic location_ids { get; set; }
        public dynamic create_user { get; set; }
        public dynamic first_name { get; set; }
        public dynamic last_name { get; set; }
        public dynamic applies_customer_group_id { get; set; }
        public dynamic created_at { get; set; }
        public dynamic updated_at { get; set; }
        public dynamic promotion_apply_type { get; set; }
        public dynamic applies_to_quantity { get; set; }
        public dynamic order_over { get; set; }
        public dynamic is_new_coupon { get; set; }
        public dynamic channel { get; set; }
        public dynamic max_amount_apply { get; set; }
        public dynamic is_advance_same_price_discount { get; set; }
        public dynamic advance_same_prices { get; set; }
    }
    public class Province
    {
        public double code { get; set; }
        public string name { get; set; }
        public Province(double _code , string _name)
        {
            code = _code;
            name = _name;
        }
    }
    public class LineItemsHis
    {
        public string ma_vt { get; set; }
        public double sl { get; set; }
        public LineItemsHis(string _ma_vt, double _sl)
        {
            ma_vt = _ma_vt;
            sl = _sl;
        }
    }
    public class Discounts
    {
        public IConfiguration config;
        public string contentre;

        public Discounts(IConfiguration _config)
        {
            config = _config;
        }
        public static Object discounts_create(Object data)
        {
            Object re = new { };
            return re;
        }
        public static Object discounts_update(Object data)
        {
            Object re = new { };
            return re;
        }
        public static Object discounts_delete(Object data)
        {
            Object re = new { };
            return re;
        }

        public async Task<ResponseData> CheckDisCount_test(Object data)
        {
            string s = data.ToString();
            JObject obj = JObject.Parse(s);
            List<LineItem> line_items = new List<LineItem>();
            List<LineItemsHis> line_item_his = new List<LineItemsHis>();

            string ma_km = (string)obj["ma_km"] ?? "";
            string ma_kh = (string)obj["ma_kh"] ?? "";
            if (ma_km == null || ma_km == "") return new ResponseData("err", "Chưa truyền mã khuyến mãi", "");
            if (ma_kh == null || ma_kh == "") return new ResponseData("err", "Chưa truyền mã khách hàng", "");
            string id_har = await Customers.getIdCustomerByID(ma_kh);
            if (id_har == "") {
                Res_NewCustomer newCus = new Res_NewCustomer();
                string sql_getcus = $"select * from dmkh where ma_kh='{ma_kh.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql_getcus);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    return new ResponseData("err", $"Khách hàng {ma_kh} không tồn tại", "");
                }
                newCus.ma_kh = ma_kh.Trim();
                newCus.ten_kh = ds.Tables[0].Rows[0]["ten_kh"].ToString();
                newCus.dia_chi = ds.Tables[0].Rows[0]["dia_chi"].ToString();
                newCus.ma_tinh = ds.Tables[0].Rows[0]["tinh_thanh"].ToString();
                newCus.ma_phuong = ds.Tables[0].Rows[0]["phuong_xa"].ToString();
                newCus.email = ds.Tables[0].Rows[0]["e_mail"].ToString();
                newCus.ma_quan = ds.Tables[0].Rows[0]["quan_huyen"].ToString();
                Customers cus = new Customers(config);
                ResponseData res_creat = await cus.CreatNewCustomer(newCus);
                if (res_creat.status == "ok") {
                    JObject root_data = JObject.Parse(res_creat.data);
                    JObject lst_dis = (JObject)root_data["customer"];
                    id_har = (string)lst_dis["id"] ?? "";
                }
                else return res_creat;
            }

            JArray list_item = (JArray)obj["items"];
            for (int i = 0; i < list_item.Count; i++)
            {
                JObject row = (JObject)list_item[i];
                string barcode = (string)row["barcode"];
                double so_luong = (double)row["so_luong"];
                Products p = new Products(config);
                ResponseData res_product = await p.GETVarianFromBarCode(barcode);
                if (res_product.status == "ok")
                {
                    string product_id = res_product.data.product_id;
                    string product_variant_id = res_product.data.product_variant_id;
                    double price = res_product.data.price;
                    double price_after = (double)row["gia_haravan"];
                    line_items.Add(new LineItem(product_variant_id, product_id, price_after, so_luong, price));
                    line_item_his.Add(new LineItemsHis(barcode, so_luong));
                }
                else
                {
                    return res_product;
                }
            }
            if (line_items.Count == 0) return new ResponseData("err", "Chưa nhập thông tin sản phẩm", "");

            string discountDetail = "";
            JObject discount;
            ResponseApiHaravan res_km = await GetDiscount(ma_km);
            if (res_km.status == "err") return new ResponseData("err", "Mã giảm giá không tồn tại trên Haravan ", "");
            else
            {
                discountDetail = res_km.data;
                JObject root_data = JObject.Parse(res_km.data);
                JArray lst_dis = (JArray)root_data["discounts"];
                if (lst_dis.Count == 0) return new ResponseData("err", "Mã giảm giá không tồn tại trên Haravan ", "");
                else discount = (JObject)lst_dis[0];
            }
            List<object> list_discount = new List<object>();
            list_discount.Add(new
            {
                code = ma_km,
                //amount = 1,
                is_coupon_code = true
            });
            Object body = new
            {
                order = new
                {
                    email = "sse.net.vn@example.com",
                    fulfillment_status = "fulfilled",
                    discount_codes = list_discount,
                    note = "Check discount bằng SSE",
                    line_items = line_items,
                    source_name = "pos",
                    source = "pos",
                    customer = new
                    {
                        id = id_har
                    }
                }
            };
            JObject temp = JObject.FromObject(body);
            var token = config.GetValue<string>("config_Haravan:private_token");
            string url = $"https://apis.haravan.com/com/orders.json";
            HttpClientApp client = new HttpClientApp(token);
            ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
            if (res.status == "ok")
            {
                JObject order_root = JObject.Parse(res.data);
                JObject order = (JObject)order_root["order"];
                double totaldiscount = (double)order["total_discounts"];
                string ngay_kt = "";
                if (!Library.IsNullOrEmpty(discount["ends_at"]))
                {
                    DateTime ends_at = Convert.ToDateTime((string)discount["ends_at"]);
                    ends_at = ends_at.AddHours(7);
                    ngay_kt = ends_at.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                }


                //decimal ma_his = InsertHistoryCheck(ma_km.Trim(), ngay_kt, totaldiscount,"");
                Object datakm = new
                {
                    IdHistory = 0,
                    TotalDisCount = totaldiscount,
                    DisCountDetail = discountDetail
                };
                return new ResponseData("ok", "Mã khuyến mãi hợp lệ", datakm);
            }
            else
            {
                return new ResponseData("err", res.data, "");        
            }

        }
        public async Task<ResponseApiHaravan> GetDiscount(string ma_km)
        {
            var token = config.GetValue<string>("config_Haravan:private_token");
            string url = $"https://apis.haravan.com/com/discounts.json?code={ma_km}";
            HttpClientApp client = new HttpClientApp(token);
            ResponseApiHaravan res_km = await client.Get_Request(url);
            return res_km;
        }
        public async Task<ResponseApiHaravan> creatTest(string ma_km)
        {
            string discountDetail = "";
            JObject discount;
            ResponseApiHaravan res_km = await GetDiscount(ma_km);
            if (res_km.status == "err") return new ResponseApiHaravan("err", "Mã giảm giá không tồn tại trên Haravan ", "");
            else
            {
                discountDetail = res_km.data;
                JObject root_data = JObject.Parse(res_km.data);
                JArray lst_dis = (JArray)root_data["discounts"];
                if (lst_dis.Count == 0) return new ResponseApiHaravan("err", "Mã giảm giá không tồn tại trên Haravan ", "");
                else discount = (JObject)lst_dis[0];
            }

            var token = config.GetValue<string>("config_Haravan:private_token");

            string code = (string)discount["code"] ?? "";

            discount.Property("id").Remove();
            string s = discount.ToString();
            
            s = "{\"discount\":" + s;
            s = s.Replace(code, "SSE_DDD");
            s = s + "}";

            string url = $"https://apis.haravan.com/com/discounts.json";
            HttpClientApp client = new HttpClientApp(token);
            ResponseApiHaravan res = await client.Post_Request_WithBody(url,s);
            return res;
        }

        public async Task<ResponseApiHaravan> deleteMaKM(string id_km)
        {
            var token = config.GetValue<string>("config_Haravan:private_token");

            string url = $"https://apis.haravan.com/com/discounts/{id_km}.json";
            HttpClientApp client = new HttpClientApp(token);
            ResponseApiHaravan res = await client.Del_Request(url);
            return res;
        }


        public decimal InsertHistoryCheck(string ma_km,string? date,double total_dis,string ma_gg2)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            conn.Open();
            try
            {
                string valdate = "";
                if (date == "") valdate = "NULL";
                else valdate = $"'{date}'";
                string sql = "declare @index numeric(18,0)=0";
                sql += Environment.NewLine + "select @index =isnull(max(id),0)+1 from his_checkdiscount";
                sql += Environment.NewLine + $"insert into his_checkdiscount(id,creattime,ma_hd,ma_gg,ma_gg2,tg_hl,to_discount)";
                sql += Environment.NewLine + $"values(@index,getdate(), '', '{ma_km}','{ma_gg2}',{valdate}, {total_dis})";

                sql += "select @index as id";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                decimal id = (decimal)ds.Tables[0].Rows[0]["id"];
                return id;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return 0;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
        public decimal UpdateHistoryCheck(decimal id,string ma_gg2)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            conn.Open();
            try
            {
                string sql = $"update his_checkdiscount set ma_gg2 = '{ma_gg2}' where id = {id}";

                SqlCommand sql_cmnd = new SqlCommand(sql, conn);
                sql_cmnd.ExecuteNonQuery();
                return id;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                return id;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }


    }
}
