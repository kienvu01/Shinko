using Haravan.FuncLib;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Model
{
    public class DiaChi
    {

        public DiaChi()
        {
        }     

        public static string getmatinhbyharavan(string ma_tinh)
        {
            try
            {
                if (ma_tinh == null || ma_tinh.Trim() == "") return "";
                string sql = $"select  ma_tinh from hrdmtinh where har_ma_tinh='{ma_tinh}' ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                string re = "";
                if (ds.Tables[0].Rows.Count > 0)
                {
                    re = ds.Tables[0].Rows[0]["ma_tinh"].ToString().Trim();
                    return re;
                }
                else
                    return "";
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public static string getmahuyenbyharavan(string ma_huyen)
        {
            try
            {
                if (ma_huyen == null || ma_huyen.Trim() == "") return "";
                string sql = $"select  ma_quan from hrdmquan where har_ma_quan='{ma_huyen}' ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                string re = "";
                if (ds.Tables[0].Rows.Count > 0)
                {
                    re = ds.Tables[0].Rows[0]["ma_quan"].ToString().Trim();
                    return re;
                }
                else
                    return "";
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static string getmaphuongbyharavan(string ma_p,string ten_p)
        {
            try
            {
                if (ma_p == null || ma_p.Trim()=="") return "";
                string sql = $"select  ma_phuong from hrdmphuong where har_ma_phuong='{ma_p}' ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                string re = "";
                if (ds.Tables[0].Rows.Count > 0)
                {
                    re = ds.Tables[0].Rows[0]["ma_phuong"].ToString().Trim();
                    return re;
                }
                else
                    return ten_p;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public static string getmaphuonghar(string ma_p)
        {
            try
            {
                if (ma_p == null || ma_p.Trim() == "") return "";
                string sql = $"select  har_ma_phuong from hrdmphuong where rtrim(ltrim(ma_phuong))='{ma_p.Trim()}' ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                string re = "";
                if (ds.Tables[0].Rows.Count > 0)
                {
                    re = ds.Tables[0].Rows[0]["har_ma_phuong"].ToString();
                    return re;
                }
                else
                    return "";
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public static string getmaquanhar(string ma_p)
        {
            try
            {
                if (ma_p == null || ma_p.Trim() == "") return "";
                string sql = $"select  har_ma_quan from hrdmquan where rtrim(ltrim(ma_quan))='{ma_p.Trim()}' ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                string re = "";
                if (ds.Tables[0].Rows.Count > 0)
                {
                    re = ds.Tables[0].Rows[0]["har_ma_quan"].ToString();
                    return re;
                }
                else
                    return "";
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public static string getmatinhhar(string ma_p)
        {
            try
            {
                if (ma_p == null || ma_p.Trim() == "") return "";
                string sql = $"select  har_ma_tinh from hrdmtinh where rtrim(ltrim(ma_tinh))='{ma_p.Trim()}' ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                string re = "";
                if (ds.Tables[0].Rows.Count > 0)
                {
                    re = ds.Tables[0].Rows[0]["har_ma_tinh"].ToString();
                    return re;
                }
                else
                    return "";
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }
}
