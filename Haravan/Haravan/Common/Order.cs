using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Haravan.Common
{
    public class OrderHarDetail
    {
        public string id { get; set; }
        public string number { get; set; }
        public string job { get; set; }
        public string ma_kh { get; set; }
    }
    public class CheckOrderExit
    {
        public string status { get; set; }
        public bool check { get; set; }
        public string tbl_sufix { get; set; }
        public CheckOrderExit(){}
        public CheckOrderExit(string _status,bool _check,string _tbl_sufix)
        {
            status = _status;
            check = _check;
            tbl_sufix = _tbl_sufix;
        }
    }
    public class Order
    {
        string stt_rec;
        DateTime ngay_lct;
        double t_so_luong;
        double t_tien_nt;
        double t_tt_nt;
        double t_tien2;
        double t_cp_vc_nt;
        double so_ct_post;
        double t_ck_tt_nt;


    }
    public class HisotryOrder
    {
        public string id { get; set; }
        public string so_ct { get; set; }
        public int status { get; set; }
        //0:ok ,1:err
        public HisotryOrder() { }
    }

    public class API_Data_Order_UpdateOrder
    {
        public string fromDate { get; set; }
        public string toDate { get; set; }
        public List<string> so_ct { get; set; }
        //0:ok ,1:err
        public API_Data_Order_UpdateOrder() { }
    }
}
