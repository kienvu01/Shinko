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
    [Route("ALP")]
    [ApiController]
    public class ALP : ControllerBase
    {
        private readonly IConfiguration _config;
        public ALP(IConfiguration config)
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

                var token = MyAppData.ALP_token;
                if (token == "")
                {
                    var ss = await ModelALP.GetToken();
                    token = ss.token;
                }
                string url = $"http://cusapi.shipnhanh.vn/api/Shipment/Delete";
                HttpClientApp client = new HttpClientApp(token);
                Object obj = new
                {
                    id = data.ma_vd
                };
                JObject temp = JObject.FromObject(obj);
                ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
                if (res.status == "ok")
                {
                    return Ok(new ResponseData("ok", "Hủy giao hàng thành công", ""));
                }
                else
                {
                    var ss = await ModelALP.GetToken();
                    token = ss.token;
                    client = new HttpClientApp(token);
                    res = await client.Post_Request_WithBody(url, temp.ToString());
                    if (res.status =="ok") return Ok(new ResponseData("ok", "Hủy giao hàng thành công", ""));
                    else
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
            string temp_s = req_data.ToString();
            JObject logdata = JObject.Parse(temp_s);

            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);


                ModelGHN ghn = new ModelGHN(_config);
                GiaoHang data = ghn.ConvertToGiaoHang(logdata);


                if (data.stt_rec == null || data.stt_rec.Trim() == "") return StatusCode(200, new { status = "err", message = "Bắt buôc phải có mã hóa đơn" });
                if (data.tablesufix == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có table sufix" }); }
                //if (!ModelGHN.CheckOrderClose(data.stt_rec.Trim(), data.tablesufix.Trim())) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Đơn phải ở trạng thái đóng đơn trước khi giao hàng" }); }
                if (data.pt_vc == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có phương thức vận chuyển" }); }
                if (!Library.CheckStringNgayThang(data.tablesufix)) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "tablesufix phải có ddingj dạng yyyyMM" }); }

                if (data.trong_luong == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có chiều rộng" }); }

                if (data.trong_luong <= 0) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Thông tin kích thước gói hàng : chiều cao,chiều rộng,chiều dài,cân nặng phải >0" }); }
                if (data.tien_boi_thuong == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có tiền bồi thường" }); }
                if (data.nguoi_tra == null) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có người trả" }); }

                if (data.to_name == null || data.to_name.Trim() == "") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có tên người nhận hàng" }); }
                if (data.to_phone == null || data.to_phone.Trim() == "") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buôc phải có số điện thoại người nhận hàng" }); }
                if (data.tien_thu_ho == null || data.tien_thu_ho < 0) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = "Bắt buộc nhập tiền thu hộ và tiền thu hộ phải >=0" }); }
                //if (!ModelGHN.CheckCodAmont(data.stt_rec,data.tablesufix,data.tien_thu_ho)) return StatusCode(200, new { status = "err", message = "Tiền thu hộ không khớp.Bạn nên Lưu dữ liệu giao hàng trước" });

                Orders or = new Orders(_config);
                ResponseData re = await or.CreatFulliment(data.stt_rec, data.tablesufix);
                if (re.status == "err")
                {
                    ApiLog.logval("err_log_ghtk_creatFulliment", re.message);
                }

                string sql_gethdgh = $"select * from m81${data.tablesufix} where stt_rec='{data.stt_rec}' " +
                    $" select t1.ten_vt,t0.* from d81${data.tablesufix} t0 left join dmvt t1 on t0.ma_vt = t1.ma_vt where stt_rec='{data.stt_rec}'  ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql_gethdgh);
                if (ds.Tables[0].Rows.Count == 0) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Không tìm thấy hóa đơn stt_rec={data.stt_rec} " }); }
                DataRow master = ds.Tables[0].Rows[0];
                if (ds.Tables[1].Rows.Count == 0) { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"hóa đơn {data.stt_rec} chưa nhập sản phẩm để vận chuyển" }); }


                ALP_Data send_data = new ALP_Data();

                string ma_ch = master["ma_dvcs"].ToString();
                string phone = ghn.GetReturnPhone(ma_ch);

                send_data.ShopCode = master["so_ct"].ToString().Trim();
                send_data.PaymentTypeId = 2;
                send_data.StructureId = 7;
               
                send_data.ServiceDVGTIds = new List<string>();
                send_data.Weight = (int)data.trong_luong;

                string ma_kho = ds.Tables[1].Rows[0]["ma_kho"].ToString();
                ALP_Location return_address = ModelALP.GetAdressLocation(ma_kho);
                if (return_address.status == "KHONGCOKHO") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Không tồn tại địa chỉ kho {ma_kho}" }); }

                send_data.PickingAddress = return_address.From_ShippingAddress;
                send_data.FromProvinceId = return_address.From_ProvinceId;
                send_data.FromDistrictId = return_address.From_DistrictId;
                send_data.FromWardId = return_address.From_WardId;
                send_data.SenderName = $"Eva De Eva ({ma_ch})"; ;
                send_data.SenderPhone = phone;
                send_data.AddressNoteFrom = return_address.AddressNoteFrom;

                string temp_dia_chi = "";
                if ((master["dia_chi"] == null || master["dia_chi"].ToString().Trim() == "")) temp_dia_chi = master["fnote1"].ToString();
                else temp_dia_chi = master["dia_chi"].ToString();

                if ((master["dia_chi"] == null || master["dia_chi"].ToString().Trim() == "")) temp_dia_chi = master["fnote1"].ToString();
                if (master["ma_quan"] == null || master["ma_quan"].ToString().Trim() == "") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Hóa đơn {data.stt_rec} chưa có mã quận" }); }
                if (master["ma_phuong"] == null || master["ma_phuong"].ToString().Trim() == "") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Hóa đơn {data.stt_rec} chưa có mã phường" }); }
                ALP_Location to_address = ModelALP.GetAdressToLocation(master["ma_phuong"].ToString().Trim());

                send_data.ShippingAddress = to_address.From_ShippingAddress;
                send_data.ProvinceId = to_address.From_ProvinceId;
                send_data.DistrictId = to_address.From_DistrictId;
                send_data.WardId = to_address.From_WardId;
                send_data.HubId = to_address.From_HubId;
                send_data.ReceiverName = data.to_name;
                send_data.ReceiverPhone = data.to_phone;
                send_data.AddressNoteTo = temp_dia_chi;
                send_data.COD = data.tien_thu_ho;
                send_data.Insured = (int)data.tien_boi_thuong;
                send_data.CusNote = "";
                send_data.Content = "";

                string ma_dvcs = master["ma_dvcs"].ToString();
                int totalItem = ds.Tables[1].Rows.Count;
                int tl= (int) data.trong_luong;
                ALP_ServiceId ser = await ModelALP.GetServiceId(return_address.From_DistrictId,return_address.From_WardId, to_address.From_DistrictId, tl, totalItem, 7);
                if (ser.status == "ERR") { ModelGHN.UpdateStatusOrder(data.stt_rec); return StatusCode(200, new { status = "err", message = $"Không có Service phù hợp cho đơn hàng" }); }
                send_data.ServiceId = ser.id;



                ResponseApiHaravan res = await ModelALP.CreatNewOrder(send_data);
                if (res.status == "ok")
                {
                    JObject root_data = JObject.Parse(res.data);
                    int code = (int)root_data["isSuccess"];
                    if (code ==1)
                    {
                        JObject datare = (JObject)root_data["data"];

                        //string expected_delivery_time = (string)datare["estimated_deliver_time"];

                        decimal total_fee = (decimal)datare["totalPrice"];
                        string order_code = (string)datare["shipmentNumber"];
                        string id = (string)datare["id"];
                        //ghn.addnewm99(data, order, order_code, expected_delivery_time, total_fee, master);

                        Object objre = new
                        {
                            status = "ok",
                            mesage = "",
                            data = new
                            {
                                total_fee = total_fee,
                                order_ghn_code = order_code,
                            }
                        };
                        ApiLog.log("api_alp_creat", Request.Headers, logdata.ToString(), objre, "");
                        string sql_up = $"exec ghn_Fastdelivery '{data.stt_rec.Trim()}','{order_code}',''";
                        sql_up += Environment.NewLine + $"insert into alp_code values({id},'{order_code}','','{data.stt_rec}')";
                        DAL.DAL_SQL.ExecuteNonquery(sql_up);
                        return Ok(objre);
                    }
                    else
                    {
                        string message = (string)root_data["message"];
                        ApiLog.log("api_alp_creat", Request.Headers, logdata.ToString(), res.data, "1");
                        string sql_up = $"exec ghn_Fastdelivery '{data.stt_rec.Trim()}','',''";
                        DAL.DAL_SQL.ExecuteNonquery(sql_up);
                        return Ok(new { status = "err", message = message });
                    }
                }
                else
                {
                    ApiLog.log("api_alp_creat", Request.Headers, logdata.ToString(), res.data, "2");
                    string sql_up = $"exec ghn_Fastdelivery '{data.stt_rec.Trim()}','',''";
                    DAL.DAL_SQL.ExecuteNonquery(sql_up);
                    return Ok(new { status = "err", message = res.data });
                }
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                ApiLog.log("api_alp_creat", Request.Headers, logdata.ToString(), e.Message, "3");
                //string sql_up = $"exec ghn_Fastdelivery '{data.stt_rec.Trim()}','',''";
                //DAL.DAL_SQL.ExecuteNonquery(sql_up);
                return Ok(new { status = "err", message = e.Message });
            }
        }

        [HttpPost]
        [Route("DongBoDiaChi")]
        public async Task<IActionResult> DongBoDiaChi()
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);


                string sql_dc = $"select a.ten_phuong+','+b.ten_quan+','+c.ten_tinh as dia_chi,a.ma_tinh,a.ma_quan,a.ma_phuong from hrdmphuong a left join hrdmquan b on a.ma_quan = b.ma_quan left join hrdmtinh c on a.ma_tinh = c.ma_tinh";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql_dc);

                string sql_db = "";
                var token = _config.GetValue<string>("Vanchuyen:GHN:privateToken");
                string url = $"http://coreapi.shipnhanh.vn/api/hub/GetInfoLocationByAddress?address=";
                HttpClientApp client = new HttpClientApp(token);

                int count_err1 = 0;
                int count_err2 = 0;
                int count_ok = 0;

                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    string dia_chi = r["dia_chi"].ToString();
                    string ma_tinh = r["ma_tinh"].ToString().Trim();
                    string ma_quan = r["ma_quan"].ToString().Trim();
                    string ma_phuong = r["ma_phuong"].ToString().Trim();
                    url = $"http://coreapi.shipnhanh.vn/api/hub/GetInfoLocationByAddress?address={dia_chi}";
                    ResponseApiHaravan res = await client.Get_Request(url);
                    if (res.status == "ok")
                    {
                        JObject root_data = JObject.Parse(res.data);
                        int isSuccess = (int)root_data["isSuccess"];
                        if(isSuccess == 1)
                        {
                            if (Library.IsNullOrEmpty(root_data["data"]))
                            {
                                count_err1++;
                            }
                            else
                            {
                                try
                                {
                                    JObject re_data = (JObject)root_data["data"];
                                    string alp_ma_tinh = (string)re_data["provinceId"]??"";
                                    string alp_ma_quan = (string)re_data["districtId"]??"";
                                    string alp_ma_phuong = (string)re_data["wardId"]??"";
                                    string alp_hubid = (string)re_data["hubId"]??"";
                                    sql_db += Environment.NewLine + $"update hrdmtinh set alp_ma_tinh ='{alp_ma_tinh.Trim()}' where ma_tinh='{ma_tinh}'";
                                    sql_db += Environment.NewLine + $"update hrdmquan set alp_ma_quan ='{alp_ma_quan.Trim()}' where ma_quan='{ma_quan}'";
                                    sql_db += Environment.NewLine + $"update hrdmphuong set alp_ma_phuong ='{alp_ma_phuong.Trim()}',alp_hubid='{alp_hubid.Trim()}' where ma_phuong='{ma_phuong}'";
                                    count_ok++;
                                }
                                catch (Exception e)
                                {
                                    int zzzz = 0;
                                    count_err1++;
                                };
                                

                            }                       
                        }
                        else
                        {
                            int zzzz = 0;
                            count_err2++;
                        }
                        
                    }
                    else
                    {
                        int zzzz = 0;
                    }
                }
                DAL.DAL_SQL.ExecuteNonquery(sql_db);
                object re_obj = new
                {
                    count_err1 = count_err1,
                    count_err2 = count_err2,
                    count_ok = count_ok
                };
                return Ok(re_obj);
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost]
        [Route("GetOrderCode")]
        public async Task<IActionResult> GetOrderCode()
        {
            try
            {

                string sql_dc = $"SELECT status,* FROM m81$202109 WHERE ISNULL(code_ghn,'')='' AND ma_ct='HDO' AND status<>4 and status <>5";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql_dc);
                string sql_update = "";
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    string so_ct = r["so_ct"].ToString().Trim();
                    var token = MyAppData.ALP_token;
                    if (token == "")
                    {
                        var ss = await ModelALP.GetToken();
                        token = ss.token;
                    }
                    string url = $"http://cusapi.shipnhanh.vn/api/Shipment/GetShopCodeByShipmentNumber?shopcode={so_ct}";
                    HttpClientApp client = new HttpClientApp(token);
                    ResponseApiHaravan res = await client.Get_Request(url);
                    if (res.status == "ok")
                    {
                        JObject root_data = JObject.Parse(res.data);
                        int code = (int)root_data["isSuccess"];
                        if (code == 1)
                        {
                            JObject datare = (JObject)root_data["data"];
                            string ordercode = (string)datare["shipmentNumber"];
                            sql_update += Environment.NewLine + $"update m81$202109 set code_ghn = '{ordercode}' where rtrim(ltrim(so_ct))='{so_ct}'";
                        }
                    }
                }
                DAL.DAL_SQL.ExecuteNonquery(sql_update);
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
