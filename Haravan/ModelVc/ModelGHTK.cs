using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.ModelVc
{
    public class GHTK_Location
    {
        public string status { get; set; }
        public string pick_address_id { get; set; }
        public string pick_address { get; set; }
        public string pick_province { get; set; }
        public string pick_district { get; set; }
        public string pick_ward { get; set; }
    }
    public class ModelGHTK
    {
        public static GHTK_Location GetAdressLocation(string ma_kho)
        {
            GHTK_Location re = new GHTK_Location();
            try
            {
                string sql = $"select ma_dvcs,ma_kho,a.ma_tinh,b.ten_tinh,a.ma_quan,c.ten_quan,a.ma_phuong,d.ten_phuong,s2,dia_chi from dmkho a left join hrdmtinh b on a.ma_tinh = b.ma_tinh left join hrdmquan c on a.ma_quan = c.ma_quan left join hrdmphuong d on a.ma_phuong = d.ma_phuong  where ma_kho='{ma_kho.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count == 0) {
                    re.status = "KHONGCOKHO";
                    return re;
                }
                re.status = "OK";
                re.pick_address_id = ds.Tables[0].Rows[0]["s2"].ToString().Trim();
                re.pick_address = ds.Tables[0].Rows[0]["dia_chi"].ToString().Trim();
                re.pick_province = ds.Tables[0].Rows[0]["ten_tinh"].ToString().Trim();
                re.pick_district = ds.Tables[0].Rows[0]["ten_quan"].ToString().Trim();
                re.pick_ward = ds.Tables[0].Rows[0]["ten_phuong"].ToString().Trim();
                return re;
            }
            catch (Exception e)
            {
                re.status = "KHONGCOKHO";
                return re;
            }
        }
        public static string GetTenTinh(string ma_tinh)
        {
            string re = "";
            try
            {
                string sql = $"select * from hrdmtinh where ma_tinh='{ma_tinh.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                re = ds.Tables[0].Rows[0]["ten_tinh"].ToString().Trim();
                return re;
            }
            catch (Exception e)
            {
                return re;
            }
        }
        public static string GetTenQuan(string ma_quan)
        {
            string re = "";
            try
            {
                string sql = $"select * from hrdmquan where ma_quan='{ma_quan.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                re = ds.Tables[0].Rows[0]["ten_quan"].ToString().Trim();
                return re;
            }
            catch (Exception e)
            {
                return re;
            }
        }
        public static string GetTenPhuong(string ma_phuong)
        {
            string re = "";
            try
            {
                string sql = $"select * from hrdmphuong where ma_phuong='{ma_phuong.Trim()}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                re = ds.Tables[0].Rows[0]["ten_phuong"].ToString().Trim();
                return re;
            }
            catch (Exception e)
            {
                return re;
            }
        }
    }
}
