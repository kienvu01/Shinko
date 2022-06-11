using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.ModelVc
{
    public class M_GHN_API
    {
    }
    public class Re_HuyGH
    {
        public string ma_vd { get; set; }
    }
    public class GetFee
    {
        public string ma_kho { get; set; }
        public int? pt_vc { get; set; }
        public int? height { get; set; }
        public int? length { get; set; }
        public int? weight { get; set; }
        public int? width { get; set; }
        public int? insurance_fee { get; set; }
        public string coupon { get; set; }
        public string to_ma_quan { get; set; }
        public string to_ma_phuong { get; set; }

    }

    public class GiaoHang
    {
        public string stt_rec { get; set; }
        public string tablesufix { get; set; }
        public int? pt_vc { get; set; }
        public string note { get; set; }
        public int? nguoi_tra { get; set; }
        public int? cho_xem_hang { get; set; }
        public int? cho_thu_hang { get; set; }
        public string to_name { get; set; }
        public string to_phone { get; set; }
        public int? chieu_cao { get; set; }
        public int? chieu_dai { get; set; }
        public int? trong_luong { get; set; }
        public int? chieu_rong { get; set; }
        public int? tien_boi_thuong { get; set; }
        public decimal tien_thu_ho{ get; set; }
        public string ma_km { get; set; }

    }

}
