using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Model
{
    [Table(name: "dmkh")]
    public class Dmkh
    {
        [Key]
        [Required]
        public string Ma_kh { get; set; }
        public string ten_kh { get; set; }
        public string ten_kh2 { get; set; }
        public bool? kh_yn { get; set; }
        public bool? cc_yn { get; set; }
        public bool? nv_yn { get; set; }
        public string ma_so_thue { get; set; }
        public string dia_chi { get; set; }
        public string dien_thoai { get; set; }
        public string fax { get; set; }
        public string e_mail { get; set; }
        public DateTime? ngay_sinh { get; set; }
        public string home_page { get; set; }
        public string doi_tac { get; set; }
        public string ong_ba { get; set; }
        public string ma_nvbh { get; set; }
        public string ngan_hang { get; set; }
        public string tk_nh { get; set; }
        public string tinh_thanh { get; set; }
        public string ghi_chu { get; set; }
        public string ma_tt { get; set; }
        public string tk { get; set; }
        public string nh_kh1 { get; set; }
        public string nh_kh2 { get; set; }
        public string nh_kh3 { get; set; }
        public string nh_kh9 { get; set; }
        public decimal? du_nt13 { get; set; }
        public decimal? du13 { get; set; }
        public decimal? t_tien_cn { get; set; }
        public decimal? t_tien_hd { get; set; }
        public DateTime? Ngay_Gh { get; set; }
        public string Status { get; set; }
        public DateTime? Datetime0 { get; set; }
        public DateTime? Datetime2 { get; set; }
        public bool? User_Id0 { get; set; }
        public bool? User_Id2 { get; set; }
        public string Ma_Td1 { get; set; }
        public string Ma_Td2 { get; set; }
        public string Ma_Td3 { get; set; }
        public decimal? Sl_Td1 { get; set; }
        public decimal? Sl_Td2 { get; set; }
        public decimal? Sl_Td3 { get; set; }
        public DateTime? Ngay_Td1 { get; set; }
        public DateTime? Ngay_Td2 { get; set; }
        public DateTime? Ngay_Td3 { get; set; }
        public string Gc_Td1 { get; set; }
        public string Gc_Td2 { get; set; }
        public string Gc_Td3 { get; set; }
        public string S1 { get; set; }
        public string S2 { get; set; }
        public string S3 { get; set; }
        public decimal? S4 { get; set; }
        public decimal? S5 { get; set; }
        public decimal? S6 { get; set; }
        public DateTime? S7 { get; set; }
        public DateTime? S8 { get; set; }
        public DateTime? S9 { get; set; }
        public string Ds_Dvcs { get; set; }
        public string gioi_tinh { get; set; }
        public decimal? Diem { get; set; }
        public bool? Khong_Kt_Mst { get; set; }
    }
}
