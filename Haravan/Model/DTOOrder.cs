using Haravan.FuncLib;
using Haravan.ModelsApp;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Model
{
    public class DTOOrder
    {
        public static async Task<string>  GetPaybyReceiver(string token,string orderid,string vc_id)
        {
            string url = $"https://apis.haravan.com/com/orders/{orderid}/fulfillments/{vc_id}.json";
            HttpClientApp client = new HttpClientApp(token);
            ResponseApiHaravan res = await client.Get_Request(url);
            JObject data = JObject.Parse(res.data);
            JObject obj = (JObject)data["fulfillment"];
            if (Library.IsNullOrEmpty(obj["note_attributes"])) return "0";
            JArray list_note = (JArray)obj["note_attributes"];
            string check = "0";
            for (int i = 0; i < list_note.Count; i++)
            {
                var note = (JObject)list_note[i];
                string name = (string)note["name"];
                if (name == "Người nhận trả phí")
                {
                    check = "1";
                }
            }
            return check;
        }
        public static bool checkOrder(JObject obj)
        {
            try
            {
                string pos = "";
                if (Library.IsNullOrEmpty(obj["source_name"])) { }
                else
                {
                    pos = (string)obj["source_name"] ?? "";
                }
                if (pos.Trim() == "pos") return false;
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
        public async static Task<String> CreatSqlOrder(JObject obj, string token)
        {
            try
            {
                string str = String.Empty;
                string confrimstatus = ((string)obj["confirmed_status"] ?? "");
                string cancelled_status = ((string)obj["cancelled_status"] ?? "");

                string close_status = ((string)obj["closed_status"] ?? "");

                DateTime myDate = Convert.ToDateTime((string)obj["created_at"] ?? "");
                myDate = myDate.AddHours(7);
                //var easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                //myDate = TimeZoneInfo.ConvertTime(myDate, easternZone);

                string table_sufix = myDate.ToString("yyyyMM", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                int i = 0;
                string ma_dvcs = "CH001";
                string so_ct = ((string)obj["order_number"] ?? "");
                string number_haravan = ((string)obj["id"] ?? "");
                string loai_ct = "CT";
                string ma_gd = "GD";
                string ma_nt = (string)(obj["currency"] ?? "");
                string ngay_ct = myDate.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                string ma_kh_haravan = (string)obj["customer"]["id"] ?? "";
                string ma_kh = "";
                if (Library.IsNullOrEmpty(obj["customer"]["phone"]))
                {
                    JArray add_arr = (JArray)obj["customer"]["addresses"];
                    for (int i_cus = 0; i_cus < add_arr.Count; i_cus++)
                    {
                        if (!Library.IsNullOrEmpty(add_arr[i_cus]["phone"]))
                        {
                            ma_kh = (string)add_arr[i_cus]["phone"] ?? "";
                            break;
                        }
                    }
                }
                else ma_kh = ((string)obj["customer"]["phone"] ?? "");
                string dien_giai = Library.SpecialChracterSql(((string)obj["note"] ?? ""));
                dien_giai = Library.AutoCutStringtooLong("m81$000000", "dien_giai", dien_giai);
                string ma_nvbh = "";
                string ma_ctv = "";
                string dia_chi_gh1 = "";
                string dia_chi_gh2 = "";
                string ma_voucher = "";
                string pos = (string)obj["source_name"] ?? "";
                JArray listvo = (JArray)obj["discount_codes"];
                for (int i_vo = 0; i_vo < listvo.Count - 1; i_vo++)
                {
                    ma_voucher += (string)listvo[i_vo]["code"] + ",";
                }
                if (listvo.Count > 0)
                    ma_voucher += (string)listvo[0]["code"];
                if (ma_voucher.Length >= 128)
                    ma_voucher = ma_voucher.Substring(0, 127);
                double t_tien2 = Library.ConvertToDouble((string)obj["subtotal_price"]);
                double t_tien = Library.ConvertToDouble((string)obj["total_line_items_price"]);
                double t_ck_tt_nt = Library.ConvertToDouble((string)obj["total_discounts"]);
                string status = (string)obj["financial_status"] ?? "";
                if (close_status == "closed") status = "close";
                if (cancelled_status == "cancelled") status = "cancel";
                string carrier_status = "";
                JArray listcarrier = (JArray)obj["fulfillments"];
                double address_lat = 0;
                double address_long = 0;
                double real_shipping_fee = 0;
                string toado = "";
                string paybyreverd = "0";
                string ma_vandon = "";
                if (listcarrier.Count > 0)
                {
                    ma_vandon = (string)listcarrier[0]["tracking_number"] ?? "";
                    carrier_status = (string)listcarrier[0]["carrier_status_code"] ?? "";
                    string vc_id = (string)listcarrier[0]["id"];
                    real_shipping_fee = Library.ConvertToDouble((string)listcarrier[0]["real_shipping_fee"] ?? "");
                    paybyreverd = await GetPaybyReceiver(token, number_haravan, vc_id);
                }
                JArray listReturn = (JArray)obj["refunds"];

                JObject dcgh1 = (JObject)obj["shipping_address"];
                string to_thanh_pho = "";
                string to_quan_huyen = "";
                string to_phuong_xa = "";
                string to_phone = "";
                string to_name = "";
                if (dcgh1 != null)
                {
                    dia_chi_gh1 = Library.SpecialChracterSql(((string)dcgh1["address1"] ?? ""))
                            + " " + Library.SpecialChracterSql(((string)dcgh1["address2"] ?? ""))
                            + " " + Library.SpecialChracterSql(((string)dcgh1["ward"] ?? ""))
                            + " " + Library.SpecialChracterSql(((string)dcgh1["district"] ?? ""))
                        + " " + Library.SpecialChracterSql(((string)dcgh1["province"] ?? ""))
                        + " " + Library.SpecialChracterSql(((string)dcgh1["country"] ?? ""));
                    address_lat = Library.ConvertToDouble((string)dcgh1["to_latitude"]);
                    address_long = Library.ConvertToDouble((string)dcgh1["to_longtitude"]);
                    if (dcgh1["province_code"] != null) to_thanh_pho = DiaChi.getmatinhbyharavan((string)dcgh1["province_code"] ?? "");
                    if (dcgh1["district_code"] != null) to_quan_huyen = DiaChi.getmahuyenbyharavan((string)dcgh1["district_code"] ?? "");
                    if (dcgh1["ward_code"] != null) to_phuong_xa = DiaChi.getmaphuongbyharavan((string)dcgh1["ward_code"] ?? "", (string)dcgh1["ward"] ?? "");
                    if (dcgh1["phone"] != null) to_phone = (string)dcgh1["phone"] ?? "";
                    to_name = (string)dcgh1["name"] ?? "";
                }
                dia_chi_gh1 = Library.AutoCutStringtooLong("m81$000000", "dia_chi", dia_chi_gh1);
                JObject dcgh2 = (JObject)obj["billing_address"];
                if (dcgh2 != null)
                {
                    dia_chi_gh2 = Library.SpecialChracterSql(((string)dcgh2["address1"] ?? ""))
                            + " " + Library.SpecialChracterSql(((string)dcgh2["address2"] ?? ""))
                            + " " + Library.SpecialChracterSql(((string)dcgh2["ward"] ?? ""))
                            + " " + Library.SpecialChracterSql(((string)dcgh2["district"] ?? ""))
                        + " " + Library.SpecialChracterSql(((string)dcgh2["province"] ?? ""))
                        + " " + Library.SpecialChracterSql(((string)dcgh2["country"] ?? ""));
                }
                dia_chi_gh2 = Library.AutoCutStringtooLong("m81$000000", "fnote1", dia_chi_gh2);
                toado = $"{address_lat},{address_long}";
                //------------------------tinh chi phi van chuyen----------------------------
                double t_cp_vc_nt = 0;
                JArray lisst_vc = (JArray)obj["shipping_lines"];
                for (int index2 = 0; index2 < lisst_vc.Count; index2++)
                {
                    var detail_vc = (JObject)lisst_vc[index2];
                    t_cp_vc_nt += Library.ConvertToDouble((string)detail_vc["price"]);
                }
                if (paybyreverd == "1") t_cp_vc_nt = 0;
                else
                {
                    if (real_shipping_fee != 0) t_cp_vc_nt = real_shipping_fee;
                }
                //---------------------------------------------------------------------------
                string delivery_time = "";
                string location_id = (string)obj["location_id"];

                str = "declare @q nvarchar(4000) = ''";

                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#master') IS NOT NULL DROP TABLE #master;";
                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#detail') IS NOT NULL DROP TABLE #detail;";
                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#return') IS NOT NULL DROP TABLE #return;";

                str += Environment.NewLine + "select top 0 * into #master from m81$000000";
                str += Environment.NewLine + "select top 0 * into #detail from d81$000000";
                str += Environment.NewLine + "select top 0 * into #return from d816$000000";
                str += Environment.NewLine;


                str += Environment.NewLine + " insert into #master ( stt_rec, ma_ct, so_ct, ma_dvcs, loai_ct, ma_gd, ngay_ct, ngay_lct, ma_kh, dien_giai, ma_nvbh, ma_kh2, ong_ba, fqty1, ten_vtthue, ghi_chuthue, fnote1, dia_chi,  datetime0, datetime2, user_id0, user_id2,t_ck_tt_nt,t_cp_vc_nt,ma_voucher,s2,ma_nt,so_ct_post,ma_tinh,ma_quan,ma_phuong,job_id,s1,code_ghn,dien_giai_post,ma_gg,fcode1)" + Environment.NewLine +
                            $" select '', 'HDO', '{so_ct}', '{ma_dvcs}', '{loai_ct}', '{ma_gd}', '{ngay_ct}', '{ngay_ct}', '{ma_kh}', N'{dien_giai}', '{ma_nvbh}', '{ma_ctv}', N'', 0, '{to_phone}', '{toado}', N'{dia_chi_gh2}' , N'{dia_chi_gh1}',  getdate(), getdate(), '14', '14',{t_ck_tt_nt},{t_cp_vc_nt},'{ma_voucher}','{number_haravan}','{ma_nt}','{number_haravan}','{to_thanh_pho}','{to_quan_huyen}',N'{to_phuong_xa}','{pos}','{ma_kh_haravan}','{ma_vandon}',N'{to_name}','{ma_voucher}','{location_id}'";
                // Khai báo biến đông:
                //if (obj.addition_property != null)
                //{
                //    string query = "declare ";
                //    foreach (JProperty property in obj.addition_property.Properties())
                //    {
                //        query += String.Format("@{0} nvarchar({1}) = '{2}', ", property.Name, property.Value.ToString().Length + 1, property.Value);
                //    }
                //    query += "@end bit = 0";
                //    str += Environment.NewLine + query;
                //}
                str += Environment.NewLine;
                str += $"DECLARE @status VARCHAR(50) set @status = '{status}'";
                str += Environment.NewLine;
                str += "Update #master set status = dbo.GetStatusOrderOnline(@status)";

                str += Environment.NewLine;
                str += $"DECLARE @carrier VARCHAR(50) set @status = '{carrier_status}'";
                str += Environment.NewLine;
                str += "Update #master set s3 = dbo.GetCarrierStatusCode(@carrier),status2 = dbo.GetCarrierStatusCode(@carrier)";

                JArray listitem = (JArray)obj["line_items"];
                string str_dmckweb = "";
                for (i = 0; i < listitem.Count; i++)
                {
                    var item = (JObject)listitem[i];
                    string ma_vt = ((string)item["barcode"] ?? "");
                    string ma_kho = "";
                    double so_luong = Library.ConvertToDouble((string)item["quantity"]);
                    double gia_nt2 = Library.ConvertToDouble((string)item["price_original"]);
                    double gia_nt = Library.ConvertToDouble((string)item["price"]);
                    double ck_nt = Library.ConvertToDouble((string)item["total_discount"]);
                    double thue_nt = 0;
                    string ds_ma_ck = "";
                    if (Library.IsNullOrEmpty(item["properties"])) { }
                    else
                    {
                        JArray listcode = (JArray)item["properties"];
                        for (int j = 0; j < listcode.Count; j++)
                        {
                            var code = listcode[j];
                            if ((string)code["name"] == "Khuyến mãi")
                            {
                                string val_code = (string)code["value"]??"";
                                string ma_code = Library.GetMaKMFromVAL(val_code).Trim();
                                ds_ma_ck += ma_code;
                                str_dmckweb += Environment.NewLine + $"exec haravan_UpdateDMCK '{ma_code}',N'{Library.GetTenKMFromVAL(val_code, ma_code)}' ";
                            }
                        }
                    }
                    if (ds_ma_ck.Length >= 23)
                        ds_ma_ck = ds_ma_ck.Substring(0, 22);

                    str += Environment.NewLine + " insert into #detail ( stt_rec, stt_rec0, ma_ct, ngay_ct, so_ct, line_nbr, ma_vt, ma_kho, ma_vv, ma_sp, so_luong,gia_nt, gia_nt2, thue_nt,ck_nt,ds_ma_ck,ck_tt_nt,ma_ck)" + Environment.NewLine +
                                $" select '', '{(i + 1).ToString().PadLeft(3, '0')}', 'HDO', '{ngay_ct}', '{so_ct}', {i + 1}, '{ma_vt}', '{ma_kho}', '', '', {so_luong},{gia_nt},{gia_nt2}, {thue_nt},{ck_nt},'{ds_ma_ck}',{ck_nt},'{ds_ma_ck}' ";
                }
                i = 0;

                str += Environment.NewLine + "declare @wsID varchar(1), @stt_rec char(13), @action varchar(10) = 'New' select @wsID = rtrim(val) from options where upper(name) = 'M_WS_ID'";
                str += Environment.NewLine + "create table #idNumber (stt_rec varchar(32))";
                str += Environment.NewLine + "insert into #idNumber exec fs_GetIdentityNumber @wsID, 'HDO', c81$000000";
                str += Environment.NewLine + "select @stt_rec = stt_rec from #idNumber drop table #idNumber";

                str += Environment.NewLine + "update #master set stt_rec = @stt_rec";
                str += Environment.NewLine + "update #detail set stt_rec = @stt_rec";

                str += Environment.NewLine + $"declare @table_sufix varchar(6) = '{table_sufix}'";

                str += Environment.NewLine + $" exec haravan_CreateOrder ";


                str += Environment.NewLine + $"if not exists(select 1 from m81${table_sufix}  where LTRIM(RTRIM(so_ct_post)) = LTRIM(RTRIM('{number_haravan}')))";
                str += Environment.NewLine + "begin";

                str += Environment.NewLine + "insert into m81$" + table_sufix + " select * from #master";
                str += Environment.NewLine + "insert into d81$" + table_sufix + " select * from #detail";

                string str2 = Environment.NewLine + "exec scSSELIB$App$Voucher$UpdateInquiryTable 'HDO', 'i81$@@table_sufix', 'm81$@@table_sufix', 'd81$@@table_sufix', 'stt_rec', @stt_rec, 'ma_kh,loai_ct,ma_nt;#10$,#15$,#20$; , , :ma_kho,ma_vt,tk_vt,tk_gv,tk_dt;#10$,#20$,#30$,#40$,#50$;d81,d81,d81,d81,d81'";
                str2 += Environment.NewLine + "exec scSSELIB$App$Voucher$UpdateGrandTable 'HDO', 'c81$000000', 'm81$@@table_sufix', 'stt_rec', @stt_rec";
                str2 += Environment.NewLine + "exec SSELIB$App$Voucher$UpdateGeneral 'HDO', 'm81$@@table_sufix', 'd81$@@table_sufix', 'i81$@@table_sufix', 'm81$@@table_sufix', @stt_rec";
                str2 = str2.Replace("@@table_sufix", table_sufix);

                str += str2;

                str += Environment.NewLine + $" exec haravan_AfterOrderCreated @stt_rec ,'{table_sufix}'";


                //---------------Tạo phiếu trả hang--------------------------
                //if (listReturn.Count > 0)
                //{
                //    bool check_re = false;
                //    str += Environment.NewLine + $"declare @checkexitre char(1)= '0'";
                //    str += Environment.NewLine + $"declare @linenumber int= 1";
                //    for (int j = 0; j < listReturn.Count; j++)
                //    {
                //        JObject reOrder = (JObject)listReturn[j];
                //        string re_so_ct = (string)reOrder["id"] ?? "";

                //        if (Library.IsNullOrEmpty(reOrder["refund_line_items"])) { }
                //        else
                //        {
                //            check_re = true;
                //            JArray re_Listitems = (JArray)reOrder["refund_line_items"];
                //            str += Environment.NewLine + $"set @checkexitre ='0'";
                //            str += Environment.NewLine + $"select top 1 @checkexitre = '1' from d816${table_sufix} WHERE LTRIM(RTRIM(s1))=LTRIM(RTRIM('{re_so_ct}')) ";
                //            str += Environment.NewLine + $"if (@checkexitre ='0')";
                //            str += Environment.NewLine + $"begin";
                //            for (int k = 0; k < re_Listitems.Count; k++)
                //            {

                //                JObject re_item = (JObject)re_Listitems[k];
                //                string ma_kho = "";
                //                string ma_vt = (string)re_item["line_item_id"] ?? "";

                //                double so_luong = Library.ConvertToDouble((string)re_item["quantity"]);
                //                str += Environment.NewLine +
                //                " insert into #return ( stt_rec, stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,ma_sp,ma_kho,so_luong,s1,s4,stt_rec_hd)";
                //                str += Environment.NewLine +
                //                $" select @stt_rec, '{(k + 1).ToString().PadLeft(3, '0')}','','01/01/2021','','{ma_vt}','','{ma_kho}',{so_luong},'{re_so_ct}',@linenumber,@stt_rec ";
                //                str += Environment.NewLine + $"set @linenumber += 1";
                //            }
                //            str += Environment.NewLine + $"end";
                //        }

                //    }
                //    if (check_re)
                //    {
                //        str += Environment.NewLine + $" declare @total int =0 ";
                //        str += Environment.NewLine + $" select @total =count(stt_rec) from d816${table_sufix} where LTRIM(RTRIM(stt_rec))=LTRIM(RTRIM(@stt_rec)) ";
                //        str += Environment.NewLine + $" exec haravan_CreateReturn @total ";
                //        str += Environment.NewLine + "insert into d816$" + table_sufix + " select * from #return";

                //        str += Environment.NewLine + $" exec haravan_AfterReturnCreated '{table_sufix}',@stt_rec,'{ngay_ct}' ";
                //    }
                //}
                
                str += Environment.NewLine + "end";
                //str += Environment.NewLine + $" exec haravan_UpdateManv @stt_rec,'{ngay_ct}' ";
                str += Environment.NewLine + str_dmckweb;
                return str;
            }
            catch (Exception e)
            {
                string number_haravan = ((string)obj["id"] ?? "");
                ApiLog.log("error/Update order/sql creat", "", e.Message, e.ToString(),number_haravan);
                return "";
            }
        }
        public async static Task<String> UpdateSqlOrder(JObject obj,string token,string table_sufix)
        {
            try 
            { 
                string confrimstatus = ((string)obj["confirmed_status"] ?? "");
                string cancelled_status = ((string)obj["cancelled_status"] ?? "");
                string close_status = ((string)obj["closed_status"] ?? "");
                string str = String.Empty;

                DateTime myDate = Convert.ToDateTime((string)obj["created_at"] ?? "");
                myDate = myDate.AddHours(7);
                //var easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                //myDate = TimeZoneInfo.ConvertTime(myDate, easternZone);

                int i = 0;
                string ma_dvcs = "18";
                string so_ct = ((string)obj["order_number"] ?? "");
                string ngay_ct = myDate.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                string id = ((string)obj["id"] ?? "");
                string paybyreverd = "0";

                string status = (string)obj["financial_status"] ?? "";
                if (close_status == "closed") status = "close";
                if (cancelled_status == "cancelled") status = "cancelled";
                string carrier_status = "";
                JArray listcarrier = (JArray)obj["fulfillments"];
                double address_lat = 0;
                double address_long = 0;
                double real_shipping_fee = 0;
                string toado = "";
                if (listcarrier.Count > 0)
                {
                    carrier_status = (string)listcarrier[0]["carrier_status_code"];
                    string vc_id = (string)listcarrier[0]["id"];
                    real_shipping_fee = Library.ConvertToDouble((string)listcarrier[0]["real_shipping_fee"] ?? "");
                    paybyreverd = await GetPaybyReceiver(token, id, vc_id);
                }

                JObject dcgh1 = (JObject)obj["shipping_address"];

                string dien_giai = ((string)obj["note"] ?? "");
                dien_giai = Library.AutoCutStringtooLong("m81$000000", "dien_giai", dien_giai);

                string dia_chi_gh1 = "";
                string dia_chi_gh2 = "";
                string to_thanh_pho = "";
                string to_quan_huyen = "";
                string to_phuong_xa = "";
                string to_phone = "";
                string to_name = "";
                if (dcgh1 != null)
                {
                    dia_chi_gh1 = Library.SpecialChracterSql(((string)dcgh1["address1"] ?? ""))
                            + " " + Library.SpecialChracterSql(((string)dcgh1["address2"] ?? ""))
                            + " " + Library.SpecialChracterSql(((string)dcgh1["ward"] ?? ""))
                            + " " + Library.SpecialChracterSql(((string)dcgh1["district"] ?? ""))
                        + " " + Library.SpecialChracterSql(((string)dcgh1["province"] ?? ""))
                        + " " + Library.SpecialChracterSql(((string)dcgh1["country"] ?? ""));
                    address_lat = Library.ConvertToDouble((string)dcgh1["to_latitude"]);
                    address_long = Library.ConvertToDouble((string)dcgh1["to_longtitude"]);
                    if (dcgh1["province_code"] != null) to_thanh_pho =DiaChi.getmatinhbyharavan((string)dcgh1["province_code"] ?? "" );
                    if (dcgh1["district_code"] != null) to_quan_huyen =DiaChi.getmahuyenbyharavan((string)dcgh1["district_code"] ?? "" );
                    if (dcgh1["ward_code"] != null) to_phuong_xa = DiaChi.getmaphuongbyharavan((string)dcgh1["ward_code"] ?? "" , (string)dcgh1["ward"] ?? "");
                    if (dcgh1["phone"] != null) to_phone = (string)dcgh1["phone"] ?? "" ;
                    to_name = (string)dcgh1["name"] ?? "";
                }
                dia_chi_gh1 = Library.AutoCutStringtooLong("m81$000000", "dia_chi", dia_chi_gh1);
                JObject dcgh2 = (JObject)obj["billing_address"];
                if (dcgh2 != null)
                {
                    dia_chi_gh2 = Library.SpecialChracterSql(((string)dcgh2["address1"] ?? ""))
                            + " " + Library.SpecialChracterSql(((string)dcgh2["address2"] ?? ""))
                            + " " + Library.SpecialChracterSql(((string)dcgh2["ward"] ?? ""))
                            + " " + Library.SpecialChracterSql(((string)dcgh2["district"] ?? ""))
                        + " " + Library.SpecialChracterSql(((string)dcgh2["province"] ?? ""))
                        + " " + Library.SpecialChracterSql(((string)dcgh2["country"] ?? ""));
                }
                dia_chi_gh2 = Library.AutoCutStringtooLong("m81$000000", "fnote1", dia_chi_gh2);
                toado = $"{address_lat},{address_long}";


                //------------------------tinh chi phi van chuyen----------------------------
                double t_cp_vc_nt = 0;
                JArray lisst_vc = (JArray)obj["shipping_lines"];
                for (int index2 = 0; index2 < lisst_vc.Count; index2++)
                {
                    var detail_vc = (JObject)lisst_vc[index2];
                    t_cp_vc_nt += Library.ConvertToDouble((string)detail_vc["price"]);
                }
                if (paybyreverd == "1") t_cp_vc_nt = 0;
                else
                {
                    if (real_shipping_fee != 0) t_cp_vc_nt = real_shipping_fee;
                }

                //---------------------------------------------------------------------------
                string delivery_time = "";

                str = "declare @q nvarchar(4000) = ''";

                str += Environment.NewLine;
                str += $"DECLARE @status VARCHAR(50) set @status = '{status}'";
                str += $"DECLARE @m_stt_rec char(13)='' ";
                str += $"DECLARE @m_ngay_ct smalldatetime ";
                str += $"select top 1 @m_stt_rec = stt_rec,@m_ngay_ct = ngay_ct from   m81${table_sufix} where LTRIM(RTRIM(so_ct_post)) = LTRIM(RTRIM('{id}')) ";

                str += Environment.NewLine;
                str += $"Update m81${table_sufix} set ghi_chuthue = '{toado}',fnote1 =N'{dia_chi_gh2}',dia_chi=N'{dia_chi_gh1}'  where LTRIM(RTRIM(stt_rec)) = LTRIM(RTRIM(@m_stt_rec)) ";
                str += $"Update m81${table_sufix} set ma_tinh ='{to_thanh_pho}',ma_quan ='{to_quan_huyen}',ma_phuong = N'{to_phuong_xa}',ten_vtthue = '{to_phone}',dien_giai_post=N'{to_name}' , dien_giai=N'{dien_giai}'  where LTRIM(RTRIM(stt_rec)) = LTRIM(RTRIM(@m_stt_rec)) ";

                str += Environment.NewLine;
                str += $"DECLARE @carrier VARCHAR(50) set @carrier = '{carrier_status}'";
                str += Environment.NewLine;

                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#master') IS NOT NULL DROP TABLE #master;";
                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#detail') IS NOT NULL DROP TABLE #detail;";
                str += Environment.NewLine + "IF OBJECT_ID('TempDb..#return') IS NOT NULL DROP TABLE #return;";

                str += Environment.NewLine + $"select top 1 * into #master from m81${table_sufix} where LTRIM(RTRIM(stt_rec)) = LTRIM(RTRIM(@m_stt_rec)) ";
                str += Environment.NewLine + $"select  * into #detail from d81${table_sufix} where LTRIM(RTRIM(stt_rec)) = LTRIM(RTRIM(@m_stt_rec))";
                str += Environment.NewLine + $"select top 0 * into #return from d816$000000";
                str += Environment.NewLine;

                JArray listReturn = (JArray)obj["refunds"];
                if (listReturn.Count > 0)
                {
                    string ordernumbers = obj["order_number"].ToString();
                    str += Environment.NewLine + $"exec haravan_cancel_order2 '{ordernumbers}'";
                }


                    //---------------Tạo phiếu trả hang--------------------------
                    //JArray listReturn = (JArray)obj["refunds"];
                    //if (listReturn.Count > 0)
                    //{
                    //    bool check_re = false;
                    //    str += Environment.NewLine + $"declare @checkexitre char(1)= '0'";
                    //    str += Environment.NewLine + $"declare @linenumber int= 1";
                    //    for (int j = 0; j < listReturn.Count; j++)
                    //    {
                    //        JObject reOrder = (JObject)listReturn[j];
                    //        string re_so_ct = (string)reOrder["id"] ?? "";
                    //        if (Library.IsNullOrEmpty(reOrder["refund_line_items"])) { }
                    //        else
                    //        {
                    //            JArray re_Listitems = (JArray)reOrder["refund_line_items"];
                    //            str += Environment.NewLine + $"set @checkexitre ='0'";
                    //            str += Environment.NewLine + $"select top 1 @checkexitre = '1' from d816${table_sufix} WHERE LTRIM(RTRIM(s1))=LTRIM(RTRIM('{re_so_ct}')) ";
                    //            str += Environment.NewLine + $"if (@checkexitre ='0')";
                    //            str += Environment.NewLine + $"begin";
                    //            for (int k = 0; k < re_Listitems.Count; k++)
                    //            {

                    //                JObject re_item = (JObject)re_Listitems[k];
                    //                string ma_kho = "";
                    //                string ma_vt = (string)re_item["line_item_id"] ?? "";

                    //                double so_luong = Library.ConvertToDouble((string)re_item["quantity"]);
                    //                str += Environment.NewLine +
                    //               " insert into #return ( stt_rec, stt_rec0,ma_ct,ngay_ct,so_ct,ma_vt,ma_sp,ma_kho,so_luong,s1,s4,stt_rec_hd)";
                    //                str += Environment.NewLine +
                    //                $" select @m_stt_rec, '{(k + 1).ToString().PadLeft(3, '0')}','','01/01/2021','','{ma_vt}','','{ma_kho}',{so_luong},'{re_so_ct}',@linenumber,@m_stt_rec ";
                    //                str += Environment.NewLine + $"set @linenumber += 1";
                    //            }
                    //            str += Environment.NewLine + $"end";
                    //        }
                    //    }
                    //    if (check_re)
                    //    {


                    //        str += Environment.NewLine + $" declare @total int =0 ";
                    //        str += Environment.NewLine + $" select @total =count(stt_rec) from d816$202104 where LTRIM(RTRIM(stt_rec))=LTRIM(RTRIM(@m_stt_rec)) ";
                    //        str += Environment.NewLine + $" exec haravan_CreateReturn @total ";


                    //        str += Environment.NewLine + "insert into d816$" + table_sufix + " select * from #return";


                    //        str += Environment.NewLine + $" exec haravan_AfterReturnCreated '{table_sufix}',@m_stt_rec,@m_ngay_ct ";
                    //    }
                    //}

                    str += Environment.NewLine + $"exec haravan_AfterOrderUpdate '{table_sufix}',@m_stt_rec";
                return str;
            }
            catch (Exception e)
            {
                string number_haravan = ((string)obj["id"] ?? "");
                ApiLog.log("error/Update order/sql update", "", e.Message, e.ToString(), number_haravan);
                return "";
            }
        }

    }
}
