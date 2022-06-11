using Haravan.ModelsApp;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.ModelVc
{
    public class ALP_Location
    {
        public string status { get; set; }
        public string From_ShippingAddress { get; set; }
        public int From_ProvinceId { get; set; }
        public int From_DistrictId { get; set; }
        public int From_WardId { get; set; }
        public int From_HubId { get; set; }
        public string AddressNoteFrom { get; set; }
    }
    public class ALP_Login
    {
        public string status { get; set; }
        public string message { get; set; }
        public string token { get; set; }
    }
    public class ALP_ServiceId
    {
        public string status { get; set; }
        public int id { get; set; }
        public DateTime expectedDeliveryTime { get; set; }
        public decimal price { get; set; }
        public string name { get; set; }
        public bool isEnabled { get; set; }
    }
    public class ModelALP
    {
        public static ALP_Location GetAdressLocation(string ma_kho)
        {
            ALP_Location re = new ALP_Location();
            try
            {
                string sql = $"select ma_dvcs,ma_kho,b.alp_ma_tinh,c.alp_ma_quan,d.alp_ma_phuong,d.alp_hubid,dia_chi,d.ten_phuong+','+c.ten_quan+','+b.ten_tinh as address from dmkho a left join hrdmtinh b on a.ma_tinh = b.ma_tinh left join hrdmquan c on a.ma_quan = c.ma_quan left join hrdmphuong d on a.ma_phuong = d.ma_phuong  where ma_kho='{ma_kho.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    re.status = "KHONGCOKHO";
                    return re;
                }
                re.status = "OK";
                re.From_ShippingAddress = ds.Tables[0].Rows[0]["address"].ToString().Trim();
                re.From_ProvinceId =Convert.ToInt32(ds.Tables[0].Rows[0]["alp_ma_tinh"]);
                re.From_DistrictId = Convert.ToInt32(ds.Tables[0].Rows[0]["alp_ma_quan"]);
                re.From_WardId = Convert.ToInt32(ds.Tables[0].Rows[0]["alp_ma_phuong"]);
                re.AddressNoteFrom = ds.Tables[0].Rows[0]["dia_chi"].ToString().Trim();
                re.From_HubId = Convert.ToInt32(ds.Tables[0].Rows[0]["alp_hubid"]);
                return re;
            }
            catch (Exception e)
            {
                re.status = "KHONGCOKHO";
                return re;
            }
        }

        public static ALP_Location GetAdressToLocation(string ma_phuong)
        {
            ALP_Location re = new ALP_Location();
            try
            {
                string sql = $"select c.alp_ma_tinh,b.alp_ma_quan,a.alp_ma_phuong,a.alp_hubid,a.ten_phuong+','+b.ten_quan+','+c.ten_tinh as address from hrdmphuong a left join hrdmquan b on a.ma_quan = b.ma_quan left join hrdmtinh c on a.ma_tinh = c.ma_tinh where a.ma_phuong='{ma_phuong.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    re.status = "KHONGCOKHO";
                    return re;
                }
                re.status = "OK";
                re.From_ShippingAddress = ds.Tables[0].Rows[0]["address"].ToString().Trim();
                re.From_ProvinceId = Convert.ToInt32(ds.Tables[0].Rows[0]["alp_ma_tinh"]);
                re.From_DistrictId = Convert.ToInt32(ds.Tables[0].Rows[0]["alp_ma_quan"]);
                re.From_WardId = Convert.ToInt32(ds.Tables[0].Rows[0]["alp_ma_phuong"]);
                re.From_HubId = Convert.ToInt32(ds.Tables[0].Rows[0]["alp_hubid"]);
                re.AddressNoteFrom = "";
                return re;
            }
            catch (Exception e)
            {
                re.status = "KHONGCOKHO";
                return re;
            }
        }
        public static string GetTinh(string ma_tinh)
        {
            string re = "";
            try
            {
                string sql = $"select * from hrdmtinh where ma_tinh='{ma_tinh.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                re = ds.Tables[0].Rows[0]["alp_ma_tinh"].ToString().Trim();
                return re;
            }
            catch (Exception e)
            {
                return re;
            }
        }
        public static string GetQuan(string ma_quan)
        {
            string re = "";
            try
            {
                string sql = $"select * from hrdmquan where ma_quan='{ma_quan.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                re = ds.Tables[0].Rows[0]["alp_ma_quan"].ToString().Trim();
                return re;
            }
            catch (Exception e)
            {
                return re;
            }
        }
        public static string GetPhuong(string ma_phuong)
        {
            string re = "";
            try
            {
                string sql = $"select * from hrdmphuong where ma_phuong='{ma_phuong.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                re = ds.Tables[0].Rows[0]["alp_ma_phuong"].ToString().Trim();
                return re;
            }
            catch (Exception e)
            {
                return re;
            }
        }
        public static string GetHubId(string ma_phuong)
        {
            string re = "";
            try
            {
                string sql = $"select * from hrdmphuong where ma_phuong='{ma_phuong.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                re = ds.Tables[0].Rows[0]["alp_hubid"].ToString().Trim();
                return re;
            }
            catch (Exception e)
            {
                return re;
            }
        }

        public static async Task<ALP_ServiceId> GetServiceId(int fromDistrictId, int fromWardId , int ToDistrictId,int Weight,int totalItem, int StructureId)
        {
            ALP_ServiceId re = new ALP_ServiceId();
            try
            {
                var token = MyAppData.ALP_token;
                if (token == "")
                {
                    var ss = await GetToken();
                    token = ss.token;
                }
                string url = $"http://postapi.shipnhanh.vn/api/price/getListService";
                HttpClientApp client = new HttpClientApp(token);
                Object obj = new
                {
                    senderId = 2362,
                    fromDistrictId= fromDistrictId,
                    fromWardId = fromWardId,
                    toDistrictId = ToDistrictId,
                    insured =0,
                    Weight = Weight,
                    totalItem = totalItem,
                    structureId = StructureId

                };
                bool check = false;
                JObject temp = JObject.FromObject(obj);
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
                if (res.status == "ok"){ check = true; }
                else
                {
                    var ss = await GetToken();
                    token = ss.token;
                    client = new HttpClientApp(token);
                    res = await client.Post_Request_WithBody(url, temp.ToString());
                    if (res.status == "ok") { check = true; }
                    else { check = false; }
                }
                if (check)
                {
                    re.status = "ERR";
                    JObject root_data = JObject.Parse(res.data);
                    int code = (int)root_data["isSuccess"];
                    if (code == 1)
                    {
                        JArray datare = (JArray)root_data["data"];
                        if (datare.Count == 0) return re;
                        decimal price_min = Decimal.MaxValue;
                        for (int i = 0; i < datare.Count; i++)
                        {
                            JObject service = (JObject)datare[i];
                            decimal price = (decimal)service["price"];
                            bool isEnable = (bool)service["isEnabled"];
                            int id = (int)service["id"];
                            if (price <= price_min && isEnable == false && (id == 1 | id == 6))
                            {
                                string d = (string)service["expectedDeliveryTime"];
                                re.status = "OK";
                                re.id = (int)service["id"];
                                DateTime date = FuncLib.Library.ConvertDatetime(d);
                                re.expectedDeliveryTime = date;
                                re.price = price;
                                re.name = (string)service["name"];
                                re.isEnabled = isEnable;
                                price_min = price;
                            }
                        }
                    }
                    return re;
                }
                else
                {
                    re.status = "ERR";
                    return re;
                }
            }
            catch (Exception e)
            {
                re.status = "ERR";
                return re;
            }
        }

        public static async Task<ResponseApiHaravan> CreatNewOrder(ALP_Data send_data)
        {
            var token = MyAppData.ALP_token;
            if (token == "")
            {
                var ss = await GetToken();
                token = ss.token;
            }
            string url = $"http://cusapi.shipnhanh.vn/api/Shipment/CreateByTPL";
            HttpClientApp client = new HttpClientApp(token);
            JObject temp = JObject.FromObject(send_data);
            ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
            if (res.status == "ok") { return res; }
            else
            {
                var ss = await GetToken();
                token = ss.token;
                client = new HttpClientApp(token);
                res = await client.Post_Request_WithBody(url, temp.ToString());
                return res;
            }
        }

        public static async Task<ALP_Login> GetToken()
        {
            ALP_Login re = new ALP_Login();
            try
            {
                var username  = MyAppData.config.GetValue<string>("Vanchuyen:ALP:username");
                var password = MyAppData.config.GetValue<string>("Vanchuyen:ALP:password");
                string url = $"http://cusapi.shipnhanh.vn/api/Account/SignIn";
                HttpClientApp client = new HttpClientApp();
                Object obj = new
                {
                    UserName = username,
                    PassWord = password,

                };
                JObject temp = JObject.FromObject(obj);
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
                if (res.status == "ok")
                {
                    re.status = "ERR";
                    JObject root_data = JObject.Parse(res.data);
                    int code = (int)root_data["isSuccess"];
                    if (code == 1)
                    {
                        JObject datare = (JObject)root_data["data"];
                        re.token = (string)datare["token"];
                        MyAppData.ALP_token = re.token;
                        re.status = "OK";                      
                    }
                    else
                    {
                        re.message = (string)root_data["message"];
                    }
                    return re;
                }
                else
                {
                    re.status = "ERR";
                    re.message = res.data;
                    return re;
                }
            }
            catch (Exception e)
            {
                re.status = "ERR";
                re.message = e.Message;
                return re;
            }
        }
    }
}
