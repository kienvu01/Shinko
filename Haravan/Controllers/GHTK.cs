using Haravan.FuncLib;
using Haravan.ModelsApp;
using Haravan.ModelVc;
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
    [Route("GHTK")]
    [ApiController]
    public class GHTK : ControllerBase
    {
        private readonly IConfiguration _config;
        public GHTK(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        [Route("HuyGH")]
        public async Task<IActionResult> HuyGiaoHang([FromBody] Re_HuyGH data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);

                var token = _config.GetValue<string>("Vanchuyen:GHTK:privateToken");
                string url = $"https://services.giaohangtietkiem.vn/services/shipment/cancel/{data.ma_vd}";
                HttpClientApp client = new HttpClientApp();
                client.AddHeader("Token", token);

                ResponseApiHaravan res = await client.Post_Request_WithBody(url, "");
                if (res.status == "ok")
                {
                    return Ok(new ResponseData("ok", "Hủy giao hàng thành công", ""));
                }
                else
                {
                    return Ok(new ResponseData("err", res.data, ""));
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
        [Route("GiaoHang")]
        public async Task<IActionResult> TaoDonGiaoHang([FromBody] GiaoHang data)
        {
            var constring = _config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            JObject logdata = JObject.FromObject(data);

            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);

                ModelGHN ghn = new ModelGHN(_config);

                if (data.stt_rec == null || data.stt_rec.Trim() == "") return StatusCode(200, new { status = "err", message = "Bắt buôc phải có mã hóa đơn" });
                if (data.tablesufix == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có table sufix" });
                if (!ModelGHN.CheckOrderClose(data.stt_rec.Trim(), data.tablesufix.Trim())) return StatusCode(200, new { status = "err", message = "Đơn phải ở trạng thái đóng đơn trước khi giao hàng" });
                if (data.pt_vc == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có phương thức vận chuyển" });
                if (!Library.CheckStringNgayThang(data.tablesufix)) return StatusCode(200, new { status = "err", message = "tablesufix phải có ddingj dạng yyyyMM" });

                if (data.trong_luong == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có chiều rộng" });

                if ( data.trong_luong <= 0 ) return StatusCode(200, new { status = "err", message = "Thông tin kích thước gói hàng : chiều cao,chiều rộng,chiều dài,cân nặng phải >0" });
                if (data.tien_boi_thuong == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có tiền bồi thường" });
                if (data.nguoi_tra == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có người trả" });

                if (data.to_name == null || data.to_name.Trim() == "") return StatusCode(200, new { status = "err", message = "Bắt buôc phải có tên người nhận hàng" });
                if (data.to_phone == null || data.to_phone.Trim() == "") return StatusCode(200, new { status = "err", message = "Bắt buôc phải có số điện thoại người nhận hàng" });
                if (data.tien_thu_ho == null || data.tien_thu_ho < 0) return StatusCode(200, new { status = "err", message = "Bắt buộc nhập tiền thu hộ và tiền thu hộ phải >=0" });
                //if (!ModelGHN.CheckCodAmont(data.stt_rec,data.tablesufix,data.tien_thu_ho)) return StatusCode(200, new { status = "err", message = "Tiền thu hộ không khớp.Bạn nên Lưu dữ liệu giao hàng trước" });

                Orders or = new Orders(_config);
                ResponseData re = await or.CreatFulliment(data.stt_rec, data.tablesufix);
                if (re.status == "err")
                {
                    ApiLog.logval("err_log_ghtk_creatFulliment", re.message);
                }

                string sql_gethdgh = $"select * from m81${data.tablesufix} where stt_rec='{data.stt_rec}' " +
                    $" select t1.ten_vt,t0.* from d81${data.tablesufix} t0 left join dmvt t1 on t0.ma_vt = t1.ma_vt where stt_rec='{data.stt_rec}'  ";
                SqlDataAdapter da = new SqlDataAdapter(sql_gethdgh, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count == 0) return StatusCode(200, new { status = "err", message = $"Không tìm thấy hóa đơn stt_rec={data.stt_rec} " });
                DataRow master = ds.Tables[0].Rows[0];
                if (ds.Tables[1].Rows.Count == 0) return StatusCode(200, new { status = "err", message = $"hóa đơn {data.stt_rec} chưa nhập sản phẩm để vận chuyển" });
                GHTK_Data send_data = new GHTK_Data();

                string ma_ch = master["ma_dvcs"].ToString();
                string phone = ghn.GetReturnPhone(ma_ch);
                GHTK_Data_Order order_data = new GHTK_Data_Order();

                string ma_kho = ds.Tables[1].Rows[0]["ma_kho"].ToString();
                GHTK_Location return_address = ModelGHTK.GetAdressLocation(ma_kho);
                if(return_address.status == "KHONGCOKHO") return StatusCode(200, new { status = "err", message = $"Không tồn tại địa chỉ kho {ma_kho}" });
                order_data.id = data.stt_rec;
                order_data.pick_name = $"Eva De Eva (${ma_ch})"; 
                order_data.pick_address_id = return_address.pick_address_id;
                order_data.pick_address = return_address.pick_address;
                order_data.pick_province = return_address.pick_province;
                order_data.pick_district = return_address.pick_district;
                order_data.pick_ward = return_address.pick_ward;
                order_data.pick_tel = phone.Trim();
                order_data.tel = data.to_phone.Trim();
                order_data.name = data.to_name.Trim();

                string temp_dia_chi = "";
                if ((master["dia_chi"] == null || master["dia_chi"].ToString().Trim() == "")) temp_dia_chi = master["fnote1"].ToString();
                else temp_dia_chi = master["dia_chi"].ToString();
                order_data.address = temp_dia_chi;
                if (master["ma_tinh"] == null || master["ma_tinh"].ToString().Trim() == "") return StatusCode(200, new { status = "err", message = $"Hóa đơn {data.stt_rec} chưa có mã tỉnh" });
                order_data.province = ModelGHTK.GetTenTinh(master["ma_tinh"].ToString()).Trim();
                if (master["ma_quan"] == null || master["ma_quan"].ToString().Trim() == "") return StatusCode(200, new { status = "err", message = $"Hóa đơn {data.stt_rec} chưa có mã quận" });
                order_data.district = ModelGHTK.GetTenQuan(master["ma_quan"].ToString()).Trim();
                if (master["ma_phuong"] == null || master["ma_phuong"].ToString().Trim() == "") return StatusCode(200, new { status = "err", message = $"Hóa đơn {data.stt_rec} chưa có mã phường" });
                order_data.ward = ModelGHTK.GetTenPhuong(master["ma_phuong"].ToString()).Trim();
                order_data.hamlet = "Khác";
                order_data.is_freeship = (data.nguoi_tra == 1? "1":"0");
                int t_thu_ho = Decimal.ToInt32(data.tien_thu_ho);
                order_data.pick_money = t_thu_ho;
                order_data.note = master["stt_rec"].ToString(); 
                order_data.value = (int)data.tien_boi_thuong ;
                order_data.transport = ((int)data.pt_vc == 1 ? "fly": "road");

                send_data.products = new List<GHTK_Data_Product>();
                foreach (DataRow r in ds.Tables[1].Rows)
                {
                    GHTK_Data_Product ite = new GHTK_Data_Product();
                    ite.product_code = r["ma_vt"].ToString().Trim();
                    ite.name = r["ten_vt"].ToString().Trim();
                    ite.quantity = Convert.ToInt32((decimal)r["so_luong"]);
                    ite.price = Convert.ToInt32((decimal)r["gia_nt"]);
                    decimal gg = (decimal)data.trong_luong;
                    ite.weight = gg/1000/ ds.Tables[1].Rows.Count;
                    send_data.products.Add(ite);
                }

                send_data.order = order_data;

                string ma_dvcs = master["ma_dvcs"].ToString();


                var token = _config.GetValue<string>("Vanchuyen:GHTK:privateToken");
                string url = $"https://services.giaohangtietkiem.vn/services/shipment/order/?ver=1.5";
                HttpClientApp client = new HttpClientApp();
                client.AddHeader("Token", token);
                JObject temp = JObject.FromObject(send_data);
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
                if (res.status == "ok")
                {
                    JObject root_data = JObject.Parse(res.data);
                    bool code = (bool)root_data["success"];
                    if (code)
                    {
                        JObject datare = (JObject)root_data["order"];

                        string expected_delivery_time = (string)datare["estimated_deliver_time"];
                        string[] date_split = expected_delivery_time.Split(" ");
                        string date_gh = date_split[date_split.Length - 1];
                        decimal total_fee = (decimal)datare["fee"];
                        decimal insurance_fee = (decimal)datare["insurance_fee"];
                        string order_code = (string)datare["label"];
                        //ghn.addnewm99(data, order, order_code, expected_delivery_time, total_fee, master);

                        Object objre = new
                        {
                            status = "ok",
                            mesage = "",
                            data = new
                            {
                                expected_delivery_time = date_gh,
                                total_fee = total_fee,
                                insurance_fee = insurance_fee,
                                order_ghn_code = order_code,
                            }
                        };
                        ApiLog.log("api_ghn_creat", Request.Headers, logdata.ToString(), objre, "");
                        string sql_up = $"exec ghn_Fastdelivery '{data.stt_rec.Trim()}','{order_code}','{date_gh}'";
                        DAL.DAL_SQL.ExecuteNonquery(sql_up);
                        return Ok(objre);
                    }
                    else
                    {
                        string message = (string)root_data["message"];
                        ApiLog.log("api_ghn_creat", Request.Headers, logdata.ToString(), res.data, "1");
                        string sql_up = $"exec ghn_Fastdelivery '{data.stt_rec.Trim()}','',''";
                        DAL.DAL_SQL.ExecuteNonquery(sql_up);
                        return Ok(new { status = "err", message = message });
                    }
                }
                else
                {
                    ApiLog.log("api_ghn_creat", Request.Headers, logdata.ToString(), res.data, "2");
                    string sql_up = $"exec ghn_Fastdelivery '{data.stt_rec.Trim()}','',''";
                    DAL.DAL_SQL.ExecuteNonquery(sql_up);
                    return Ok(new { status = "err", message = res.data });
                }
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                ApiLog.log("api_ghn_creat", Request.Headers, logdata.ToString(), e.Message, "2");
                string sql_up = $"exec ghn_Fastdelivery '{data.stt_rec.Trim()}','',''";
                DAL.DAL_SQL.ExecuteNonquery(sql_up);
                return Ok(new { status = "err", message = e.Message });
            }
        }
        [HttpPost]
        [Route("WebhooksOrder")]
        public async Task<IActionResult> Webhooks([FromBody] Object data)
        {

            ILog log = Logger.GetLog(typeof(Webhooks));
            log.Info("Cap nhat giao hang nhanh");
            log.Info(Request.Path);
            log.Info(Request.Headers);
            try
            {
                string s = data.ToString();
                JObject obj = JObject.Parse(s);
                log.Info(obj.ToString());
                ApiLog.logval("info_ghn", obj.ToString());
                ModelGHN ghn = new ModelGHN(_config);
                string status = (string)obj["Status"];
                string OrderCode = (string)obj["OrderCode"];
                string ClientOrderCode = (string)obj["ClientOrderCode"];
                string[] list_ma = ClientOrderCode.Split(".");
                double TotalFee = (double)obj["TotalFee"];
                string stt_rec = list_ma[0];
                DateTime time = (DateTime)obj["Time"];
                string t = time.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                string sql = $"exec haravan_UpdateStatusVC '{stt_rec.Trim()}','{status}','{OrderCode.Trim()}',{TotalFee},'{ClientOrderCode}','{t}'";
                ApiLog.logval("info_ghn_sql", sql);
                DAL.DAL_SQL.ExecuteNonquery(sql);

                return Ok("Đồng Bộ thành công");

            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return StatusCode(200, e.Message);
            }
        }

        [HttpPost]
        [Route("UpdateLocation")]
        public async Task<IActionResult> UpdateLocation()
        {
            try
            {
                var token = _config.GetValue<string>("Vanchuyen:GHTK:privateToken");
                string url = $"https://services.giaohangtietkiem.vn//services/shipment/list_pick_add";
                HttpClientApp client = new HttpClientApp();
                client.AddHeader("Token", token);
                ResponseApiHaravan res = await client.Get_Request(url);
                JObject root_data = JObject.Parse(res.data);
                JArray lst_location = (JArray)root_data["data"];

                string sql_up = "";
                string sql = "select * from dmkho where ma_kho like 'TP_%'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                for (int i = 0; i < lst_location.Count; i++)
                {
                    JObject location = (JObject) lst_location[i];
                    string pick_address_id = (string)location["pick_address_id"];
                    string name_root = (string)location["pick_name"];
                    string[] split_name = name_root.Trim().Split(" ");
                    string name = split_name[0].Trim();
                    foreach(DataRow r in ds.Tables[0].Rows)
                    {
                        string ma_dvcs = r["ma_dvcs"].ToString().Trim();
                        string ma_kho = r["ma_kho"].ToString().Trim();
                        if (name == ma_dvcs)
                        {
                            sql_up += Environment.NewLine + $"update dmkho set s2='{pick_address_id.Trim()}' where ma_kho='{ma_kho}'";
                            break;
                        }
                    }
                }
                DAL.DAL_SQL.ExecuteNonquery(sql_up);

                return Ok("Đồng Bộ thành công");

            }
            catch (Exception e)
            {
                return StatusCode(200, e.Message);
            }
        }
    }
}
