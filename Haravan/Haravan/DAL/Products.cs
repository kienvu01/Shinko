using Haravan.Controllers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Haravan.Haravan.Model;
using Haravan.Haravan.Common;
using Haravan.FuncLib;

namespace Haravan.Haravan.DataAcess
{
    public class Products
    {
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
        public static async Task<ResponseData> CreatNewProduct(string ma_vt)
        {
            string sql = $"exec haravan_GetItemToHaravan '{ma_vt}'";

            DataSet ds_getitem = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
            Res_CreatNewItem data = new Res_CreatNewItem();
            string lst_barcode_exit = "";
            var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");
            string url = $"https://apis.haravan.com/com/products.json";
            HttpClientApp client = new HttpClientApp(token);

            string product_id = "";
            ResponseApiHaravan res;

            if (ds_getitem.Tables[0].Rows.Count == 0) return new ResponseData( "err", $"Sản phẩm không tồn tại" ,"");
            if (ds_getitem.Tables[0].Rows[0]["har_product"].ToString().Trim() != "" && ds_getitem.Tables[0].Rows[0]["har_variant"].ToString().Trim() != "") return new ResponseData("err", $"Sản phẩm đã được đồng bộ", "");
            try
            {
                if (ds_getitem.Tables[1].Rows.Count == 0)
                {
                    DataRow vt = ds_getitem.Tables[0].Rows[0];
                    List<CreatNewItem_Varian> varians = new List<CreatNewItem_Varian>();
                    CreatNewItem_Varian var_temp = new CreatNewItem_Varian();

                    string ten_sp = "";
                    ten_sp = vt["ten_vt"].ToString();

                    string ten_locai_vt = "";
                    ten_locai_vt = vt["loai_vt"].ToString();

                    decimal price = 0;
                    if (ds_getitem.Tables[4].Rows.Count > 0) price = FuncLib.Library.ConvertToDecimal(ds_getitem.Tables[4].Rows[0]["gia_nt2"].ToString());

                    var_temp.barcode = vt["ma_vt"].ToString();
                    var_temp.grams = FuncLib.Library.ConvertToDecimal(vt["weight"].ToString());
                    var_temp.inventory_management = "haravan";
                    var_temp.price = price;
                    var_temp.requires_shipping = true;
                    var_temp.sku = vt["ma_vt"].ToString();
                    var_temp.title = vt["ten_vt"].ToString();
                    var_temp.option1 = "";
                    var_temp.option2 = "";
                    var_temp.option3 = "";
                    int index_option = 0;
                    foreach (DataRow r in ds_getitem.Tables[3].Rows)
                    {
                        if (index_option == 0) var_temp.option1 = r["ten_nh"].ToString();
                        if (index_option == 1) var_temp.option2 = r["ten_nh"].ToString();
                        if (index_option == 2) var_temp.option3 = r["ten_nh"].ToString();
                        if (r["ten_nh"].ToString().Trim() != "") index_option++;
                    }
                    varians.Add(var_temp);


                    Object body = new
                    {
                        product = new
                        {
                            title = ten_sp,
                            body_html = "",
                            product_type = ten_locai_vt,
                            vendor = "ERP",
                            tags = "",
                            published = true,
                            variants = varians
                        }
                    };
                    JObject temp = JObject.FromObject(body);

                    res = await client.Post_Request_WithBody(url, temp.ToString());

                }
                else
                {
                    DataRow vt = ds_getitem.Tables[0].Rows[0];


                    string pro_id = ds_getitem.Tables[1].Rows[0]["har_product"].ToString();
                    CreatNewItem_Varian var_temp = new CreatNewItem_Varian();

                    string ten_sp = "";
                    ten_sp = vt["ten_vt"].ToString();

                    string ten_locai_vt = "";
                    ten_locai_vt = vt["loai_vt"].ToString();

                    decimal price = 0;
                    if (ds_getitem.Tables[4].Rows.Count > 0) price = FuncLib.Library.ConvertToDecimal(ds_getitem.Tables[4].Rows[0]["gia_nt2"].ToString());

                    var_temp.barcode = vt["ma_vt"].ToString();
                    var_temp.grams = FuncLib.Library.ConvertToDecimal(vt["weight"].ToString());
                    var_temp.inventory_management = "haravan";
                    var_temp.price = price;
                    var_temp.requires_shipping = true;
                    var_temp.sku = vt["ma_vt"].ToString();
                    var_temp.title = vt["ten_vt"].ToString();
                    var_temp.option1 = "";
                    var_temp.option2 = "";
                    var_temp.option3 = "";
                    int index_option = 0;
                    foreach (DataRow r in ds_getitem.Tables[3].Rows)
                    {
                        if (index_option == 0) var_temp.option1 = r["ten_nh"].ToString();
                        if (index_option == 1) var_temp.option2 = r["ten_nh"].ToString();
                        if (index_option == 2) var_temp.option3 = r["ten_nh"].ToString();
                        if (r["ten_nh"].ToString().Trim() != "") index_option++;
                    }
                    url = $"https://apis.haravan.com/com/products/{pro_id}/variants.json ";

                    Object body = new
                    {
                        variant = var_temp
                    };
                    JObject temp = JObject.FromObject(body);

                    res = await client.Post_Request_WithBody(url, temp.ToString());
                    product_id = pro_id;
                }

                if (res.status == "ok")
                {
                    JObject root_total = JObject.Parse(res.data);
                    JObject new_var = new JObject();

                    if (ds_getitem.Tables[1].Rows.Count == 0)
                    {
                        JObject product = (JObject)root_total["product"];
                        JArray listVarianCreat = (JArray)product["variants"];
                        product_id = product["id"].ToString();
                        for (int i = 0; i < listVarianCreat.Count; i++)
                        {
                            new_var = (JObject)listVarianCreat[i];
                        }
                    }
                    else new_var = (JObject)root_total["variant"];

                    string new_var_id = new_var["id"].ToString();
                    string barcode = new_var["barcode"].ToString();
                    string sql_gettonkho = Environment.NewLine + $"update dmvt set har_variant='{new_var_id}',har_product='{product_id}' where ma_vt='{barcode}'";

                    sql_gettonkho += Environment.NewLine + $"select isnull(sum(isnull(ton13,0)),0) ton13 from cdvt213 where ma_kho='{MyAppData.ma_kho_SSE_BANLE}' and ma_vt='{barcode}'";
                    DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql_gettonkho);
                    decimal quantity = 0;
                    if (ds.Tables[0].Rows.Count > 0) quantity = FuncLib.Library.ConvertToDecimal(ds.Tables[0].Rows[0]["ton13"].ToString());
                    List<Store_UpdateLineItem> temp_line = new List<Store_UpdateLineItem>();
                    temp_line.Add(new Store_UpdateLineItem(product_id, new_var_id, quantity));
                    url = $"https://apis.haravan.com/com/inventories/adjustorset.json";
                    Object body_kho = new
                    {
                        inventory = new
                        {
                            location_id = MyAppData.ma_kho_Haravan,
                            type = "set",
                            reason = "shrinkage",
                            note = "Update by SSE",
                            line_items = temp_line
                        }
                    };
                    JObject temp_kho = JObject.FromObject(body_kho);

                    ResponseApiHaravan res_kho = await client.Post_Request_WithBody(url, temp_kho.ToString());


                    return new ResponseData("ok", "Thêm thành công", "");
                }
                else
                {
                    return new ResponseData("err", res.data, "");
                }
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message,"");
            }
        }

        public static async Task<ResponseData> UpdateProduct(Req_UpdateHaravan data)
        {
            try
            {
                var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");
                string url = $"https://apis.haravan.com/com/products.json";
                HttpClientApp client = new HttpClientApp(token);


                foreach (Req_updateHaravan_product item in data.items)
                {
                    string ma_vt = item.ma_vt.Trim();
                    string sql = $"exec haravan_GetItemToHaravan '{ma_vt}'";
                    string sql_re = "";
                    DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                    if (ds.Tables[0].Rows.Count == 0) continue;
                    if (ds.Tables[0].Rows[0]["har_variant"].ToString().Trim() == "") continue;

                    string har_product = "";
                    string har_variant = "";
                    har_product = ds.Tables[0].Rows[0]["har_product"].ToString();
                    har_variant = ds.Tables[0].Rows[0]["har_variant"].ToString();


                    CreatNewItem_Varian var_temp = new CreatNewItem_Varian();
                    var_temp.option1 = "";
                    var_temp.option2 = "";
                    var_temp.option3 = "";
                    int index_option = 0;
                    foreach (DataRow r in ds.Tables[3].Rows)
                    {
                        if (index_option == 0) var_temp.option1 = r["ten_nh"].ToString();
                        if (index_option == 1) var_temp.option2 = r["ten_nh"].ToString();
                        if (index_option == 2) var_temp.option3 = r["ten_nh"].ToString();
                        if (r["ten_nh"].ToString().Trim() != "") index_option++;
                    }

                    url = $"https://apis.haravan.com/com/variants/{har_variant}.json ";
                    decimal price = 0;
                    if (ds.Tables[4].Rows.Count > 0) price = Library.ConvertToDecimal(ds.Tables[4].Rows[0]["gia_nt2"].ToString());
                    Object body_var = new
                    {
                        variant = new
                        {
                            price = price,
                            option1 = var_temp.option1,
                            option2 = var_temp.option2,
                            option3 = var_temp.option3,
                        }
                    };
                    JObject body_temp2 = JObject.FromObject(body_var);

                    ResponseApiHaravan res2 = await client.Put_Request_WithBody(url, body_temp2.ToString());
                    if (res2.status == "ok")
                    {
                        url = $"https://apis.haravan.com/com/inventory_locations.json?location_ids={MyAppData.ma_kho_Haravan}&variant_ids={har_variant}";
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


                        List<Store_UpdateLineItem> temp_line = new List<Store_UpdateLineItem>();
                        temp_line.Add(new Store_UpdateLineItem(har_product, har_variant, quantity));
                        url = $"https://apis.haravan.com/com/inventories/adjustorset.json";
                        Object body_kho = new
                        {
                            inventory = new
                            {
                                location_id = MyAppData.ma_kho_Haravan,
                                type = "adjust",
                                reason = "",
                                note = "Update by SSE",
                                line_items = temp_line
                            }
                        };
                        JObject temp_kho = JObject.FromObject(body_kho);

                        ResponseApiHaravan res_kho = await client.Post_Request_WithBody(url, temp_kho.ToString());


                    }
                }
                return new ResponseData( "ok", "Chỉnh sửa thành công" ,"");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message,"");
            }
        }
        public static async Task<ResponseData> DeleteProduct(string ma_vt)
        {
            try
            {
                var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");
                string url = $"https://apis.haravan.com/com/products.json";
                HttpClientApp client = new HttpClientApp(token);

                string sql = $"select * from dmvt where ma_vt = '{ma_vt}'";

                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0) return new ResponseData("err", $"Sản phẩm không tồn tại" ,"");
                if (ds.Tables[0].Rows[0]["har_product"].ToString().Trim() != "" && ds.Tables[0].Rows[0]["har_product"].ToString().Trim() != "") new ResponseData("err", $"Sản phẩm đã được đồng bộ", "");

                string har_variant = ds.Tables[0].Rows[0]["har_variant"].ToString();
                url = $"https://apis.haravan.com/com/variants/{har_variant}.json ";
                ResponseApiHaravan res2 = await client.Del_Request(url);
                if (res2.status == "ok")
                {
                    return new ResponseData("ok", "Xóa thành công" ,"");
                }
                else
                {
                    return new ResponseData("err", res2.data, "");
                }
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public static async Task<ResponseData> CreatAllNewProduct(List<ProductToHaravanAll> lst_newproduct)
        {

            var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");
            string url = $"https://apis.haravan.com/com/products.json";
            HttpClientApp client = new HttpClientApp(token);

            string product_id = "";
            ResponseApiHaravan res;
            string sql_update = "";

            int tt = 0;

            foreach (ProductToHaravanAll p in lst_newproduct)
            {
                tt++;
                string sql = $"exec haravan_GetAllItemToHaravan '{p.ma_vt}',0 ";
                DataSet ds_getitem = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds_getitem.Tables[0].Rows.Count == 0) continue;
                string ten_sp = "";
                string ten_locai_vt = "";
                List<CreatNewItem_Varian> varians = new List<CreatNewItem_Varian>();
                foreach (DataRow vt in ds_getitem.Tables[0].Rows)
                {
                    
                    CreatNewItem_Varian var_temp = new CreatNewItem_Varian();

                    ten_sp = vt["ten_vt2"].ToString();

                   
                    ten_locai_vt = vt["loai_vt"].ToString();

                    decimal price = 0;
                    price = FuncLib.Library.ConvertToDecimal(vt["gia"].ToString());

                    var_temp.barcode = vt["ma_vt"].ToString();
                    var_temp.grams = FuncLib.Library.ConvertToDecimal(vt["weight"].ToString());
                    var_temp.inventory_management = "haravan";
                    var_temp.price = price;
                    var_temp.requires_shipping = true;
                    var_temp.sku = vt["ma_vt"].ToString();
                    var_temp.title = vt["ten_vt"].ToString();
                    var_temp.option1 = "";
                    var_temp.option2 = "";
                    var_temp.option3 = "";
                    int index_option = 0;
                    foreach (DataRow r in ds_getitem.Tables[1].Rows)
                    {
                        if (r["ma_vt"].ToString().Trim() == var_temp.barcode)
                        {
                            if (index_option == 0) var_temp.option1 = r["ten_nh"].ToString();
                            if (index_option == 1) var_temp.option2 = r["ten_nh"].ToString();
                            if (index_option == 2) var_temp.option3 = r["ten_nh"].ToString();
                            if (r["ten_nh"].ToString().Trim() != "") index_option++;
                        }
                    }
                    varians.Add(var_temp);
                  
                }
                Object body = new
                {
                    product = new
                    {
                        title = ten_sp,
                        body_html = "",
                        product_type = ten_locai_vt,
                        vendor = "ERP",
                        tags = "",
                        published = true,
                        variants = varians
                    }
                };
                JObject temp = JObject.FromObject(body);

                url = $"https://apis.haravan.com/com/products.json";
                res = await client.Post_Request_WithBody(url, temp.ToString());
                if (res.status == "ok") { }
                else
                {
                    System.Threading.Thread.Sleep(2000);
                    res = await client.Post_Request_WithBody(url, temp.ToString());
                }
                if (res.status == "ok")
                {
                    JObject root_total = JObject.Parse(res.data);
                    JObject new_var = new JObject();

                    JObject product = (JObject)root_total["product"];
                    JArray listVarianCreat = (JArray)product["variants"];
                    product_id = product["id"].ToString();
                    decimal quantity = 0;
                    List<Store_UpdateLineItem> temp_line = new List<Store_UpdateLineItem>();
                    for (int i = 0; i < listVarianCreat.Count; i++)
                    {
                        new_var = (JObject)listVarianCreat[i];
                        string new_var_id = new_var["id"].ToString();
                        string barcode = new_var["barcode"].ToString();
                        sql_update += Environment.NewLine + $"update dmvt set har_variant='{new_var_id}',har_product='{product_id}' where ma_vt='{barcode}'";
                        foreach (DataRow r_kho in ds_getitem.Tables[0].Rows)
                        {
                            if (r_kho["ma_vt"] == barcode)
                            {
                                quantity = FuncLib.Library.ConvertToDecimal(r_kho["ton13"].ToString());
                                temp_line.Add(new Store_UpdateLineItem(product_id, new_var_id, quantity));
                            }
                        }
                    }
                    url = $"https://apis.haravan.com/com/inventories/adjustorset.json";
                    Object body_kho = new
                    {
                        inventory = new
                        {
                            location_id = MyAppData.ma_kho_Haravan,
                            type = "set",
                            reason = "shrinkage",
                            note = "Update by SSE",
                            line_items = temp_line
                        }
                    };
                    JObject temp_kho = JObject.FromObject(body_kho);

                    ResponseApiHaravan res_kho = await client.Post_Request_WithBody(url, temp_kho.ToString());
                    if (res_kho.status == "ok") { }
                    else
                    {
                        System.Threading.Thread.Sleep(2000);
                        res_kho = await client.Post_Request_WithBody(url, temp_kho.ToString());
                    }
                }
            }
            if (sql_update != "") DAL.DAL_SQL.ExecuteNonquery(sql_update);
            return new ResponseData("ok", "Thêm thành công", "");
        }

        public static async Task<ResponseData> CreatAllProductVariant(List<ProductToHaravanAll> lst_newproduct)
        {

            var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");
            string url = $"https://apis.haravan.com/com/products.json";
            HttpClientApp client = new HttpClientApp(token);

            string product_id = "";
            ResponseApiHaravan res;
            string sql_update = "";

            List<Store_UpdateLineItem> temp_line = new List<Store_UpdateLineItem>();
            foreach (ProductToHaravanAll p in lst_newproduct)
            {
                string sql = $"exec haravan_GetAllItemToHaravan '{p.ma_vt}',0 ";
                DataSet ds_getitem = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                string ten_sp = "";
                string ten_locai_vt = "";
                List<CreatNewItem_Varian> varians = new List<CreatNewItem_Varian>();
                foreach (DataRow vt in ds_getitem.Tables[0].Rows)
                {

                    CreatNewItem_Varian var_temp = new CreatNewItem_Varian();

                    ten_sp = vt["ten_vt"].ToString();


                    ten_locai_vt = vt["loai_vt"].ToString();

                    decimal price = 0;
                    price = FuncLib.Library.ConvertToDecimal(vt["gia"].ToString());

                    var_temp.barcode = vt["ma_vt"].ToString();
                    var_temp.grams = FuncLib.Library.ConvertToDecimal(vt["weight"].ToString());
                    var_temp.inventory_management = "haravan";
                    var_temp.price = price;
                    var_temp.requires_shipping = true;
                    var_temp.sku = vt["ma_vt"].ToString();
                    var_temp.title = vt["ten_vt"].ToString();
                    var_temp.option1 = "";
                    var_temp.option2 = "";
                    var_temp.option3 = "";
                    int index_option = 0;
                    foreach (DataRow r in ds_getitem.Tables[1].Rows)
                    {
                        if (r["ma_vt"].ToString().Trim() == var_temp.barcode)
                        {
                            if (index_option == 0) var_temp.option1 = r["ten_nh"].ToString();
                            if (index_option == 1) var_temp.option2 = r["ten_nh"].ToString();
                            if (index_option == 2) var_temp.option3 = r["ten_nh"].ToString();
                            if (r["ten_nh"].ToString().Trim() != "") index_option++;
                        }
                    }
                    varians.Add(var_temp);

                    Object body = new
                    {
                        variant = var_temp
                    };
                    product_id = p.har_product.Trim();
                    JObject temp = JObject.FromObject(body);

                    url = $"https://apis.haravan.com/com/products/{product_id}/variants.json ";

                    res = await client.Post_Request_WithBody(url, temp.ToString());
                    if (res.status == "ok") { }
                    else
                    {
                        System.Threading.Thread.Sleep(2000);
                        res = await client.Post_Request_WithBody(url, temp.ToString());
                    }
                    if (res.status == "ok")
                    {
                        JObject root_total = JObject.Parse(res.data);
                        JObject new_var = new JObject();
                        new_var = (JObject)root_total["variant"];
                   
                        decimal quantity = 0;
                        string new_var_id = new_var["id"].ToString();
                        string barcode = new_var["barcode"].ToString();
                        sql_update = Environment.NewLine + $"update dmvt set har_variant='{new_var_id}',har_product='{product_id}' where ma_vt='{barcode}'";
                        foreach (DataRow r_kho in ds_getitem.Tables[0].Rows)
                        {
                            if (r_kho["ma_vt"] == barcode)
                            {
                                quantity = FuncLib.Library.ConvertToDecimal(r_kho["ton13"].ToString());
                                temp_line.Add(new Store_UpdateLineItem(product_id, new_var_id, quantity));
                            }
                        }
                    }
                }
            }
            if(sql_update !="") DAL.DAL_SQL.ExecuteNonquery(sql_update);
            url = $"https://apis.haravan.com/com/inventories/adjustorset.json";
            Object body_kho = new
            {
                inventory = new
                {
                    location_id = MyAppData.ma_kho_Haravan,
                    type = "set",
                    reason = "shrinkage",
                    note = "Update by SSE",
                    line_items = temp_line
                }
            };
            JObject temp_kho = JObject.FromObject(body_kho);

            ResponseApiHaravan res_kho = await client.Post_Request_WithBody(url, temp_kho.ToString());
            if (res_kho.status == "ok") { }
            else
            {
                System.Threading.Thread.Sleep(2000);
                res_kho = await client.Get_Request(url);
            }
            return new ResponseData("ok", "Thêm thành công", "");
        }

        public async Task<ResponseData> CheckExitBarcode(string barcode)
        {
            try
            {
                if (barcode == null || barcode == "")
                    return new ResponseData("err", "Không có sản phẩm nào có barcode này trên haravan 1", "");
                var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");

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
                var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");

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
            var constring = MyAppData.config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            conn.Open();
            try
            {

                string locationId = MyAppData.config.GetValue<string>("config_Haravan:store024");
                
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
                var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");
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

        public static async Task<int> MapHaravanProductcode()
        {
            try
            {
                string sql = "";
                var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");
                string url = $"https://apis.haravan.com/com/locations.json";
                HttpClientApp client = new HttpClientApp(token);
                int page = 0;
                bool checkss = true;
                while (checkss)
                {
                    page += 1;
                    url = $"https://apis.haravan.com/com/products.json?page={page}&limit=50";
                    ResponseApiHaravan res_kho;
                    ResponseApiHaravan res_kho_te = await client.Get_Request(url);
                    if (res_kho_te.status == "ok")
                    {
                        res_kho = res_kho_te;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(2000);
                        res_kho = await client.Get_Request(url);
                    }
                    JObject root_products = JObject.Parse(res_kho.data);
                    JArray ListItem = (JArray)root_products["products"];
                    for (int i = 0; i < ListItem.Count; i++)
                    {
                        var product = (JObject)ListItem[i];

                        JArray listvarian = (JArray)product["variants"];
                        string pro_id = (string)product["id"] ?? "";

                        for (int j = 0; j < listvarian.Count; j++)
                        {
                            var varian = (JObject)listvarian[j];
                            string ma_vt = (string)varian["barcode"];
                            string varian_id = (string)varian["id"];
                            string title = (string)varian["title"];
                            if (ma_vt != null && ma_vt != "")
                            {
                                sql += Environment.NewLine + $"Update dmvt set har_product='{pro_id}',har_variant='{varian_id}' where ma_vt='{ma_vt}'";

                            }
                        }
                    }
                    if (ListItem.Count >= 50)
                    {
                        checkss = true;
                    }
                    else checkss = false;
                }
                DAL.DAL_SQL.ExecuteNonquery(sql);
                return 1;
            }
            catch (Exception e)
            {
                return 1;
            }
        }

    }
}
