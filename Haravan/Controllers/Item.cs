using Haravan.FuncLib;
using Haravan.Haravan.Common;
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
using System.Threading.Tasks;

namespace Haravan.Controllers
{
    [Route("Item")]
    [ApiController]
    public class Item : Controller
    {
        private readonly IConfiguration _config;


        public Item(IConfiguration config)
        {
            _config = config;
        }

        //Lấy ds sản phẩm từ haravan
        [HttpPost]
        [Route("GetItemHaravan")]
        public async Task<IActionResult> GetItemHaravan()
        {
            bool check = Library.CheckAuthentication(_config, this);
            if (!check) return StatusCode(401);
            try
            {
                string sql = "";
                var token = _config.GetValue<string>("config_Haravan:private_token");
                string url = $"https://apis.haravan.com/com/locations.json";
                HttpClientApp client = new HttpClientApp(token);
                int page = 0;
                bool checkss = true;
                while (checkss)
                {
                    page += 1;
                    url = $"https://apis.haravan.com/com/products.json?page={page}&limit=50";
                    ModelsApp.ResponseApiHaravan res_kho;
                    ModelsApp.ResponseApiHaravan res_kho_te = await client.Get_Request(url);
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

                        for(int j = 0; j < listvarian.Count; j++)
                        {
                            var varian = (JObject)listvarian[j];
                            string ma_vt = (string)varian["barcode"];
                            string varian_id = (string)varian["id"];
                            string title = (string)varian["title"];
                            if(ma_vt !=null && ma_vt != "")
                            {
                                sql +=Environment.NewLine + "INSERT INTO dmvt(ma_vt,ma_vt2,ten_vt,dvt,nhieu_dvt,vt_ton_kho,gia_ton,loai_vt,status,datetime0,datetime2,s1,s2,s3) ";
                                sql += Environment.NewLine + $"VALUES('{ma_vt}','','{title}','Cái',0,1,1,61,'1',GETDATE(),GETDATE(),'{pro_id}','{varian_id}','0')";

                            }
                        }
                    }
                    if (ListItem.Count >= 50)
                    {
                        checkss = true;
                    }
                    else checkss = false;
                }
                return Ok(sql);

            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        [Route("GetHaravanVariantCode")]
        public async Task<IActionResult> GetHaravanVariantCode()
        {
            bool check = Library.CheckAuthentication(_config, this);
            if (!check) return StatusCode(401);
            try
            {
                await Haravan.DataAcess.Products.MapHaravanProductcode();
                await Haravan.DataAcess.Products2.MapHaravanProductcode();
                await Haravan.DataAcess.Products3.MapHaravanProductcode();
                return Ok();
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }


        [HttpPost]
        [Route("CreateNewItem")]
        public async Task<IActionResult> CreateNewItem(string ma_vt)
        {
            bool check = Library.CheckAuthentication(_config, this);
            if (!check) return StatusCode(401);

            string sql = $"select ma_kho from dmvt where ma_vt='{ma_vt}'";
            DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
            if (ds.Tables[0].Rows.Count == 0) return StatusCode(200, new Haravan.Common.ResponseData("err", "Sản phẩm không tồn tại", ""));
            string ma_kho = ds.Tables[0].Rows[0]["ma_kho"].ToString();
            Haravan.Common.ResponseData res;
            if (ma_kho != MyAppData.ma_kho_SSE_MAWO)
            {
                res = await Haravan.DataAcess.Products.CreatNewProduct(ma_vt);
                res = await Haravan.DataAcess.Products2.CreatNewProduct(ma_vt);
            }
            else
            {
                res = await Haravan.DataAcess.Products3.CreatNewProduct(ma_vt);
            }
            return StatusCode(200, res);

        }

        [HttpPost]
        [Route("UpdateItem")]
        public async Task<IActionResult> UpdateItem(Haravan.Common.Req_UpdateHaravan data)
        {
            bool check = Library.CheckAuthentication(_config, this);

            Haravan.Common.ResponseData res = await Haravan.DataAcess.Products.UpdateProduct(data);
            res = await Haravan.DataAcess.Products2.UpdateProduct(data);
            res = await Haravan.DataAcess.Products3.UpdateProduct(data);
            return StatusCode(200, res);         
        }

        [HttpPost]
        [Route("UpdateGiaToHaravan")]
        public async Task<IActionResult> UpdateGiaToHaravan()
        {
            bool check = Library.CheckAuthentication(_config, this);
            string sql = $"exec haravan_GetAllItemToUpdateHaravan ";

            DataSet ds_getitem = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
            Haravan.Common.Req_UpdateHaravan data1 = new Haravan.Common.Req_UpdateHaravan();
            data1.items = new List<Haravan.Common.Req_updateHaravan_product>();

            Haravan.Common.Req_UpdateHaravan data2 = new Haravan.Common.Req_UpdateHaravan();
            data2.items = new List<Haravan.Common.Req_updateHaravan_product>();

            Haravan.Common.Req_UpdateHaravan data3 = new Haravan.Common.Req_UpdateHaravan();
            data3.items = new List<Haravan.Common.Req_updateHaravan_product>();

            foreach (DataRow r in ds_getitem.Tables[0].Rows)
            {
                Haravan.Common.Req_updateHaravan_product temp = new Haravan.Common.Req_updateHaravan_product();
                temp.ma_vt = r["ma_vt"].ToString();
                temp.changeProduct = true;
                data1.items.Add(temp);
            }
            foreach (DataRow r in ds_getitem.Tables[1].Rows)
            {
                Haravan.Common.Req_updateHaravan_product temp = new Haravan.Common.Req_updateHaravan_product();
                temp.ma_vt = r["ma_vt"].ToString();
                temp.changeProduct = true;
                data2.items.Add(temp);
            }
            foreach (DataRow r in ds_getitem.Tables[2].Rows)
            {
                Haravan.Common.Req_updateHaravan_product temp = new Haravan.Common.Req_updateHaravan_product();
                temp.ma_vt = r["ma_vt"].ToString();
                temp.changeProduct = true;
                data3.items.Add(temp);
            }
            Haravan.Common.ResponseData res = await Haravan.DataAcess.Products.UpdateProduct(data1);
            res = await Haravan.DataAcess.Products2.UpdateProduct(data2);
            res = await Haravan.DataAcess.Products3.UpdateProduct(data3);
            return StatusCode(200, res);
        }

        [HttpPost]
        [Route("DeleteItem")]
        public async Task<IActionResult> DeleteItem(string ma_vt)
        {
            bool check = Library.CheckAuthentication(_config, this);
            Haravan.Common.ResponseData res = await Haravan.DataAcess.Products.DeleteProduct(ma_vt);
            res = await Haravan.DataAcess.Products2.DeleteProduct(ma_vt);
            res = await Haravan.DataAcess.Products3.DeleteProduct(ma_vt);
            return StatusCode(200, res);
        }
        [HttpPost]
        [Route("CreateAllProduct")]
        public async Task<IActionResult> CreateAllProduct()
        {
            bool check = Library.CheckAuthentication(_config, this);
            if (!check) return StatusCode(401);
            string sql = $"exec haravan_GetAllMasterItemToHaravan ";

            DataSet ds_getitem = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
            string lst_barcode_exit = "";

            List<ProductToHaravanAll> lst_newproduct_1 = new List<ProductToHaravanAll>();
            List<ProductToHaravanAll> lst_product_1 = new List<ProductToHaravanAll>();
            List<ProductToHaravanAll> lst_newproduct_2 = new List<ProductToHaravanAll>();
            List<ProductToHaravanAll> lst_product_2 = new List<ProductToHaravanAll>();
            List<ProductToHaravanAll> lst_newproduct_3 = new List<ProductToHaravanAll>();
            List<ProductToHaravanAll> lst_product_3 = new List<ProductToHaravanAll>();
            foreach (DataRow r in ds_getitem.Tables[0].Rows)
            {
                ProductToHaravanAll temp = new ProductToHaravanAll();
                temp.ma_vt = r["ma_vt2"].ToString();
                if (r["har_product_1"].ToString() == "")
                {
                    temp.har_product = "";
                    temp.so_luong = Library.ConvertToDecimal(r["ton13_1"].ToString());
                    lst_newproduct_1.Add(temp);
                }
                else
                {
                    temp.har_product = r["har_product_1"].ToString();
                    if (FuncLib.Library.ConvertToDecimal(r["total_record_1"].ToString()) > 0)
                    {
                        temp.so_luong = Library.ConvertToDecimal(r["ton13_1"].ToString());
                        lst_product_1.Add(temp);
                    }
                }
                if (r["har_product_2"].ToString() == "")
                {
                    temp.har_product = "";
                    temp.so_luong = Library.ConvertToDecimal(r["ton13_2"].ToString());
                    lst_newproduct_2.Add(temp);
                }
                else
                {
                    temp.har_product = r["har_product_2"].ToString();
                    if (FuncLib.Library.ConvertToDecimal(r["total_record_2"].ToString()) > 0)
                    {
                        temp.so_luong = Library.ConvertToDecimal(r["ton13_2"].ToString());
                        lst_product_2.Add(temp);
                    }
                }
                if (r["har_product_3"].ToString() == "")
                {
                    temp.har_product = "";
                    temp.so_luong = Library.ConvertToDecimal(r["ton13_3"].ToString());
                    lst_newproduct_3.Add(temp);
                }
                else
                {
                    temp.har_product = r["har_product_3"].ToString();
                    if (FuncLib.Library.ConvertToDecimal(r["total_record_3"].ToString()) > 0)
                    {
                        temp.so_luong = Library.ConvertToDecimal(r["ton13_3"].ToString());
                        lst_product_3.Add(temp);
                    }
                }
            }

            Haravan.Common.ResponseData res = await Haravan.DataAcess.Products.CreatAllNewProduct(lst_newproduct_1);
            res = await Haravan.DataAcess.Products2.CreatAllNewProduct(lst_newproduct_2);
            res = await Haravan.DataAcess.Products3.CreatAllNewProduct(lst_newproduct_3);

            res = await Haravan.DataAcess.Products.CreatAllProductVariant(lst_product_1);
            res = await Haravan.DataAcess.Products2.CreatAllProductVariant(lst_product_2);
            res = await Haravan.DataAcess.Products3.CreatAllProductVariant(lst_product_3);
            return StatusCode(200, res);
        }


    }
}
