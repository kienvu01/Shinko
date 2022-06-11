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

namespace Haravan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WP_Product : ControllerBase
    {
        private readonly IConfiguration _config;


        public WP_Product(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        [Route("ShinkoProductDeleteAll")]
        public async Task<IActionResult> DeleteShinkoProduct(int start , int end)
        {
            try
            {

                string host = _config.GetValue<string>("WP:url");
                string url = $"{host}/wp-json/wc/v3/products/batch";
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                client.SetAuthorizationBasic(token);
                int count = 0;
                string response = start.ToString();

                List<int> id = new List<int>();
                for (int i = start; i < end+1; i++)
                {
                    count++;
                    id.Add(i);
                    if(count > 99||i==end)
                    {
                        List_WP_ProductShinkoDelete deleteId = new List_WP_ProductShinkoDelete()
                            {
                             delete = id
                            };
                        
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        string jsonString = JsonSerializer.Serialize(deleteId, options);
                        ResponseApiHaravan res = await client.Post_Request_WithBody(url, jsonString.ToString());
                        id = new List<int>();
                        count =0;
                        response = res.data;
                    }
                }
                
                return StatusCode(200, response);
            }
            catch (Exception e)
            {
                ApiLog.log("creat_product_wp2", e, "", "", e.Message);
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.ToString());
            }
        }





            [HttpPost]
        [Route("CreateShinkoProduct")]
        public async Task<IActionResult> CreateShinkoProduct()
        {
            try
            {
                string sql = $"exec shinko_AddProduct";
                string sql_re = "";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0) return StatusCode(200, new { status = "err", message = $"No Data" });
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                string host = _config.GetValue<string>("WP:url");
                string url = $"";
                client.SetAuthorizationBasic(token);
                string response = "Added:";
                string failed="Failed:";
                url = $"{host}/wp-json/wc/v3/products";

                string sqlColor = $"select isnull(xcolor,'')as xcolor from dmvt group by xcolor";
                DataSet dsColor = DAL.DAL_SQL.ExecuteGetlistDataset(sqlColor);
                List<string> listColor = new List<string>();
                foreach(DataRow row in dsColor.Tables[0].Rows)
                {
                    listColor.Add(row["xcolor"].ToString());
                }

                string sqlSize = $"select isnull(xsize,'')as xsize from dmvt group by xsize";
                DataSet dsSize = DAL.DAL_SQL.ExecuteGetlistDataset(sqlSize);
                List<string> listSize = new List<string>();
                foreach (DataRow row in dsSize.Tables[0].Rows)
                {
                    listSize.Add(row["xsize"].ToString());
                }

                atribute1 color = new atribute1()
                {
                    id = 1,
                    options = listColor,
                    visiable = true,
                    variation = true,
                    name = "Color",
                    position = 1,


                };
                atribute1 size = new atribute1()
                {
                    id = 2,
                    options = listSize,
                    visiable = true,
                    variation = true,
                    name = "Size",
                    position = 0,
                };
                List<atribute1> listAtt = new List<atribute1>();

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    response += dr["wp_id"].ToString();


                    if (Int32.Parse(dr["wp_id"].ToString()) == 0)
                    {
                        string ma_vt2 = dr["ma_vt"].ToString();
                        List<WP_ProductShinko> body_create = new List<WP_ProductShinko>();
                        //url = $"{host}/wp-json/wc/v3/products";

                        WP_ProductShinko p = new WP_ProductShinko();
                        WP_CateId cat = new WP_CateId();
                        // cat.id = Convert.ToInt32(Convert.ToDecimal(dr["cat_id"].ToString()));
                        cat.id = 0;
                        List<WP_CateId> listCat = new List<WP_CateId>();
                        listCat.Add(cat);
                        int z = Convert.ToInt32(Convert.ToDecimal(dr["ton13"].ToString()));
                        p.stock_quantity = z;
                        p.name = dr["ten_vt"].ToString().Trim();
                        p.sku = dr["ma_vt2"].ToString().Trim();
                        p.regular_price = dr["gia_nt2"].ToString().Trim();

                        p.manage_stock = true;
                        p.categories = listCat;

                        listAtt.Add(color);
                        listAtt.Add(size);
                        p.attributes = listAtt;
    
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        string jsonString = JsonSerializer.Serialize(p, options);
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
                           
                            failed += $"\n {p.name} {res.data}";
                        }

                        //body_create.Add(p);
                        //Object body_product_temp = JObject.FromObject(p);
                        //ResponseApiHaravan res = await client.Post_Request_WithBody(url, body_product_temp.ToString());
                        //if (res.status == "ok")
                        //{
                        //    JObject root_total = JObject.Parse(res.data);
                        //    wp_product = root_total["id"].ToString();

                        //    sql_re = Environment.NewLine + $"update dmvt set s9 ='{now}' where ma_vt ='{root_total["sku"].ToString()}'";
                        //    DAL.DAL_SQL.ExecuteNonquery(sql_re);
                        //    Console.WriteLine($"Addded {root_total["sku"]}");


                        //}
                        //else
                        //{
                        //    Console.WriteLine("faile add{0}", dr["ten_vt"].ToString().Trim());
                        //}
                    }

                }


                return StatusCode(200, response + "\n"+ failed );
            }
            catch (Exception e)
            {
                ApiLog.log("creat_product_wp2", e, "", "", e.Message);
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.ToString());
            }
        }


        [HttpPost]
        [Route("AlterShinkoCategory")]
        public async Task<IActionResult> AlterShinkoCategory(string ma_vt)
        {
            string sql = $"exec shinko_alter_product";
            string sql_re = "";
            DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
            if (ds.Tables[0].Rows.Count == 0) return StatusCode(200, new { status = "err", message = $"No Data" });
            string token = Library.GetTokenWWP();
            HttpClientApp client = new HttpClientApp();
            string host = _config.GetValue<string>("WP:url");
            string url = $"";
            client.SetAuthorizationBasic(token);
            DataRow target = ds.Tables[0].Rows[0];
            if (target["wp_id"] == "0")
            {
                url = $"{host}/wp-json/wc/v3/products";
                WP_ProductShinko p = new WP_ProductShinko();
                WP_CateId cat = new WP_CateId();
                cat.id = Convert.ToInt32(Convert.ToDecimal(target["cat_id"].ToString()));
                List<WP_CateId> listCat = new List<WP_CateId>();
                listCat.Add(cat);
                int z = Convert.ToInt32(Convert.ToDecimal(target["ton13"].ToString()));
                p.stock_quantity = z;
                p.name = target["ten_vt"].ToString().Trim();
                p.sku = target["ma_vt2"].ToString().Trim();
                p.regular_price = target["gia_nt2"].ToString().Trim();

                p.manage_stock = true;
                p.categories = listCat;

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(p, options);
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, jsonString.ToString());
                string response = "";
                if (res.status == "ok")
                {
                    JObject root_total = JObject.Parse(res.data);
                    sql_re = Environment.NewLine + $"update dmvt set wp_id ='{root_total["id"].ToString()}' where ma_vt2 ='{target["ma_vt2"].ToString().Trim()}'";
                    DAL.DAL_SQL.ExecuteNonquery(sql_re);
                    response += $"\n {p.name}-{root_total["id"].ToString()} ";

                }
                else
                {
                    response += $"\n {p.name}";
                }
                return StatusCode(200, response);
            }
            else 
            {
                url = $"{host}/wp-json/wc/v3/products/{target["wp_id"]}";
                WP_ProductShinko p = new WP_ProductShinko();
                WP_CateId cat = new WP_CateId();
                cat.id = Convert.ToInt32(Convert.ToDecimal(target["cat_id"].ToString()));
                List<WP_CateId> listCat = new List<WP_CateId>();
                listCat.Add(cat);
                int z = Convert.ToInt32(Convert.ToDecimal(target["ton13"].ToString()));
                p.stock_quantity = z;
                p.name = target["ten_vt"].ToString().Trim();
                p.sku = target["ma_vt2"].ToString().Trim();
                p.regular_price = target["gia_nt2"].ToString().Trim();

                p.manage_stock = true;
                p.categories = listCat;

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(p, options);
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, jsonString.ToString());
                string response = "";
                if (res.status == "ok")
                {
       
                    response += $"suceeded: \n {p.name} ";

                }
                else
                {
                    response += $"failed: \n {p.name}";
                }
                return StatusCode(200, response);
            }    
        }


        [HttpPost]
        [Route("DeleteShinkoCategory")]
        public async Task<IActionResult> DeleteShinkoCategory(int start, int end)
        {


            try
            {
                string sql = $"exec shinko_listCate";
                string sql_re = "";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0) return StatusCode(200, new { status = "err", message = $"No Data" });
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                string host = _config.GetValue<string>("WP:url");
                string url = $"";
                client.SetAuthorizationBasic(token);
                string response = "Deleted:";
                string failed = "Failed:";
                

                for(int i = start;i<end+1; i++)
                {
                    url = $"{host}/wp-json/wc/v3/products/categories/{i}?force=true";
                    ResponseApiHaravan res = await client.Del_Request(url);
                    if (res.status == "ok")
                    {
                        JObject root_total = JObject.Parse(res.data);
                        response += $"\n {i}  ";

                    }
                    else
                    {
                        failed += $"\n {i} {res.data}";
                    }
                }


                return StatusCode(200, response + "\n" + failed);
            }
            catch (Exception e)
            {
                ApiLog.log("creat_product_wp2", e, "", "", e.Message);
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.ToString());
            }
        }
        [HttpPost]
        [Route("CreateShinkoCategory")]
        public async Task<IActionResult> CreateShinkoCategory()
        {
            try
            {
                string sql = $"exec shinko_listCate";
                string sql_re = "";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0) return StatusCode(200, new { status = "err", message = $"No Data" });
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                string host = _config.GetValue<string>("WP:url");
                string url = $"";
                client.SetAuthorizationBasic(token);
                string response = "Added:";
                string failed = "Failed:";
                url = $"{host}/wp-json/wc/v3/products/categories";

                foreach (DataRow dr in ds.Tables[0].Rows)
                {

                    ///đồng bộ tầng 1
                    
                    if (Int32.Parse(dr["wp_cateId"].ToString()) == 0)
                    {
                        //url = $"{host}/wp-json/wc/v3/products";
                        if (dr["ma_nh"].ToString().Trim().Length == 2)
                        {
                            WP_CategoryShinko c = new WP_CategoryShinko();
                            c.name = dr["ten_nh"].ToString().Trim();


                            var options = new JsonSerializerOptions { WriteIndented = true };
                            string jsonString = JsonSerializer.Serialize(c, options);
                            ResponseApiHaravan res = await client.Post_Request_WithBody(url, jsonString.ToString());
                            response += $"\n";
                            if (res.status == "ok")
                            {
                                JObject root_total = JObject.Parse(res.data);
                                foreach (DataRow dro in ds.Tables[0].Rows)
                                {
                                    if(dro["ma_nh"].ToString().Trim().Length == 3 && dro["ma_nh"].ToString().Substring(0, 1)== dr["ma_nh"].ToString().Substring(0, 1))
                                    {

                                        sql_re = Environment.NewLine + $"update dmnhvt set wp_cateId ='{root_total["id"].ToString()}' where ma_nh  ='{dro["ma_nh"].ToString().Trim()}'";
                                        DAL.DAL_SQL.ExecuteNonquery(sql_re);
                                        response += $"\n  {dro["ten_nh"].ToString()}";
                                    }

                                }

                            }
                            else
                            {
                                failed += $"\n {c.name}";
                            }
                        }

                    }

                }


                return StatusCode(200, response + "\n" + failed);
            }
            catch (Exception e)
            {
                ApiLog.log("creat_product_wp2", e, "", "", e.Message);
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.ToString());
            }
        }


        [HttpPost]
        [Route("CreateShinkoCategorylv2")]
        public async Task<IActionResult> CreateShinkoCategorylv2()
        {
            try
            {
                string sql = $"exec shinko_listCate";
                string sql_re = "";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0) return StatusCode(200, new { status = "err", message = $"No Data" });
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                string host = _config.GetValue<string>("WP:url");
                string url = $"";
                client.SetAuthorizationBasic(token);
                string response = "Added:";
                string failed = "Failed:";
                url = $"{host}/wp-json/wc/v3/products/categories";

                foreach (DataRow dr in ds.Tables[0].Rows)
                {

                    ///đồng bộ tầng 2

                    if (Int32.Parse(dr["wp_cateId"].ToString()) != 0)
                    {
                        //url = $"{host}/wp-json/wc/v3/products";
                        if (dr["ma_nh"].ToString().Trim().Length == 3)
                        {
                            WP_CategoryShinkoChild c = new WP_CategoryShinkoChild();
                            c.name = dr["ten_nh"].ToString().Trim();
                            c.parent = Int32.Parse(dr["wp_cateId"].ToString().Trim());

                            var options = new JsonSerializerOptions { WriteIndented = true };
                            string jsonString = JsonSerializer.Serialize(c, options);
                            ResponseApiHaravan res = await client.Post_Request_WithBody(url, jsonString.ToString());
                            response += $"\n {c.name}-{dr["ma_nh"].ToString().Trim()}";
                            if (res.status == "ok")
                            {
                                JObject root_total = JObject.Parse(res.data);
                                foreach (DataRow dro in ds.Tables[0].Rows)
                                {
                                    if (dro["ma_nh"].ToString().Trim().Length > 3 && dro["ma_nh"].ToString().Substring(0, 3) == dr["ma_nh"].ToString().Trim())
                                    {
                                        sql_re = Environment.NewLine + $"update dmnhvt set wp_cateId ={root_total["id"].ToString()} where ma_nh  ='{dro["ma_nh"].ToString().Trim()}'";
                                        DAL.DAL_SQL.ExecuteNonquery(sql_re);
                                        response += $"\n {dro["ma_nh"].ToString()}-{dr["ma_nh"].ToString().Trim()} -{root_total["id"].ToString()}";
                                    }
                                      

                                }

                            }
                            else
                            {
                                failed += $"\n {c.name}";
                            }
                        }

                    }

                }


                return StatusCode(200, response + "\n" + failed);
            }
            catch (Exception e)
            {
                ApiLog.log("creat_product_wp2", e, "", "", e.Message);
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.ToString());
            }
        }


        [HttpPost]
        [Route("CreateShinkoCategorylv3")]
        public async Task<IActionResult> CreateShinkoCategorylv3()
        {
            try
            {
                string sql = $"exec shinko_listCate";
                string sql_re = "";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0) return StatusCode(200, new { status = "err", message = $"No Data" });
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                string host = _config.GetValue<string>("WP:url");
                string url = $"";
                client.SetAuthorizationBasic(token);
                string response = "Added:";
                string failed = "Failed:";
                url = $"{host}/wp-json/wc/v3/products/categories";

                foreach (DataRow dr in ds.Tables[0].Rows)
                {

                    ///đồng bộ tầng 1

                    if (Int32.Parse(dr["wp_cateId"].ToString()) != 0)
                    {
                        //url = $"{host}/wp-json/wc/v3/products";
                        if (dr["ma_nh"].ToString().Trim().Length > 3)
                        {
                            WP_CategoryShinkoChild c = new WP_CategoryShinkoChild();
                            c.name = dr["ten_nh"].ToString().Trim();
                            c.parent = Int32.Parse(dr["wp_cateId"].ToString().Trim());

                            var options = new JsonSerializerOptions { WriteIndented = true };
                            string jsonString = JsonSerializer.Serialize(c, options);
                            ResponseApiHaravan res = await client.Post_Request_WithBody(url, jsonString.ToString());
                            if (res.status == "ok")
                            {
                                JObject root_total = JObject.Parse(res.data);
                                response += $"\n {c.name} {sql_re}";

                                sql_re = Environment.NewLine + $"update dmvt set cat_id ={root_total["id"].ToString()} where nh_vt4  ='{dr["ma_nh"].ToString().Trim()}'";
                                DAL.DAL_SQL.ExecuteNonquery(sql_re);


                            }
                            else
                            {
                                failed += $"\n {c.name}+{res.data}";
                            }
                        }

                    }

                }


                return StatusCode(200, response + "\n" + failed);
            }
            catch (Exception e)
            {
                ApiLog.log("creat_product_wp2", e, "", "", e.Message);
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.ToString());
            }
        }


        [HttpPost]
        [Route("CreateShinkoVariant")]
        public async Task<IActionResult> CreateShinkoVariant()
        {
            try
            {
                string sql = $"exec shinko_AddVariant";
                string sql_re = "";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0) return StatusCode(200, new { status = "err", message = $"No Data" });
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                string host = _config.GetValue<string>("WP:url");
                string url = $"";
                client.SetAuthorizationBasic(token);
                string response = "Added:";
                string failed = "Failed:";
                

                foreach (DataRow dr in ds.Tables[0].Rows)
                {

                    if (dr["wp_id"].ToString() != ""&& dr["variations_id"].ToString() == "0")
                    {

                        // if (dr["s9"].ToString() != "1/1/1900 12:00:00 AM")
                        url = $"{host}/wp-json/wc/v3/products/{dr["wp_id"].ToString()}/variations";
                        DateTime now = DateTime.Now;

                        //url = $"{host}/wp-json/wc/v3/products";
                        
                        
                        WP_VariantShinko p = new WP_VariantShinko();
                        int z = Convert.ToInt32(Convert.ToDecimal(dr["ton13"].ToString()));
                        p.stock_quantity = z;
                        p.sku = dr["ma_vt"].ToString().Trim();
                        p.regular_price = dr["gia_nt2"].ToString().Trim();
                        p.manage_stock = true;
                        p.description = dr["ten_vt"].ToString().Trim();
                        WP_Variant_att x = new WP_Variant_att();
                        x.option = dr["xcolor"].ToString().Trim();
                        x.id = 1;
                        WP_Variant_att size = new WP_Variant_att();
                        size.option = dr["xsize"].ToString().Trim();
                        size.id = 2;
                        List<WP_Variant_att> listAtt = new List<WP_Variant_att>();
                        listAtt.Add(x);
                        p.attributes = listAtt;



                        JObject body_temp2 = JObject.FromObject(p);
                        ResponseApiHaravan res = await client.Post_Request_WithBody(url, body_temp2.ToString());
                        if (res.status == "ok")
                        {
                            JObject root_total = JObject.Parse(res.data);
                            sql_re = Environment.NewLine + $"update dmvt set variations_id ='{root_total["id"].ToString()}' where ma_vt ='{dr["ma_vt"].ToString().Trim()}'";
                            DAL.DAL_SQL.ExecuteNonquery(sql_re);
                            response += body_temp2.ToString().Trim();

                        }
                        else
                        {
                            failed += $"\n {p.sku} {res.data}";
                        }

                        //body_create.Add(p);
                        //Object body_product_temp = JObject.FromObject(p);
                        //ResponseApiHaravan res = await client.Post_Request_WithBody(url, body_product_temp.ToString());
                        //if (res.status == "ok")
                        //{
                        //    JObject root_total = JObject.Parse(res.data);
                        //    wp_product = root_total["id"].ToString();

                        //    sql_re = Environment.NewLine + $"update dmvt set s9 ='{now}' where ma_vt ='{root_total["sku"].ToString()}'";
                        //    DAL.DAL_SQL.ExecuteNonquery(sql_re);
                        //    Console.WriteLine($"Addded {root_total["sku"]}");


                        //}
                        //else
                        //{
                        //    Console.WriteLine("faile add{0}", dr["ten_vt"].ToString().Trim());
                        //}


                    }
                }

                return StatusCode(200, url+ "\n"+response + "\n" + failed);
            }
            catch (Exception e)
            {
                ApiLog.log("creat_product_wp2", e, "", "", e.Message);
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.ToString());
            }
        }


        //Lấy ds sản phẩm từ haravan
        [HttpPost]
        [Route("CreateProduct")]
        public async Task<IActionResult> CreateProduct(string ma_vt)
        {
            bool check = Library.CheckAuthentication(_config, this);
            ApiLog.log("test", "", "", "", ma_vt);
            //if (!check) return StatusCode(401);
            try
            {
                string sql = $"exec haravan_GetItemToWP '{ma_vt}'";
                string sql_re = "";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0) return StatusCode(200, new { status = "err", message = $"Sản phẩm không tồn tại" });
                if (ds.Tables[0].Rows[0]["wp_product"].ToString().Trim() != "" && ds.Tables[0].Rows[0]["wp_variant"].ToString().Trim() != "") return StatusCode(200, new { status = "err", message = $"Sản phẩm đã được đồng bộ" });

                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                client.SetAuthorizationBasic(token);
                string host = _config.GetValue<string>("WP:url");
                string url = $"";

                DataRow vt = ds.Tables[0].Rows[0];
                string ma_vt2 = vt["ma_vt2"].ToString();
                string wp_product = "";
                string wp_variant = "";
                decimal stock_p = 0;

                if (ds.Tables[1].Rows.Count > 0)
                {
                    wp_product = ds.Tables[1].Rows[0]["wp_product"].ToString();
                    wp_variant = ds.Tables[1].Rows[0]["wp_variant"].ToString();
                    stock_p = Library.ConvertToDecimal(ds.Tables[1].Rows[0]["ton13"].ToString());
                }
                if (wp_product != "")
                {
                    List<WP_Product_att> list_att = new List<WP_Product_att>();
                    WP_Product_att temp = new WP_Product_att();
                    int? id_att = null;
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        DataRow r = ds.Tables[2].Rows[i];
                        if (id_att != null && id_att != Convert.ToInt32(r["wp_code"].ToString()))
                        {
                            list_att.Add(temp);
                            temp = new WP_Product_att();
                        }
                        id_att = Convert.ToInt32(r["wp_code"].ToString());
                        temp.id = Convert.ToInt32(r["wp_code"].ToString());
                        temp.options.Add(r["ten_nh"].ToString());
                    }
                    list_att.Add(temp);
                    url = $"{host}/wp-json/wc/v3/products";

                    Object body = new
                    {

                        name = vt["ten_vt2"],
                        type = "variable",
                        status = "publish",
                        sku = ma_vt2,
                        price = 2500000,
                        manage_stock = true,
                        stock_quantity = stock_p,
                        attributes = list_att
                    };
                    JObject body_temp = JObject.FromObject(body);
                    ApiLog.log("creat_product_wp", body_temp, "", "", ma_vt);

                    ResponseApiHaravan res = await client.Post_Request_WithBody(url, body_temp.ToString());
                    ApiLog.log("creat_product_wp2", res, "", "", ma_vt);
                    if (res.status == "ok")
                    {
                        JObject root_total = JObject.Parse(res.data);
                        wp_product = root_total["id"].ToString();
                        sql_re = Environment.NewLine + $"update dmvt set wp_product ='{wp_product}' where ma_vt2 ='{root_total["sku"].ToString()}'";
                        DAL.DAL_SQL.ExecuteNonquery(sql_re);
                    }
                    else
                    {
                        return StatusCode(200, new { status = "err", message = res.data });
                    }
                }
                List<WP_Variant_att> list_var_att = new List<WP_Variant_att>();
                foreach (DataRow r in ds.Tables[3].Rows)
                {
                    WP_Variant_att temp = new WP_Variant_att();
                    temp.option = r["ten_nh"].ToString();
                    temp.id = Convert.ToInt32(r["wp_code"].ToString());
                    list_var_att.Add(temp);
                }
                url = $"{host}/wp-json/wc/v3/products/{wp_product.Trim()}/variations";
                decimal price = 0;
                decimal stock = 0;
                stock = Library.ConvertToDecimal(ds.Tables[0].Rows[0]["ton13"].ToString());
                if (ds.Tables[4].Rows.Count > 0) price = Library.ConvertToDecimal(ds.Tables[4].Rows[0]["gia_nt2"].ToString());
                Object body_var = new
                {
                    description = vt["ten_vt"].ToString(),
                    status = "publish",
                    sku = ma_vt.Trim(),
                    regular_price = price.ToString(),
                    manage_stock = true,
                    stock_quantity = stock,
                    attributes = list_var_att
                };
                JObject body_temp2 = JObject.FromObject(body_var);
                ApiLog.log("creat_product_variant_wp", body_temp2, "", "", ma_vt);
                ResponseApiHaravan res2 = await client.Post_Request_WithBody(url, body_temp2.ToString());
                ApiLog.log("creat_product_variant_wp2", res2, "", "", ma_vt);
                if (res2.status == "ok")
                {
                    JObject root_total = JObject.Parse(res2.data);
                    wp_variant = root_total["id"].ToString();
                    sql_re = Environment.NewLine + $"update dmvt set wp_product ='{wp_product}' , wp_variant ='{wp_variant}'  where ma_vt ='{root_total["sku"].ToString()}'";
                    DAL.DAL_SQL.ExecuteNonquery(sql_re);
                    return StatusCode(200, new { status = "ok", message = "Thêm thành công" });
                }
                else
                {
                    return StatusCode(200, new { status = "err", message = res2.data });
                }
            }
            catch (Exception e)
            {
                ApiLog.log("creat_product_wp2", e, "", "", e.Message);
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        [Route("CreateAllProduct")]
        public async Task<IActionResult> CreateAllProduct()
        {
            bool check = Library.CheckAuthentication(_config, this);
            if (!check) return StatusCode(401);
            ResponseData res = await ModelsApp.WP_Product.CreateAllProduct();
            return StatusCode(200, res);
           
        }

        [HttpPost]
        [Route("CreateNewCustomer")]
        public async Task<IActionResult> CreateNewCustomer(string ten_kh, string sdt, string dia_chi)
        {
            string sql = $"select * from customer";
            string sql_re = "";
            DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
            
            string token = Library.GetTokenWWP();
            HttpClientApp client = new HttpClientApp();
            string host = _config.GetValue<string>("WP:url");
            string url = $"";
            client.SetAuthorizationBasic(token);
            string response = "Added:";
            string failed = "Failed:";
            bool exist = false;
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                if (r["ma_kh"].ToString() == sdt) exist = false; 
            }
            if(exist)
            {
               sql = $"insert into table dmkh(ma_kh,ten_kh,dien_thoai,dia_chi) VALUES({sdt},{ten_kh},{sdt},{dia_chi});";
               DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                return StatusCode(200, "added succeded");
            }
                return StatusCode(500, "customer existed");
            
        }
        [HttpPost]
        [Route("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(Req_UpdateWP data)
        {
            bool check = Library.CheckAuthentication(_config, this);
            try
            {
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                client.SetAuthorizationBasic(token);

                foreach (Req_updateWP_product item in data.items)
                {
                    string ma_vt = item.ma_vt.Trim();
                    string sql = $"exec haravan_GetItemToWP '{ma_vt}'";
                    string sql_re = "";
                    DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                    if (ds.Tables[0].Rows.Count == 0) continue;
                    if (ds.Tables[0].Rows[0]["wp_variant"].ToString().Trim() == "") continue;

                    string host = _config.GetValue<string>("WP:url");
                    string url = $"";
                    string wp_product = "";
                    string wp_variant = "";
                    wp_product = ds.Tables[0].Rows[0]["wp_product"].ToString();
                    wp_variant = ds.Tables[0].Rows[0]["wp_variant"].ToString();

                    List<WP_Variant_att> list_var_att = new List<WP_Variant_att>();
                    foreach (DataRow r in ds.Tables[3].Rows)
                    {
                        WP_Variant_att temp = new WP_Variant_att();
                        temp.option = r["ten_nh"].ToString();
                        temp.id = Convert.ToInt32(r["wp_code"].ToString());
                        list_var_att.Add(temp);
                    }
                    url = $"{host}/wp-json/wc/v3/products/{wp_product.Trim()}/variations/{wp_variant.Trim()}";
                    decimal price = 0;
                    decimal stock = 0;
                    stock = Library.ConvertToDecimal(ds.Tables[0].Rows[0]["ton13"].ToString());
                    if (ds.Tables[4].Rows.Count > 0) price = Library.ConvertToDecimal(ds.Tables[4].Rows[0]["gia_nt2"].ToString());
                    Object body_var = new
                    {
                        description = ds.Tables[0].Rows[0]["ten_vt"].ToString(),

                        regular_price = price.ToString(),
                        stock_quantity = stock,
                        attributes = list_var_att
                    };
                    JObject body_temp2 = JObject.FromObject(body_var);

                    ResponseApiHaravan res2 = await client.Put_Request_WithBody(url, body_temp2.ToString());
                }
                return StatusCode(200, new { status = "ok", message = "Chỉnh sửa thành công" });



            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }

        }
        [HttpPost]
        [Route("UpdateGiaToWP")]
        public async Task<IActionResult> UpdateGiaToWP()
        {
            bool check = Library.CheckAuthentication(_config, this);
            try
            {
                string sql_getitem = $"exec haravan_GetAllItemToUpdateWP ";

                DataSet ds_getitem = DAL.DAL_SQL.ExecuteGetlistDataset(sql_getitem);
                Req_UpdateWP data = new Req_UpdateWP();
                data.items = new List<Req_updateWP_product>();
                foreach(DataRow r in ds_getitem.Tables[0].Rows)
                {
                    Req_updateWP_product temp = new Req_updateWP_product();
                    temp.ma_vt = r["ma_vt"].ToString();
                    temp.changeProduct = true;
                    data.items.Add(temp);
                }

                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                client.SetAuthorizationBasic(token);

                foreach (Req_updateWP_product item in data.items)
                {
                    string ma_vt = item.ma_vt.Trim();
                    string sql = $"exec haravan_GetItemToWP '{ma_vt}'";
                    string sql_re = "";
                    DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                    if (ds.Tables[0].Rows.Count == 0) continue;
                    if (ds.Tables[0].Rows[0]["wp_variant"].ToString().Trim() == "") continue;

                    string host = _config.GetValue<string>("WP:url");
                    string url = $"";
                    string wp_product = "";
                    string wp_variant = "";
                    wp_product = ds.Tables[0].Rows[0]["wp_product"].ToString();
                    wp_variant = ds.Tables[0].Rows[0]["wp_variant"].ToString();

                    List<WP_Variant_att> list_var_att = new List<WP_Variant_att>();
                    foreach (DataRow r in ds.Tables[3].Rows)
                    {
                        WP_Variant_att temp = new WP_Variant_att();
                        temp.option = r["ten_nh"].ToString();
                        temp.id = Convert.ToInt32(r["wp_code"].ToString());
                        list_var_att.Add(temp);
                    }
                    url = $"{host}/wp-json/wc/v3/products/{wp_product.Trim()}/variations/{wp_variant.Trim()}";
                    decimal price = 0;
                    decimal stock = 0;
                    stock = Library.ConvertToDecimal(ds.Tables[0].Rows[0]["ton13"].ToString());
                    if (ds.Tables[4].Rows.Count > 0) price = Library.ConvertToDecimal(ds.Tables[4].Rows[0]["gia_nt2"].ToString());
                    Object body_var = new
                    {
                        description = ds.Tables[0].Rows[0]["ten_vt"].ToString(),

                        regular_price = price.ToString(),
                        stock_quantity = stock,
                        attributes = list_var_att
                    };
                    JObject body_temp2 = JObject.FromObject(body_var);

                    ResponseApiHaravan res2 = await client.Put_Request_WithBody(url, body_temp2.ToString());
                }
                return StatusCode(200, new { status = "ok", message = "Chỉnh sửa thành công" });



            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }

        }

        [HttpPost]
        [Route("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(string ma_vt)
        {
            bool check = Library.CheckAuthentication(_config, this);
            try
            {
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                client.SetAuthorizationBasic(token);

                string sql = $"select * from dmvt where ma_vt = '{ma_vt}'";
                string sql_re = "";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0) return StatusCode(200, new { status = "err", message = $"Sản phẩm không tồn tại" });
                if (ds.Tables[0].Rows[0]["wp_product"].ToString().Trim() != "" && ds.Tables[0].Rows[0]["wp_variant"].ToString().Trim() != "") return StatusCode(200, new { status = "err", message = $"Sản phẩm đã được đồng bộ" });

                string host = _config.GetValue<string>("WP:url");
                string url = $"";
                string wp_product = "";
                string wp_variant = "";
                wp_product = ds.Tables[0].Rows[0]["wp_product"].ToString();
                wp_variant = ds.Tables[0].Rows[0]["wp_variant"].ToString();

                url = $"{host}/wp-json/wc/v3/products/{wp_product.Trim()}/variations/{wp_variant.Trim()}?force=true";

                Object body_var = new
                {
                };
                JObject body_temp2 = JObject.FromObject(body_var);

                ResponseApiHaravan res2 = await client.Del_Request(url);
                if (res2.status == "ok")
                {
                    return StatusCode(200, new { status = "ok", message = "Xóa thành công" });
                }
                else
                {
                    return StatusCode(200, new { status = "err", message = res2.data });
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
        [Route("UpdateAllProduct")]
        public async Task<IActionResult> UpdateAllProduct()
        {
            bool check = Library.CheckAuthentication(_config, this);
            //if (!check) return StatusCode(401);
            try
            {
                string sql = $"";

                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                client.SetAuthorizationBasic(token);
                string host = _config.GetValue<string>("WP:url");
                string url = $"";
                bool checkend = false;
                int page = 1;
                while (!checkend)
                {
                    url = $"{host}/wp-json/wc/v3/products?page={page}&per_page=100";

                    ResponseApiHaravan res = await client.Get_Request(url);
                    if (res.status == "ok")
                    {
                        JArray lst_creat = JArray.Parse(res.data);
                        foreach (JObject p in lst_creat)
                        {
                            string id_product = p["id"].ToString();
                            if (id_product == "3801")
                            {
                                int ttt = 0;
                            }
                            string p_barcode = p["sku"].ToString();
                            sql += Environment.NewLine + $"update dmvt set wp_product='{id_product}' where ma_vt2 ='{p_barcode.ToString()}'";
                            url = $"{host}/wp-json/wc/v3/products/{id_product}/variations?page=1&per_page=100";
                            ResponseApiHaravan res_variant = await client.Get_Request(url);
                            if (res_variant.status == "ok")
                            {
                                JArray lst_variant = JArray.Parse(res_variant.data);
                                foreach (JObject v in lst_variant)
                                {
                                    string ma_vt = v["sku"].ToString();
                                    string id_variant = v["id"].ToString();
                                    sql += Environment.NewLine + $"update dmvt set wp_product='{id_product}',wp_variant='{id_variant}' where ma_vt ='{ma_vt}'";
                                }
                            }
                        }
                        if (lst_creat.Count < 100) checkend = true;
                    }
                    else
                    {
                        checkend = true;
                    }
                    page++;

                }
                DAL.DAL_SQL.ExecuteNonquery(sql);
                return StatusCode(200, new { status = "ok", message = "Thêm thành công" });
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
        public async Task<IActionResult> Tes22t(string ma_vt)
        {
            try
            {
                string sql = $"";

                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                client.SetAuthorizationBasic(token);
                string host = _config.GetValue<string>("WP:url");
                string url = $"";
                bool checkend = false;
                int page = 1;
                while (!checkend)
                {
                    url = $"{host}/wp-json/wc/v3/products?page={page}&per_page=100";

                    ResponseApiHaravan res = await client.Get_Request(url);
                    if (res.status == "ok")
                    {
                        JArray lst_creat = JArray.Parse(res.data);
                        foreach (JObject p in lst_creat)
                        {
                            string id_product = p["id"].ToString();
                            if (id_product == "3801")
                            {
                                int ttt = 0;
                            }
                            string p_barcode = p["sku"].ToString();
                            sql += Environment.NewLine + $"update dmvt set wp_product='{id_product}' where ma_vt2 ='{p_barcode.ToString()}'";
                            url = $"{host}/wp-json/wc/v3/products/{id_product}/variations?page=1&per_page=100";
                            ResponseApiHaravan res_variant = await client.Get_Request(url);
                            if (res_variant.status == "ok")
                            {
                                JArray lst_variant = JArray.Parse(res_variant.data);
                                foreach (JObject v in lst_variant)
                                {
                                    string ma_vt_v = v["sku"].ToString();
                                    if (ma_vt_v.Trim() == ma_vt.Trim())
                                    {
                                        return Ok(v.ToString());
                                    }
                                
                                }
                            }
                        }
                        if (lst_creat.Count < 100) checkend = true;
                    }
                    else
                    {
                        checkend = true;
                    }
                    page++;

                }
                return StatusCode(200, new { status = "ok", message = "Thêm thành công" });
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }
    }
}
