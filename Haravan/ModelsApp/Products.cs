using Haravan.Controllers;
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
    public class Res_GETVarian
    {
        public string ma_vt { get; set; }
        public string product_variant_id {get;set;}
        public string product_id { get; set; }
    }
    public class Products
    {
        public IConfiguration config;

        public Products(IConfiguration _config)
        {
            config = _config;
        }
        public static ResponseData products_create(Object data)
        {
            try
            {
                string temp = data.ToString();
                JObject obj = JObject.Parse(temp);

                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public static ResponseData products_update(Object data)
        {
            try
            {
                string temp = data.ToString();
                JObject obj = JObject.Parse(temp);

                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public static ResponseData products_deleted(Object data)
        {
            try
            {
                string temp = data.ToString();
                JObject obj = JObject.Parse(temp);

                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public async Task<ResponseData> CreatNewProduct(Object obj)
        {
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                conn.Open();
                JObject p = JObject.FromObject(obj);
                Object body = new
                {
                    title = (string)p["title"],

                };

                var token = config.GetValue<string>("config_Haravan:private_token");

                string url = $"https://apis.haravan.com/com/product.json";
                HttpClientApp client = new HttpClientApp(token);
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, body.ToString());
                JObject data = JObject.Parse(res.data);
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public async Task<ResponseData> CheckExitBarcode(string barcode)
        {
            try
            {
                if (barcode == null || barcode == "")
                    return new ResponseData("err", "Không có sản phẩm nào có barcode này trên haravan 1", "");
                var token = config.GetValue<string>("config_Haravan:private_token");

                string url = $"https://apis.haravan.com/com/products.json?query=filter=(barcode:product={barcode})";
                HttpClientApp client = new HttpClientApp(token);
                ResponseApiHaravan res = await client.Get_Request(url);
                JObject data = JObject.Parse(res.data);
                JArray ListProduct = (JArray)data["products"];
                if (ListProduct.Count == 0) return new ResponseData("ok", "0", "1");       
                else return new ResponseData("ok", "1", "1");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }

        public async Task<ResponseData> GETVarianFromBarCode(string barcode)
        {
            try
            {
                if (barcode == null || barcode == "")
                    return new ResponseData("err", "Không có sản phẩm nào có barcode này trên haravan 1", "");
                var token = config.GetValue<string>("config_Haravan:private_token");

                string url = $"https://apis.haravan.com/com/products.json?query=filter=(barcode:product={barcode})";
                HttpClientApp client = new HttpClientApp(token);
                ResponseApiHaravan res = await client.Get_Request(url);
                JObject data = JObject.Parse(res.data);
                JArray ListProduct = (JArray)data["products"];
                if (ListProduct.Count == 0) return new ResponseData("err", $" Không có sản phẩm nào có barcode {barcode} này trên haravan 2", "1");
                if (ListProduct.Count > 1) return new ResponseData("err", "Đang có 2 sản phẩm có chung 1 mã barcode trên haravan", "");
                var product = (JObject)ListProduct[0];
                JArray arrVarian = (JArray)product["variants"];
                for (int i = 0; i < arrVarian.Count; i++)
                {
                    var varian = (JObject)arrVarian[i];
                    string _barcode = (string)varian["barcode"] ?? "";
                    if (barcode == _barcode)
                    {
                        string id_p = (string)product["id"];
                        Object temp = new
                        {
                            product_variant_id = (string)varian["id"] ?? "",
                            product_id = id_p,
                            not_allow_promotion = (bool)product["not_allow_promotion"],
                            price = (double)varian["price"],
                        };
                        return new ResponseData("ok", "", temp);
                    }
                }
                return new ResponseData("err", $"Không có sản phẩm nào có barcode {barcode} này trên haravan 3", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }

        public async void UpdateStore24(){
            var constring = config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            conn.Open();
            try
            {

                string locationId = config.GetValue<string>("config_Haravan:store024");
                
                string sqlgetitem = $"SELECT  ma_vt,ton13  FROM cdvt213  where ma_kho ='TP_CH024' ";
                

                SqlDataAdapter da = new SqlDataAdapter(sqlgetitem, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                List<string> lst_str = new List<string>();
                string stritems = "";
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    if (stritems.Length <= 8000)
                        stritems += $"{r["ma_vt"].ToString().Trim()},";
                    else
                    {
                        stritems += "' '";
                        lst_str.Add(stritems);
                        stritems = $"{r["ma_vt"].ToString().Trim()},";
                    }
                }
                stritems += "' '";
                lst_str.Add(stritems);
                var token = config.GetValue<string>("config_Haravan:private_token");
                string url = "";

                HttpClientApp client = new HttpClientApp(token);
                List<Res_GETVarian> lst_varian = new List<Res_GETVarian>();
                foreach (string str in lst_str)
                {
                    url = $"https://apis.haravan.com/com/products.json?query=filter=(barcode:product in {str})";

                    ResponseApiHaravan res = await client.Get_Request(url);
                    JObject root_products = JObject.Parse(res.data);
                    JArray ListProduct = (JArray)root_products["products"];
                    for (int i = 0; i < ListProduct.Count; i++)
                    {
                        var product = (JObject)ListProduct[i];
                        string id_p = (string)product["id"];
                        JArray arrVarian = (JArray)product["variants"];
                        for (int j = 0; j < arrVarian.Count; j++)
                        {
                            var varian = (JObject)arrVarian[j];
                            Res_GETVarian temp_v = new Res_GETVarian();
                            temp_v.ma_vt = (string)varian["barcode"] ?? "";

                            temp_v.product_variant_id = (string)varian["id"];
                            temp_v.product_id = id_p;
                            lst_varian.Add(temp_v);
                        }
                    }
                }



                List<List<Store_UpdateLineItem>> lineItems = new List<List<Store_UpdateLineItem>>();
                List<Store_UpdateLineItem> temp_line = new List<Store_UpdateLineItem>();
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    string barcode = r["ma_vt"].ToString().Trim();
                    decimal quantity = (decimal)r["ton13"];
                    for (int j = 0; j < lst_varian.Count; j++)
                    {
                        if (barcode.Trim() == lst_varian[j].ma_vt.Trim())
                        {
                            if (temp_line.Count < 500)
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
                foreach (List<Store_UpdateLineItem> line in lineItems)
                {
                    url = $"https://apis.haravan.com/com/inventories/adjustorset.json";
                    ResponseApiHaravan ress = new ResponseApiHaravan("ok", "", "");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        Object body = new
                        {
                            inventory = new
                            {
                                location_id = locationId,
                                type = "set",
                                reason = "shrinkage",
                                note = "Update by SSE",
                                line_items = line
                            }
                        };
                        JObject temp = JObject.FromObject(body);

                        ress = await client.Post_Request_WithBody(url, temp.ToString());
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

    }
}
