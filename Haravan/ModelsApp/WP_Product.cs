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
    public class WP_Product
    {
        public async static Task<int> CreatBathProduct(List<WP_ProductBodyToWP> body_creat)
        {
            try
            {
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                client.SetAuthorizationBasic(token);
                string host = MyAppData.config.GetValue<string>("WP:url");
                string url = $"";

                string sql = "";

                foreach (WP_ProductBodyToWP product in body_creat)
                {
                    url = $"{host}/wp-json/wc/v3/products";

                    Object body_product = new
                    {

                        name = product.name,
                        type = "variable",
                        status = "publish",
                        sku = product.sku,
                        price = 2500000,
                        manage_stock = true,
                        stock_quantity = product.stock_quantity,
                        attributes = product.attributes
                    };
                    JObject body_product_temp = JObject.FromObject(body_product);

                    ResponseApiHaravan res_product = await client.Post_Request_WithBody(url, body_product_temp.ToString());
                    if (res_product.status == "ok")
                    {
                        JObject root_total = JObject.Parse(res_product.data);
  
                        sql  += Environment.NewLine + $"update dmvt set wp_product ='{root_total["id"].ToString()}' where ma_vt2 ='{root_total["sku"].ToString()}'";
                        string sql_variant = $"exec haravan_GetAllItemToWP '{root_total["sku"].ToString()}'";
                        DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql_variant);
                        List<WP_VariantBodyToWP> body_variant = new List<WP_VariantBodyToWP>();
                        foreach (DataRow r in ds.Tables[0].Rows)
                        {
                            WP_VariantBodyToWP variant = new WP_VariantBodyToWP();
                            variant.description = r["ten_vt"].ToString();
                            variant.sku = r["ma_vt"].ToString();
                            variant.regular_price = Library.ConvertToDecimal(r["gia"].ToString());
                            variant.stock_quantity = Library.ConvertToDecimal(r["ton13"].ToString());
                            List<WP_Variant_att> attributes = new List<WP_Variant_att>();
                            foreach (DataRow r2 in ds.Tables[1].Rows)
                            {
                                if (r2["ma_vt"].ToString().Trim() == variant.sku.Trim())
                                {
                                    WP_Variant_att t = new WP_Variant_att();
                                    t.id = Library.ConvertToInt(r2["wp_code"].ToString());
                                    t.option = r2["ten_nh"].ToString();
                                    attributes.Add(t);
                                }
                            }
                            variant.attributes = attributes;
                            body_variant.Add(variant);
                        }
                        url = $"{host}/wp-json/wc/v3/products/{root_total["id"].ToString()}/variations/batch";
                        Object body2 = new
                        {

                            create = body_variant
                        };
                        JObject body2_temp = JObject.FromObject(body2);

                        ResponseApiHaravan res_variant = await client.Post_Request_WithBody(url, body2_temp.ToString());
                        if (res_variant.status == "ok")
                        {
                            JObject root_creat_variant = JObject.Parse(res_variant.data);
                            JArray lst_variant_creat = (JArray)root_creat_variant["create"];
                            foreach (JObject v in lst_variant_creat)
                                if (v["id"] != null && v["sku"] != null) sql += Environment.NewLine + $"update dmvt set wp_variant ='{v["id"].ToString().Trim()}' where ma_vt ='{v["sku"].ToString().Trim()}'";
                        }
                    }
                }

                DAL.DAL_SQL.ExecuteNonquery(sql);
                return 1;
            }
            catch (Exception e)
            {
                return 1;
            }
        }
        public async static Task<int> CreatProductInMavt2(WP_ListProductHasItemNotToWP vt)
        {
            try
            {
                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp();
                client.SetAuthorizationBasic(token);
                string host = MyAppData.config.GetValue<string>("WP:url");
                string url = $"";


                string sql = "";
                string sql_variant = $"exec haravan_GetAllItemToWP '{vt.ma_vt.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql_variant);
                List<WP_VariantBodyToWP> body_variant = new List<WP_VariantBodyToWP>();
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    WP_VariantBodyToWP variant = new WP_VariantBodyToWP();
                    variant.description = r["ten_vt"].ToString();
                    variant.sku = r["ma_vt"].ToString();
                    variant.regular_price = Library.ConvertToDecimal(r["gia"].ToString());
                    variant.stock_quantity = Library.ConvertToDecimal(r["ton13"].ToString());
                    List<WP_Variant_att> attributes = new List<WP_Variant_att>();
                    foreach (DataRow r2 in ds.Tables[1].Rows)
                    {
                        if (r2["ma_vt"].ToString().Trim() == variant.sku.Trim())
                        {
                            WP_Variant_att t = new WP_Variant_att();
                            t.id = Library.ConvertToInt(r2["wp_code"].ToString());
                            t.option = r2["ten_nh"].ToString();
                            attributes.Add(t);
                        }
                    }
                    variant.attributes = attributes;
                    body_variant.Add(variant);
                }
                url = $"{host}/wp-json/wc/v3/products/{vt.wp_product.Trim()}/variations/batch";
                Object body2 = new
                {

                    create = body_variant
                };
                JObject body2_temp = JObject.FromObject(body2);

                ResponseApiHaravan res_variant = await client.Post_Request_WithBody(url, body2_temp.ToString());
                if (res_variant.status == "ok")
                {
                    JObject root_creat_variant = JObject.Parse(res_variant.data);
                    JArray lst_variant_creat = (JArray)root_creat_variant["create"];
                    foreach (JObject v in lst_variant_creat)
                        sql += Environment.NewLine + $"update dmvt set wp_variant ='{v["id"].ToString().Trim()}',wp_product='{vt.wp_product.Trim()}' where ma_vt ='{v["sku"].ToString().Trim()}'";
                }
                if (sql != "") DAL.DAL_SQL.ExecuteNonquery(sql);
                return 1;
            }catch(Exception e)
            {
                return 1;
            }
        }

        public async static Task<ResponseData> CreateAllProduct()
        {
            try
            {
                string sql = $"exec haravan_GetAllMasterItemToWP";
                string sql_re = "";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);

                string token = Library.GetTokenWWP();
                HttpClientApp client = new HttpClientApp(token);
                string host = MyAppData.config.GetValue<string>("WP:url");
                string url = $"";

                List<WP_Product_att> lst_att = new List<WP_Product_att>();
                WP_Product_att att = new WP_Product_att();

                string ma_tt = "";
                foreach (DataRow r in ds.Tables[1].Rows)
                {
                    if (ma_tt == "" || ma_tt == r["ma_tt"].ToString())
                    {
                        att.options.Add(r["ten_nh"].ToString());
                        att.id = Convert.ToInt32(r["wp_code"].ToString());  
                    }
                    else
                    {
                        lst_att.Add(att);
                        att = new WP_Product_att();
                    }
                    ma_tt = r["ma_tt"].ToString();
                }
                lst_att.Add(att);


                List<WP_ProductBodyToWP> body_creat = new List<WP_ProductBodyToWP>();
                List<WP_ListProductHasItemNotToWP> list_notwp = new List<WP_ListProductHasItemNotToWP>();
                int dem = 0;
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    if (r["wp_product"].ToString() != "") {
                        if (FuncLib.Library.ConvertToDecimal(r["total_record"].ToString()) > 0)
                        list_notwp.Add(new WP_ListProductHasItemNotToWP(r["ma_vt2"].ToString(), r["wp_product"].ToString()));
                    }
                    else
                    {
                        if (dem < 20)
                        {
                            WP_ProductBodyToWP p = new WP_ProductBodyToWP();
                            p.attributes = lst_att;
                            p.name = r["ten_vt2"].ToString().Trim();
                            p.sku = r["ma_vt2"].ToString().Trim();
                            p.price = 0;
                            p.stock_quantity = Library.ConvertToDecimal(r["ton13"].ToString());
                            body_creat.Add(p);
                            dem++;
                        }
                        else
                        {
                            await ModelsApp.WP_Product.CreatBathProduct(body_creat);
                            body_creat = new List<WP_ProductBodyToWP>();
                            dem = 0;
                        }
                    }
                }
                if (dem > 0) await WP_Product.CreatBathProduct(body_creat);
                if (list_notwp.Count >0)
                {
                    foreach (WP_ListProductHasItemNotToWP vt in list_notwp)
                        await CreatProductInMavt2(vt);
                }
                return new ResponseData("ok", "Thêm thành công", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message,"");
            }
        }
    }

}
