using log4net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using Haravan.FuncLib;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;
using Haravan.Haravan.Model;
using Haravan.Haravan.Common;

namespace Haravan.Haravan.DataAcess
{
    public class Customers
    {
        public ResponseData customers_create(Object data)
        {
            try
            {
                string temp = data.ToString();
                JObject obj = JObject.Parse(temp);
                UpdateCustomer(obj);
                UpdatediemSo(obj);
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err",e.Message,"");
            }     
        }
        public ResponseData customers_update(Object data)
        {
            try
            {
                string temp = data.ToString();
                JObject obj = JObject.Parse(temp);
                UpdateCustomer(obj);
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        public Object customers_enable(Object data)
        {
            Object re = new { };
            return re;
        }
        public Object customers_disable(Object data)
        {
            Object re = new { };
            return re;
        }
        public ResponseData customers_delete(Object data)
        {
            try
            {
                string temp = data.ToString();
                JObject obj = JObject.Parse(temp);
                DeleteCustomer((string)obj["id"] ?? "");
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
        
        public async Task<ResponseData> UpdateAllCustomer(string fromDate , string toDate,string phone)
        {
            try
            {
                var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");
                bool check = true;
                int page = 0;
                while (check)
                {
                    page += 1;
                    string url = $"https://apis.haravan.com/com/customers.json?updated_at_min={fromDate}&updated_at_max={toDate}&page={page}&limit=50";
                    if (phone == null || phone.Trim() == "") { } else
                    {
                        url += $"&query=filter=(phone:customer in {phone})";
                    }
                    HttpClientApp client = new HttpClientApp(token);
                    ResponseApiHaravan res = await client.Get_Request(url);
                    JObject data = JObject.Parse(res.data);
                    JArray arrCustomers = (JArray)data["customers"];
                    for (int i = 0; i < arrCustomers.Count; i++)
                    {
                        JObject obj = (JObject)arrCustomers[i];
                        CreatNewKH(obj);                                  
                    }
                    if (arrCustomers.Count >= 50)
                        check = true;
                    else  check = false; 
                }
                return new ResponseData("ok", "", "");
            }
            catch (Exception e)
            {
                ApiLog.log("err-UpdateAllCustomer", e.Message, "", "", "");
                return new ResponseData("err", e.Message, "");
            }
          
        }

        //------------------------------------  DTO  ---------------------------------------------------
        //------------------------------------  DTO  ---------------------------------------------------
        public void AddCustomer(JObject kh)
        {
            try
            {
                CustomerInfo cus = GetAddressByHaravan(kh);
                if (cus.phone != "")
                {
                    JArray arrAddress = (JArray)kh["addresses"];
                    string diachi1 = "";
                    string diachi2 = "";
                    if (arrAddress.Count > 0)
                    {
                        var item = (JObject)arrAddress[0];
                        diachi1 = ((string)item["address1"] ?? "")
                            + " " + ((string)item["address2"] ?? "")
                            + " " + ((string)item["ward"] ?? "")
                            + " " + ((string)item["district"] ?? "")
                        + " " + ((string)item["province"] ?? "")
                        + " " + ((string)item["country"] ?? "");
                        diachi1 = Library.AutoCutStringtooLong("dmkh", "dia_chi1", diachi1);
                    }
                    if (arrAddress.Count > 1)
                    {
                        var item = (JObject)arrAddress[1];
                        diachi2 = ((string)item["address1"] ?? "")
                            + " " + ((string)item["address2"] ?? "")
                            + " " + ((string)item["ward"] ?? "")
                            + " " + ((string)item["district"] ?? "")
                        + " " + ((string)item["province"] ?? "")
                        + " " + ((string)item["country"] ?? "");
                        diachi2 = Library.AutoCutStringtooLong("dmkh", "dia_chi2", diachi2);
                    }
                    double gioitinh = Library.ConvertToDouble((string)kh["gender"] ?? "");
                    string email= ((string)kh["email"] ?? "");
                    string note= ((string)kh["note"] ?? "");
                    string brithday = "NULL";
                    if (!Library.IsNullOrEmpty(kh["birthday"]))
                    {
                        brithday = "'" + Library.ConvertSmallDatetime(((string)kh["birthday"] ?? "")).ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "'";
                    }

                    string sql_run = $"insert into dmkh(ma_kh,ten_kh,ten_kh2,kh_yn,cc_yn,nv_yn,dia_chi,dien_thoai,e_mail,ong_ba,ma_nvbh,tinh_thanh,ghi_chu,";
                    sql_run+=Environment.NewLine + $"t_tien_cn,t_tien_hd,ngay_gh,status,tien,t_tien,diem_so,t_diem_so,gioi_tinh,ngay_sinh,dia_chi1,dia_chi2,quan_huyen,phuong_xa)";
                    sql_run += Environment.NewLine + $"values('{cus.phone}', N'{cus.ten_kh}', N'{cus.ten_kh}', 1, 1, 1, N'{cus.diachi}', N'{cus.phone}', N'{email}', N'', '','{cus.tinh_thanh}', N'{note}',";
                    sql_run += Environment.NewLine + $"0, 0, GETDATE(), '1', 0, 0, 0, 0, {gioitinh}, {brithday}, N'{diachi1}',N'{diachi2}' , '{cus.quan_huyen}', '{cus.phuong_xa}'); ";
                    int i = DAL.DAL_SQL.ExecuteNonquery(sql_run);                  
                }               
            }
            catch (Exception e)
            {
                ApiLog.logval("error-AddCustomer", e.Message);
                ILog log = Logger.GetLog(typeof(Customers));
                log.Error(e.Message);
            }
         
        }
        //Update Customer when get 1 customer
        public void UpdateCustomer(JObject kh)
        {
            try
            {
                CustomerInfo cus = GetAddressByHaravan(kh);
                if (cus.phone != "")
                {
                    string s = $"select * from dmkh where ma_kh = '{cus.phone.Trim()}' ";
                    DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(s);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        JArray arrAddress = (JArray)kh["addresses"];
                        string diachi1 = "";
                        string diachi2 = "";
                        if (arrAddress.Count > 0)
                        {
                            var item = (JObject)arrAddress[0];
                            diachi1 = ((string)item["address1"] ?? "")
                                + " " + ((string)item["address2"] ?? "")
                                + " " + ((string)item["ward"] ?? "")
                                + " " + ((string)item["district"] ?? "")
                            + " " + ((string)item["province"] ?? "")
                            + " " + ((string)item["country"] ?? "");
                            diachi1 = Library.AutoCutStringtooLong("dmkh", "dia_chi1", diachi1);
                        }
                        if (arrAddress.Count > 1)
                        {
                            var item = (JObject)arrAddress[1];
                            diachi2 = ((string)item["address1"] ?? "")
                                + " " + ((string)item["address2"] ?? "")
                                + " " + ((string)item["ward"] ?? "")
                                + " " + ((string)item["district"] ?? "")
                            + " " + ((string)item["province"] ?? "")
                            + " " + ((string)item["country"] ?? "");
                            diachi2 = Library.AutoCutStringtooLong("dmkh", "dia_chi2", diachi2);
                        }
                        double gioitinh = Library.ConvertToDouble((string)kh["gender"] ?? "");

                        string email = ((string)kh["email"] ?? "");
                        string note = ((string)kh["note"] ?? "");
                        string brithday = "NULL";
                        if (!Library.IsNullOrEmpty(kh["birthday"]))
                        {
                            brithday ="'" +Library.ConvertSmallDatetime(((string)kh["birthday"] ?? "")).ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"))+"'";
                        }

                        string sql_run = $"update dmkh set ";
                        sql_run += Environment.NewLine + $"ten_kh=N'{cus.ten_kh}' ,ten_kh2 = N'{cus.ten_kh}',dia_chi = N'{cus.diachi}',dien_thoai = '{cus.phone}',e_mail = N'{email}',tinh_thanh = '{cus.tinh_thanh}',";
                        sql_run += Environment.NewLine + $"ghi_chu = N'{note}',gioi_tinh = {gioitinh},ngay_sinh = {brithday},dia_chi1 = N'{diachi1}',dia_chi2 = N'{diachi2}',quan_huyen = '{cus.quan_huyen}',phuong_xa = '{cus.quan_huyen}'";
                        sql_run += Environment.NewLine + $"where LTRIM(LTRIM(isnull(ma_kh,''))) = LTRIM(LTRIM(isnull({cus.phone},'')))";
                        int i = DAL.DAL_SQL.ExecuteNonquery(sql_run);                       
                    }
                    else
                    {
                        AddCustomer(kh);
                    }
                }
            }
            catch (Exception e)
            {
                ApiLog.log("err-MdelApp-UpdateCustomer", "", "", "", "");
            }

        }

        public void UpdatediemSo(JObject kh)
        {
            try
            {
                CustomerInfo cus = GetAddressByHaravan(kh);
                if (cus.phone != "")
                {
                    string s = $"update dmkh set diem_so=0 where ma_kh = '{cus.phone.Trim()}' ";
                    DAL.DAL_SQL.ExecuteNonquery(s);       
                }
            }
            catch (Exception e)
            {
                ApiLog.log("err-MdelApp-UpdateCustomer", "", "", "", "");
            }

        }
        private void DeleteCustomer(string ma_kh)
        {
            try
            {
                string s = $"Delete from dmkh where LTRIM(LTRIM(ma_kh)) = LTRIM(LTRIM({ma_kh}))";
                DAL.DAL_SQL.ExecuteNonquery(s);
            }
            catch (Exception e)
            {
                ApiLog.logval("deleteCustomer-error", e.Message);
                ILog log = Logger.GetLog(typeof(Customers));
                log.Error(e.Message);
            }

        }

        public static async Task<string> getIdCustomerByID(string ma_kh)
        {
            try
            {
                string s = $"select * from dmkh where ma_kh='{ma_kh.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(s);
                if (ds.Tables[0].Rows.Count == 0) return "";
                string phone = ds.Tables[0].Rows[0]["dien_thoai"].ToString();


                var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");
                string url = $"https://apis.haravan.com/com/customers.json?query=filter=(phone:customer={phone.Trim()})";
                HttpClientApp client = new HttpClientApp(token);
                ResponseApiHaravan res = await client.Get_Request(url);
                JObject data = JObject.Parse(res.data);
                JArray data_customers = (JArray)data["customers"];
                if (data_customers.Count == 0) return "";
                JObject cus = (JObject)data_customers[0];
                string id = (string)cus["id"] ?? "";
                return id;
            }
            catch (Exception e)
            {
  
                return "";
            }           
        }

        public string Ma_tinh_By_Code(string ma_tinh)
        {
            try
            {
                string s = $"select * from hrdmtinh where rtrim(ltrim(har_ma_tinh)) = '{ma_tinh.Trim()}' ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(s);
                string re = ds.Tables[0].Rows[0]["ma_tinh"].ToString();
                return re;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public string Ma_quan_By_Code(string ma_quan)
        {
            try
            {
                string s = $"select * from hrdmquan where rtrim(ltrim(har_ma_quan)) = '{ma_quan.Trim()}' ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(s);
                string re = ds.Tables[0].Rows[0]["ma_quan"].ToString();
                return re;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public string Ma_phuong_By_Code(string ma_phuong)
        {
            try
            {
                string s = $"select * from hrdmphuong where rtrim(ltrim(har_ma_phuong)) = '{ma_phuong.Trim()}' ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(s);
                string re = ds.Tables[0].Rows[0]["ma_phuong"].ToString();
                return re;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public void CreatNewKH(JObject kh)
        {
            try
            {
                CustomerInfo cus = GetAddressByHaravan(kh);
                if (cus.phone != "")
                {
                    string s = $"select * from dmkh where ma_kh = '{cus.phone.Trim()}' ";
                    DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(s);
                    if (ds.Tables[0].Rows.Count == 0)
                    {
                        JArray arrAddress = (JArray)kh["addresses"];
                        string diachi1 = "";
                        string diachi2 = "";
                        if (arrAddress.Count > 0)
                        {
                            var item = (JObject)arrAddress[0];
                            diachi1 = ((string)item["address1"] ?? "")
                                + " " + ((string)item["address2"] ?? "")
                                + " " + ((string)item["ward"] ?? "")
                                + " " + ((string)item["district"] ?? "")
                            + " " + ((string)item["province"] ?? "")
                            + " " + ((string)item["country"] ?? "");
                            diachi1 = Library.AutoCutStringtooLong("dmkh", "dia_chi1", diachi1);
                        }
                        if (arrAddress.Count > 1)
                        {
                            var item = (JObject)arrAddress[1];
                            diachi2 = ((string)item["address1"] ?? "")
                                + " " + ((string)item["address2"] ?? "")
                                + " " + ((string)item["ward"] ?? "")
                                + " " + ((string)item["district"] ?? "")
                            + " " + ((string)item["province"] ?? "")
                            + " " + ((string)item["country"] ?? "");
                            diachi2 = Library.AutoCutStringtooLong("dmkh", "dia_chi2", diachi2);
                        }
                        double gioitinh = Library.ConvertToDouble((string)kh["gender"] ?? "");

                        string email = ((string)kh["email"] ?? "");
                        string note = ((string)kh["note"] ?? "");
                        string brithday = "NULL";
                        if (!Library.IsNullOrEmpty(kh["birthday"]))
                        {
                            brithday = "'" + Library.ConvertSmallDatetime(((string)kh["birthday"] ?? "")).ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "'";
                        }

                        string sql_run = $"insert into dmkh(ma_kh,ten_kh,ten_kh2,kh_yn,cc_yn,nv_yn,dia_chi,dien_thoai,e_mail,ong_ba,ma_nvbh,tinh_thanh,ghi_chu,";
                        sql_run += Environment.NewLine + $"t_tien_cn,t_tien_hd,ngay_gh,status,tien,t_tien,diem_so,t_diem_so,gioi_tinh,ngay_sinh,dia_chi1,dia_chi2,quan_huyen,phuong_xa)";
                        sql_run += Environment.NewLine + $"values('{cus.phone}', N'{cus.ten_kh}', N'{cus.ten_kh}', 1, 1, 1, N'{cus.diachi}', N'{cus.phone}', N'{email}', N'', '','{cus.tinh_thanh}', N'{note}',";
                        sql_run += Environment.NewLine + $"0, 0, GETDATE(), '1', 0, 0, 0, 0, {gioitinh}, {brithday}, N'{diachi1}',N'{diachi2}' , '{cus.quan_huyen}', '{cus.phuong_xa}'); ";
                        int i = DAL.DAL_SQL.ExecuteNonquery(sql_run);
                    }
                }
            }
            catch (Exception e)
            {
                ApiLog.log("error-CreatNewKH", "", e.Message, "", "");
            }
        }
        public CustomerInfo GetAddressByHaravan(JObject obj)
        {
            CustomerInfo kh = new CustomerInfo();
            try
            {
                if (Library.IsNullOrEmpty(obj["phone"]))
                {
                    JObject defualt_address = (JObject)obj["default_address"];
                    if (Library.IsNullOrEmpty(defualt_address["phone"]))
                    {
                        JArray lst_address = (JArray)obj["addresses"];
                        for(int i = 0; i < lst_address.Count; i++)
                        {
                            JObject address = (JObject)lst_address[i];
                            if (!Library.IsNullOrEmpty(address["phone"]))
                            {
                                kh.phone = (string)address["phone"] ?? "";
                                string diachi = (string)address["address1"] ?? "";
                                kh.diachi = Library.AutoCutStringtooLong("dmkh", "dia_chi", diachi);
                                kh.ten_kh = ((string)address["last_name"] ?? "") + " " + ((string)address["first_name"] ?? "");
                                kh.tinh_thanh = Ma_tinh_By_Code((string)address["province_code"] ?? "");
                                kh.quan_huyen = Ma_quan_By_Code(((string)address["district_code"] ?? ""));
                                kh.phuong_xa = Ma_phuong_By_Code(((string)address["ward_code"] ?? ""));
                                break;
                            }
                        }
                        return kh;
                    }
                    else
                    {
                        kh.phone = (string)defualt_address["phone"] ?? "";
                        string diachi = (string)defualt_address["address1"] ?? "";
                        kh.diachi = Library.AutoCutStringtooLong("dmkh", "dia_chi", diachi);
                        kh.ten_kh = ((string)defualt_address["last_name"] ?? "") + " " + ((string)defualt_address["first_name"] ?? "");
                        kh.tinh_thanh = Ma_tinh_By_Code((string)defualt_address["province_code"] ?? "");
                        kh.quan_huyen = Ma_quan_By_Code(((string)defualt_address["district_code"] ?? ""));
                        kh.phuong_xa = Ma_phuong_By_Code(((string)defualt_address["ward_code"] ?? ""));
                        return kh;
                    }
                }
                else
                {
                    kh.phone = (string)obj["phone"] ?? "";
                    string diachi = (string)obj["address1"] ?? "";
                    kh.diachi = Library.AutoCutStringtooLong("dmkh", "dia_chi", diachi);
                    kh.ten_kh = ((string)obj["last_name"] ?? "") + " " + ((string)obj["first_name"] ?? "");
                    kh.tinh_thanh = Ma_tinh_By_Code((string)obj["province_code"] ?? "");
                    kh.quan_huyen = Ma_quan_By_Code(((string)obj["district_code"] ?? ""));
                    kh.phuong_xa = Ma_phuong_By_Code(((string)obj["ward_code"] ?? ""));
                    return kh;
                }
            }
            catch(Exception e)
            {
                return kh;
            }
        }
        public async Task<ResponseData> CreatNewCustomer(Res_NewCustomer cus)
        {
            try
            {
                if(cus.ma_kh == null || cus.ma_kh.Trim() == "") return new ResponseData("err", "Chưa nhập số điện thoại khách hàng" , "");


                var token = MyAppData.config.GetValue<string>("config_Haravan:private_token");
                string url = $"https://apis.haravan.com/com/customers.json?query=filter=(phone:customer={cus.ma_kh})";
                HttpClientApp client = new HttpClientApp(token);
                ResponseApiHaravan res_check = await client.Get_Request(url);
                if (res_check.status == "ok")
                {
                    JObject root_customer = JObject.Parse(res_check.data);
                    JArray ListCustomer = (JArray)root_customer["customers"];
                    if (ListCustomer.Count > 0) return new ResponseData("ok", "ok", "");
                }
                else return new ResponseData("err", res_check.data, "");



                url = $"https://apis.haravan.com/com/customers.json";
                List<AddressHaravan> address = new List<AddressHaravan>();

                AddressHaravan add = new AddressHaravan();
                add.address1 = cus.dia_chi;
                add.phone = cus.ma_kh;
                add.district_code = DiaChi.getmaquanhar(cus.ma_quan);
                add.ward_code = DiaChi.getmaphuonghar(cus.ma_phuong);
                add.province_code = DiaChi.getmatinhhar(cus.ma_tinh);
                add.country_code = "vn";
                if (add.province_code != "")
                address.Add(add);

                Object body = new
                {
                    customer = new
                    {
                        first_name = cus.ten_kh,
                        last_name = " ",
                        email = cus.email,
                        phone = cus.ma_kh,
                        addresses = address
                    }
                };
                JObject temp = JObject.FromObject(body);

                ResponseApiHaravan res = await client.Post_Request_WithBody(url, temp.ToString());
                if (res.status == "ok") return new ResponseData("ok", "Thêm thành công", "");
                else return new ResponseData("err", res.data, "");
            }
            catch (Exception e)
            {
                return new ResponseData("err", e.Message, "");
            }
        }
    }
}
