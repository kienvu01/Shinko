using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Haravan.Common
{
    public class API_Data_Customer_UpdateCustomer
    {
        public string fromDate { get; set; }
        public string toDate { get; set; }
        public List<string> phone { get; set; }
        //0:ok ,1:err
        public API_Data_Customer_UpdateCustomer() { }
    }
    public class CustomerInfo
    {
        public string phone { get; set; }
        public string ten_kh { get; set; }
        public string diachi { get; set; }
        public string tinh_thanh { get; set; }
        public string quan_huyen { get; set; }
        public string phuong_xa { get; set; }
        public CustomerInfo()
        {
            phone = "";
            ten_kh = "";
            diachi = "";
            tinh_thanh = "";
            quan_huyen = "";
            phuong_xa = "";
        }

    }
    public class AddressHaravan
    {
        public string address1 { get; set; }
        public string phone { get; set; }
        public string district_code { get; set; }
        public string ward_code { get; set; }
        public string province_code { get; set; }
        public string country_code { get; set; }
    }
    public class Res_NewCustomer
    {
        public string ma_kh { get; set; }
        public string ten_kh { get; set; }
        public string dia_chi { get; set; }
        public string ma_tinh { get; set; }
        public string ma_phuong { get; set; }
        public string email { get; set; }
        public string ma_quan { get; set; }
    }
    public class Customer
    {
    }
}
