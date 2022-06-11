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
    [Route("GHN")]
    [ApiController]
    public class GHN : ControllerBase
    {
        private readonly IConfiguration _config;


        public GHN(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        [Route("GetFeeAndTime")]
        public async Task<IActionResult> GetFeeAndTime([FromBody] GetFee data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);
                if (data.ma_kho == null) return StatusCode(200, new {status = "err", message =  "Bắt buôc phải có mã kho" });
                if (data.pt_vc == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có phương thức vận chuyển" });
                if (data.height == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có trọng lượng" });
                if (data.length == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có chiều dài" });
                if (data.weight == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có chiều rộng" });
                if (data.width == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có chiều cao" });
                if (data.insurance_fee == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có tiền bồi thường" });
                if (data.to_ma_quan == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có mã phường" });
                if (data.to_ma_phuong == null) return StatusCode(200, new { status = "err", message = "Bắt buôc phải có mã phường" });

                ModelGHN ghn = new ModelGHN(_config);
                GHNAddress from_Address = ghn.GetAddressByCuaHang(data.ma_kho);
                if (from_Address.status == "KHONGCOKHO") return StatusCode(200, new { status = "err", message = $"Không tồn tại kho {data.ma_kho}" });
                if (from_Address.ma_tinh == null || from_Address.ma_tinh == "") return StatusCode(200, new { status = "err", message = $"Kho thuộc cửa hàng {data.ma_kho} không có thông tin mã tỉnh" });
                if (from_Address.ma_quan == null || from_Address.ma_quan == "") return StatusCode(200, new { status = "err", message = $"Kho thuộc cửa hàng {data.ma_kho} không có thông tin mã quận" });
                if (from_Address.ma_phuong == null || from_Address.ma_phuong == "") return StatusCode(200, new { status = "err", message = $"Kho thuộc cửa hàng {data.ma_kho} không có thông tin mã phường" });
                int service_id = await ghn.GetService(from_Address.ma_quan,data.to_ma_quan.ToString(), data.pt_vc);

                int ghn_to_ma_quan = ghn.GetGHN_Ma_quan(data.to_ma_quan);
                if (ghn_to_ma_quan == -1 ) return StatusCode(200, new { status = "err", message = $"Không tốn tại mã quận {data.to_ma_quan} trên giao hàng nhanh hoặc trên phần mềm" });
                string ghn_to_ma_phuong = ghn.GetGHN_Ma_phuong(data.to_ma_phuong);
                if(ghn_to_ma_phuong == "") return StatusCode(200, new { status = "err", message = $"Không tốn tại mã phường {data.to_ma_phuong} trên giao hàng nhanh hoặc trên phần mềm" });


                var token = _config.GetValue<string>("Vanchuyen:GHN:privateToken");
                var shopid = _config.GetValue<string>("Vanchuyen:GHN:ShopId");

                string url = $"https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/fee";
                HttpClientApp client = new HttpClientApp();
                client.AddHeader("Token", token);
                client.AddHeader("ShopId", shopid);
                client.AddHeader("Accept", "text/plain");
                Object body = new
                {
                    from_district_id =Convert.ToInt32(from_Address.ma_quan),
                    service_id= service_id,
                    service_type_id=data.pt_vc,
                    to_district_id= ghn_to_ma_quan,
                    to_ward_code= ghn_to_ma_phuong.Trim(),
                    height= data.height,
                    length= data.length,
                    weight= data.weight,
                    width=data.width,
                    insurance_fee=data.insurance_fee,
                    coupon= data.coupon
                };
                JObject temp = JObject.FromObject(body);

                ResponseApiHaravan res = await client.Get_Request_WithBody(url, temp.ToString());
                double total = 0;
                if (res.status == "ok")
                {
                    JObject root_total = JObject.Parse(res.data);
                    total = (double)root_total["data"]["total"];
                }

                Object body2 = new
                {
                    from_district_id =Convert.ToInt32(from_Address.ma_quan),
                    from_ward_code = from_Address.ma_phuong.Trim(),
                    to_district_id = ghn_to_ma_quan,
                    to_ward_code = ghn_to_ma_phuong.Trim(),
                    service_id = service_id,
                };
                JObject temp2 = JObject.FromObject(body2);
                url = $"https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/leadtime";
                ResponseApiHaravan re2s = await client.Get_Request_WithBody(url, temp2.ToString());
                DateTime leadtime = DateTime.Now;
                if (re2s.status == "ok")
                {
                    JObject root_data = JObject.Parse(re2s.data);
                    double totaltime= (double)root_data["data"]["leadtime"];
                    leadtime = (new DateTime(1970, 1, 1)).AddMilliseconds(totaltime * 1000);
                }
                Object re = new
                {
                    status ="ok" ,
                    message = "" ,
                    data = new
                    {
                        total = total,
                        tg_dukien = leadtime
                    }
                };

                return Ok(re);
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(200, e.Message);
            }
        }

        [HttpPost]
        [Route("DongBoDiaChi")]
        public async Task<IActionResult> DongBoDiaChi([FromBody] Object data)
        {
            var constring = _config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            try
            {
                conn.Open();
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);
                string s = data.ToString();
                JObject obj = JObject.Parse(s);
                List<string> lst_tinh = obj["ma_tinh"].ToObject<List<string>>();
                bool auto_insert = (bool)obj["auto_insert"];
                ModelGHN ghn = new ModelGHN(_config);
                string sql = await ghn.SQLDongBo_Tinh(lst_tinh, auto_insert);

                SqlCommand com = new SqlCommand(sql, conn);
                com.ExecuteNonQuery();
                return Ok("Đồng Bộ thành công");
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }


        [HttpPost]
        [Route("HuyGH")]
        public async Task<IActionResult> HuyGiaoHang([FromBody] Re_HuyGH data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);

                ModelGHN ghn = new ModelGHN(_config);
                var shopid = _config.GetValue<string>("Vanchuyen:GHN:ShopId");

                List<string> ds_vd = new List<string>();
                ds_vd.Add(data.ma_vd);

                var token = _config.GetValue<string>("Vanchuyen:GHN:privateToken");
                string url = $"https://online-gateway.ghn.vn/shiip/public-api/v2/switch-status/cancel";
                HttpClientApp client = new HttpClientApp();
                client.AddHeader("Token", token);
                client.AddHeader("ShopId", shopid);
                object order = new
                {
                    order_codes = ds_vd
                };
                JObject temp = JObject.FromObject(order);
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
                if (res.status == "ok")
                {
                    return Ok(new ResponseData("ok","Hủy giao hàng thành công",""));
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
        public async Task<IActionResult> TaoDonGiaoHang([FromBody] Object req_data)
        {
            var constring = _config.GetValue<string>("ConnectionStrings:ConnectionDBAPP");
            SqlConnection conn = new SqlConnection(constring);
            string temp_s = req_data.ToString();
            JObject logdata = JObject.Parse(temp_s);
            ApiLog.log("api_ghn_creat", Request.Headers, logdata.ToString(), "", "-1");

            try
            {               
               
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);

                ModelGHN ghn = new ModelGHN(_config);

                GiaoHang data = ghn.ConvertToGiaoHang(logdata);


                if (data.stt_rec == null || data.stt_rec.Trim() == "") { return StatusCode(200, new { status = "err", message = "Bắt buôc phải có mã hóa đơn" }); }
                if (data.tablesufix == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có table sufix" }); }
                if (!ModelGHN.CheckOrderClose(data.stt_rec.Trim(), data.tablesufix.Trim())) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Đơn phải ở trạng thái đóng đơn trước khi giao hàng" }); }
                if (data.pt_vc == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có phương thức vận chuyển" }); }
                if (!Library.CheckStringNgayThang(data.tablesufix)) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "tablesufix phải có ddingj dạng yyyyMM" }); }
                if (data.chieu_cao == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có trọng lượng" }); }
                if (data.chieu_dai == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có chiều dài" }); }
                if (data.trong_luong == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có chiều rộng" }); }
                if (data.chieu_rong == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có chiều cao" }); }
                if (data.chieu_cao <= 0 || data.chieu_dai <= 0 || data.trong_luong <= 0 || data.chieu_rong <= 0) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Thông tin kích thước gói hàng : chiều cao,chiều rộng,chiều dài,cân nặng phải >0" }); }
                if (data.tien_boi_thuong == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có tiền bồi thường" }); }
                if (data.nguoi_tra == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có người trả" }); }
                if (data.cho_xem_hang == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có trạng thái cho xem hàng" }); }
                if (data.cho_thu_hang == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có trạng thái cho thử hàng" }); }
                if (data.to_name == null || data.to_name.Trim() == "") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có tên người nhận hàng" }); }
                if (data.to_phone == null || data.to_phone.Trim() == "") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có số điện thoại người nhận hàng" }); }
                if (data.tien_thu_ho == null || data.tien_thu_ho < 0) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buộc nhập tiền thu hộ và tiền thu hộ phải >=0" }); }
                //if (!ModelGHN.CheckCodAmont(data.stt_rec,data.tablesufix,data.tien_thu_ho)) return StatusCode(200, new { status = "err", message = "Tiền thu hộ không khớp.Bạn nên Lưu dữ liệu giao hàng trước" });

                Orders or = new Orders(_config);
                ResponseData re = await or.CreatFulliment(data.stt_rec, data.tablesufix);
                if (re.status == "err")
                {
                    ApiLog.logval("err_log_ghn_creatFulliment", re.message);
                }

                string sql_gethdgh = $"select * from m81${data.tablesufix} where stt_rec='{data.stt_rec}' " +
                    $" select t1.ten_vt,t0.* from d81${data.tablesufix} t0 left join dmvt t1 on t0.ma_vt = t1.ma_vt where stt_rec='{data.stt_rec}'  ";
                SqlDataAdapter da = new SqlDataAdapter(sql_gethdgh, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count == 0) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Không tìm thấy hóa đơn stt_rec={data.stt_rec} " }); }
                DataRow master = ds.Tables[0].Rows[0];
                if (ds.Tables[1].Rows.Count == 0) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"hóa đơn {data.stt_rec} chưa nhập sản phẩm để vận chuyển" }); }
                OrderCreatNew order = new OrderCreatNew();


                string ma_ch = master["ma_dvcs"].ToString();

                string ma_kho = ds.Tables[1].Rows[0]["ma_kho"].ToString();
                int t_tt_nt = Convert.ToInt32((decimal)master["t_tt_nt"]);
                //-------------------------------------------------------------------------------------------------------------------------------------
                order.payment_type_id = data.nguoi_tra;
                order.note =master["stt_rec"].ToString();
                //-------------------------------------------------------------------------------------------------------------------------------------
                GHNAddress fromaddess = ghn.GetAddressByKho(ma_kho);
                if (fromaddess.status == "KHONGCOKHO") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Không tồn tại kho {ma_kho}" }); }
                if (fromaddess.ma_tinh == null || fromaddess.ma_tinh == "") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Kho không có thông tin mã tỉnh" }); }
                if (fromaddess.ma_quan == null || fromaddess.ma_quan == "") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Kho không có thông tin mã quận" }); }
                if (fromaddess.ma_phuong == null || fromaddess.ma_phuong == "") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Kho không có thông tin mã phường" }); }
                if (data.cho_xem_hang == 0) order.required_note = "KHONGCHOXEMHANG";
                else
                    if (data.cho_thu_hang == 0) order.required_note = "CHOXEMHANGKHONGTHU";
                else order.required_note = "CHOTHUHANG";
                //-------------------------------------------------------------------------------------------------------------------------------------

                string phone = ghn.GetReturnPhone(ma_ch);
                order.return_name = $"Eva De Eva (${ma_ch})";
                order.return_phone = phone;
                order.return_address = fromaddess.address;
                order.return_district_id = Library.ConvertToInt(fromaddess.ma_quan);
                order.return_ward_code = fromaddess.ma_phuong.Trim();
                //-------------------------------------------------------------------------------------------------------------------------------------
                order.from_name = $"Eva De Eva (${ma_ch})";
                order.from_phone = phone;
                order.from_address = fromaddess.address;
                order.from_district_id = Library.ConvertToInt(fromaddess.ma_quan);
                order.from_ward_code = fromaddess.ma_phuong.Trim();
                //-------------------------------------------------------------------------------------------------------------------------------------
                order.client_order_code = $"{data.stt_rec}.{master["so_ct"].ToString()}.{master["s2"]}.";
                order.to_name = data.to_name;
                order.to_phone =data.to_phone;
                if ((master["dia_chi"] == null || master["dia_chi"].ToString().Trim() == "") && (master["fnote1"] == null || master["fnote1"].ToString().Trim() == ""))
                { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Hóa đơn {data.stt_rec} chưa có thông tin địa chỉ người nhận" }); }
                string temp_dia_chi = "";
                if ((master["dia_chi"] == null || master["dia_chi"].ToString().Trim() == "")) temp_dia_chi = master["fnote1"].ToString();
                else temp_dia_chi = master["dia_chi"].ToString();
                order.to_address = temp_dia_chi;
                if (master["ma_phuong"] == null || master["ma_phuong"].ToString().Trim() == "") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Hóa đơn {data.stt_rec} chưa có mã phường" }); }
                order.to_ward_code = ghn.GetGHN_Ma_phuong(master["ma_phuong"].ToString()).Trim();
                if (order.to_ward_code == "") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Không tồn tại mã phường {master["ma_phuong"].ToString()} trên giao hàng nhanh hoặc trên phần mềm" }); }
                if (master["ma_quan"] == null || master["ma_quan"].ToString().Trim() == "") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Hóa đơn {data.stt_rec} chưa có mã quận" }); }
                order.to_district_id = ghn.GetGHN_Ma_quan(master["ma_quan"].ToString());
                if (order.to_district_id == -1) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Không tồn tại mã quận {master["ma_quan"].ToString()} trên giao hàng nhanh hoặc trên phần mềm" }); }
                //-------------------------------------------------------------------------------------------------------------------------------------

                int t_thu_ho = Decimal.ToInt32(data.tien_thu_ho);

                order.cod_amount = t_thu_ho;
                order.content = order.note;

                order.weight = data.trong_luong;
                order.length = data.chieu_dai;
                order.width = data.chieu_rong; 
                order.height = data.chieu_cao;

                //-------------------------------------------------------------------------------------------------------------------------------------
                order.deliver_station_id = null;
                order.insurance_value = data.tien_boi_thuong;
                order.service_type_id =data.pt_vc;
                order.service_id = await ghn.GetService(order.return_district_id.ToString(), order.to_district_id.ToString(), order.service_type_id);
                decimal order_value = t_tt_nt;
                order.coupon = data.ma_km;
                List<ItemLineGHN> items = new List<ItemLineGHN>();
                foreach (DataRow r in ds.Tables[1].Rows)
                {
                    ItemLineGHN ite = new ItemLineGHN();
                    ite.code = r["ma_vt"].ToString().Trim();
                    ite.name = r["ten_vt"].ToString().Trim() ?? " ";
                    ite.quantity = Convert.ToInt32((decimal)r["so_luong"]);
                    ite.price = Convert.ToInt32((decimal)r["gia_nt"]);
                    items.Add(ite);
                }
                order.items = items;
                string ma_dvcs = master["ma_dvcs"].ToString();
                GHNAddress dia_chi_kho_ghn = ghn.GetAddressByDVCS(ma_dvcs);
                var shopid = _config.GetValue<string>("Vanchuyen:GHN:ShopId");


                var token = _config.GetValue<string>("Vanchuyen:GHN:privateToken");
                string url = $"https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/create";
                HttpClientApp client = new HttpClientApp();
                client.AddHeader("Token", token);
                client.AddHeader("ShopId", shopid);
                JObject temp = JObject.FromObject(order);
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
                if (res.status == "ok")
                {
                    JObject root_data = JObject.Parse(res.data);
                    int code = (int)root_data["code"];
                    if (code == 200)
                    {
                        JObject datare = (JObject)root_data["data"];

                        string expected_delivery_time = (string)datare["expected_delivery_time"];
                        decimal total_fee = (decimal)datare["total_fee"];
                        string order_code = (string)datare["order_code"];
                        ghn.addnewm99(data, order, order_code, expected_delivery_time, total_fee, master);

                        Object objre = new
                        {
                            status = "ok",
                            mesage = "",
                            data = new
                            {
                                expected_delivery_time = expected_delivery_time,
                                total_fee = total_fee,
                                order_ghn_code = order_code,
                            }
                        };
                        ApiLog.log("api_ghn_creat", Request.Headers, logdata.ToString(), objre, "");
                        string sql_up = $"exec ghn_Fastdelivery '{data.stt_rec.Trim()}','{order_code}','{expected_delivery_time}'";
                        DAL.DAL_SQL.ExecuteNonquery(sql_up);
                        return Ok(objre);
                    }
                    else
                    {
                        string message = (string)root_data["message"];
                        ApiLog.log("api_ghn_creat", Request.Headers, logdata.ToString(), res.data, "1");
                        string sql_up = $"exec ghn_Fastdelivery '{data.stt_rec.Trim()}','',''";
                        DAL.DAL_SQL.ExecuteNonquery(sql_up);
                        return Ok( new { status = "err", message = res.data });
                    }
                }
                else
                {
                    ApiLog.log("api_ghn_creat", Request.Headers, logdata.ToString(), res.data, "2");
                    string sql_up = $"exec ghn_Fastdelivery '{data.stt_rec.Trim()}','',''";
                    DAL.DAL_SQL.ExecuteNonquery(sql_up);
                    return Ok( new { status = "err", message = res.data });
                }
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                ApiLog.log("api_ghn_creat", Request.Headers, logdata.ToString(), e.Message, "3");

                string stt_rec = logdata["stt_rec"].ToString().Trim();
                string sql_up = $"exec ghn_Fastdelivery '{stt_rec}','',''";
                DAL.DAL_SQL.ExecuteNonquery(sql_up);
                return Ok( new { status = "err", message = e.Message });
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
        [Route("GetOldStatus")]
        public async Task<IActionResult> GetOldStatus([FromBody] Object data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);

                ModelGHN ghn = new ModelGHN(_config);
                var shopid = _config.GetValue<string>("Vanchuyen:GHN:ShopId");


                var token = _config.GetValue<string>("Vanchuyen:GHN:privateToken");
                string url = $"https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/detail";
                HttpClientApp client = new HttpClientApp();
                client.AddHeader("Token", token);
                client.AddHeader("ShopId", shopid);

                string sql = "";
                string sql_getdata = "select '202108' tbl,code_ghn,stt_rec from m81$202108 where isnull(code_ghn,'')<>''";

                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql_getdata);
                foreach(DataRow r in ds.Tables[0].Rows)
                {
                    string code_ghn = r["code_ghn"].ToString().Trim();
                    string tbl = r["tbl"].ToString().Trim();
                    string stt_rec = r["stt_rec"].ToString().Trim();
                    object order = new
                    {
                        order_code = code_ghn
                    };
                    JObject temp = JObject.FromObject(order);
                    ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
                    if (res.status == "ok")
                    {
                        JObject root_data = JObject.Parse(res.data);
                        if (!Library.IsNullOrEmpty(root_data["data"]))
                        {
                            if (!Library.IsNullOrEmpty(root_data["data"]["status"]))
                            {
                                string status = (string)root_data["data"]["status"] ?? "";
                                sql +=Environment.NewLine + $"update m81${tbl} set status2 = dbo.GetCarrierStatusCode('{status.Trim()}') where stt_rec='{stt_rec}'";
                            }
                        }
                    }
                    else
                    {
                        int kk = 0;
                    }
                }
                int kkss = 1;
                return Ok();

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
