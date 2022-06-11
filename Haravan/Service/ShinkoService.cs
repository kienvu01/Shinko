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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Haravan.Service
{
    public class ShinkoService : IShinkoService
    {
        private readonly IConfiguration _config;


        public ShinkoService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> CreateAllProducts()
        {
            
            string sql = $"exec shinko_AddProduct";
            string sql_re = "";
            DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
            if (ds.Tables[0].Rows.Count == 0) return "No Data";
            string token = Library.GetTokenWWP();
            HttpClientApp client = new HttpClientApp();
            string host = _config.GetValue<string>("WP:url");
            string url = $"";
            client.SetAuthorizationBasic(token);
            string response = "Added:";
            string failed = "Failed:";
            url = $"{host}/wp-json/wc/v3/products";

            foreach (DataRow dr in ds.Tables[0].Rows)
            {



                // if (dr["s9"].ToString() != "1/1/1900 12:00:00 AM")

                string ma_vt2 = dr["ma_vt"].ToString();
                List<WP_ProductShinko> body_create = new List<WP_ProductShinko>();




                //url = $"{host}/wp-json/wc/v3/products";

                WP_ProductShinko p = new WP_ProductShinko();
                WP_CateId cat = new WP_CateId();
                cat.id = Convert.ToInt32(Convert.ToDecimal(dr["cat_id"].ToString()));
                List<WP_CateId> listCat = new List<WP_CateId>();
                listCat.Add(cat);
                int z = Convert.ToInt32(Convert.ToDecimal(dr["ton13"].ToString()));
                p.stock_quantity = z;
                p.name = dr["ten_vt"].ToString().Trim();
                p.sku = dr["ma_vt2"].ToString().Trim();
                p.regular_price = dr["gia_nt2"].ToString().Trim();

                p.manage_stock = true;
                p.categories = listCat;
           
                var options = new JsonSerializerOptions { WriteIndented = true };
                object jsonString = jsonString = JObject.FromObject(p);
         
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, jsonString.ToString());
                if (res.status == "ok")
                {
                  
                    JObject root_total = JObject.Parse(res.data);
                    sql_re = Environment.NewLine + $"update dmvt set wp_id ='{root_total["id"].ToString()}' where ma_vt2 ='{dr["ma_vt2"].ToString().Trim()}'";
                    DAL.DAL_SQL.ExecuteNonquery(sql_re);
                    response += $"\n {p.name} ";
                  

                }
                else
                {
                    failed += $"\n {p.name}";
                }

            }
            return response + "\n" + failed;
        }

        public async Task<string> DeleteAllProducts(int start, int end)
        {
            string host = _config.GetValue<string>("WP:url");
            string url = $"{host}/wp-json/wc/v3/products/batch";
            string token = Library.GetTokenWWP();
            HttpClientApp client = new HttpClientApp();
            client.SetAuthorizationBasic(token);
            int count = 0;
            string response = start.ToString();

            List<int> id = new List<int>();
            for (int i = start; i < end + 1; i++)
            {
                count++;
                id.Add(i);
                if (count > 99 || i == end)
                {
                    List_WP_ProductShinkoDelete deleteId = new List_WP_ProductShinkoDelete()
                    {
                        delete = id
                    };

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(deleteId, options);
                    ResponseApiHaravan res = await client.Post_Request_WithBody(url, jsonString.ToString());
                    id = new List<int>();
                    count = 0;
                    response = jsonString;
                }
            }
            return response;
        }
    }
}

