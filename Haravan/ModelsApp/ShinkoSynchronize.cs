using Haravan.FuncLib;
using Haravan.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;



namespace Haravan.ModelsApp
{
    public class ShinkoSynchronize
    {
        public async static Task<String> InsertToWoocomerce()
        {
            try
            {
                string host = MyAppData.config.GetValue<string>("WP:url");
                string url = $"{host}/wp-json/wc/v3/products";
                string sql = $"exec shinko_AddProduct ";
                string sql_re = "";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp(token);

                List<WP_Product_att> lst_att = new List<WP_Product_att>();
                List<WP_ProductShinko> body_create = new List<WP_ProductShinko>();
                string res="";
                //sku = ma_vt
                foreach (DataRow r in ds.Tables[0].Rows)
                {

                    WP_ProductShinko p = new WP_ProductShinko();
                    p.name = r["ten_vt"].ToString().Trim();
                    p.sku = r["ma_vt"].ToString().Trim(); ;
                    p.regular_price = r["gia_nt2"].ToString().Trim();
                    body_create.Add(p);
                    Object body_product_temp = JObject.FromObject(p);
                    ResponseApiHaravan res_product = await client.Post_Request_WithBody(url, body_product_temp.ToString());
                    res = res_product.message;
                    Console.WriteLine("Added");
                }
                return res;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}


