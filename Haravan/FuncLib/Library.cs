using Haravan.Model;
using Haravan.ModelsApp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Haravan.FuncLib
{
    public class Library
    {
        private static readonly Encoding encoding = Encoding.UTF8;
        public static JObject ConvertRawBodyJobject(Object obj)
        {
            string s123 = obj.ToString();

            JObject j = JObject.Parse(s123);
            return j;
        }
        public static string ComputeSha256Hash(string rawData,string key)
        {
            // Create a SHA256   
            var keyByte = encoding.GetBytes(key);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                hmacsha256.ComputeHash(encoding.GetBytes(rawData));
                //hmacsha256.ComputeHash(ObjectToByteArray(rawData));
                return ByteToString(hmacsha256.Hash);
                //return BitConverter.ToString(hmacsha256.Hash).ToLower().Replace("-", string.Empty);
            }
        }
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public static string ByteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
                sbinary += buff[i].ToString("X2"); /* hex format */
            return sbinary;
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Verify_Webhook(Object data, string key)
        {
            string json = JsonConvert.SerializeObject(data);
            string s = Base64Encode(ComputeSha256Hash(json, key));
            return s;
        }
        public static DateTime ConvertSmallDatetime(string val)
        {
            try
            {
                DateTime re = DateTime.ParseExact(val, "yyyy/MM/dd ", System.Globalization.CultureInfo.InvariantCulture);
                return re;
            }
            catch (Exception e)
            {
                return DateTime.Now;
            }
        }
        public static DateTime ConvertDatetime(string val)
        {
            try
            {
                DateTime re = DateTime.ParseExact(val, "yyyy-MM-dd Thh:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
                return re;
            }
            catch (Exception e)
            {
                return DateTime.Now;
            }
        }
        public static bool IsNullOrEmpty(JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }
        public static double ConvertToDouble(string val)
        {
            try
            {
                double re = Convert.ToDouble(val);
                return re;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
        public static decimal ConvertToDecimal(string val)
        {
            try
            {
                decimal re = Convert.ToDecimal(val);
                return re;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
        public static int ConvertToInt(string val)
        {
            try
            {
                int re = Convert.ToInt32(val);
                return re;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
        public static string GetMaKMFromVAL(string val)
        {
            try
            {
                int i = val.IndexOf('-');
                string re = val.Substring(0, i);
                return re;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static string GetTenKMFromVAL(string val,string ma)
        {
            try
            {
                string re = val.Replace(ma, "");
                return re;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static bool CheckStringNgayThang(string s)
        {
            if (s.Trim().Length != 6) return false;
            try
            {
                int y = Convert.ToInt32(s.Substring(0, 4));
                int m = Convert.ToInt32(s.Substring(4, 2));
                DateTime t = new DateTime(y, m, 1);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
        public static string AutoCutStringtooLong (string tablename,string col,string val)
        {
            try
            {
                string sql = $"select COLUMN_NAME,IS_NULLABLE,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME ='{tablename}' and COLUMN_NAME='{col}'";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                int maxlength =Convert.ToInt32(ds.Tables[0].Rows[0]["CHARACTER_MAXIMUM_LENGTH"].ToString());
                if (val.Length >= maxlength)
                    return val.Substring(0, maxlength - 1);
                else return val;             
            }
            catch (Exception e)
            {
                return val;
            }
        }

        public static decimal GetTonKhoTheoItem( string ma_vt,string ma_kho)
        {
            try
            {
                string sql = $"SELECT   ma_vt,ma_kho, ton13 FROM cdvt213  where ma_kho='{ma_kho.Trim()}' and ma_vt='{ma_vt.Trim()}' ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    decimal ton13 = ConvertToDecimal(ds.Tables[0].Rows[0]["ton13"].ToString());
                    return ton13;
                }
                else return 0;                
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public static ResponseData GetIdStoreHaravan( string ma_kho)
        {
            try
            {
                string sql = $"SELECT   s1 FROM dmkho  where ma_kho='{ma_kho.Trim()}' ";
                DataSet ds = DAL.DAL_SQL.ExecuteGetlistDataset(sql); 
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string s1 = ds.Tables[0].Rows[0]["s1"].ToString();
                    return new ResponseData("ok", s1, s1);
                }
                else return new ResponseData("err", "Kho không tồn tại trên Haravan ", ""); ;

            }
            catch (Exception e)
            {
                return new ResponseData("err",e.Message,"");
            }
        }
        public static bool CheckAuthentication(IConfiguration config, ControllerBase con)
        {
            bool check = false;
            string x = con.Request.Headers["x-sse-code"];
            string y = config.GetValue<string>("x_sse_code");
            if (x == y) check = true;
            return check;
        }
        public static string convertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
        public static string getFirstWord(string text)
        {
            text = text.Trim();
            int i = text.IndexOf(" ");
            if (i == -1) return "";
            string word = text.Substring(0, i).Trim();
            return word;
        }
        public static string CutFristWord(string text)
        {
            text = text.Trim();
            int i = text.IndexOf(" ");
            if (i == -1) return text;
            string word = text.Substring(i).Trim();
            return word;
        }
        public static string TenTinh(string tinh1)
        {
            try
            {
                if (tinh1 == null) return "";
                string s = tinh1.ToUpper().Replace(" ", "");
                return s;
            }
            catch(Exception e)
            {
                return "";
            }
            
        }
        public static string TenQuan(string quan)
        {
            try
            {
                if (quan == null){
                    return "";
                }
                bool check = false;
                quan = quan.ToUpper().Trim();
                string t1 = Library.convertToUnSign(getFirstWord(quan).Trim());
                string t2 = Library.convertToUnSign(getFirstWord(CutFristWord(quan.Trim()).Trim()).Trim());
                if (t1 == "HUYEN" || t1 == "QUAN") return CutFristWord(quan).Replace(" ", "");
                if (t1 + t2 == "THANHPHO") return CutFristWord(CutFristWord(quan).Trim()).Replace(" ", "");
                return quan.Replace(" ", "");
            }
            catch (Exception e)
            {
                return "";
            }
            
        }
        public static string TenPhuong(string quan)
        {
            if (quan == null){
                return "";
            }
            bool check = false;
            quan = quan.ToUpper().Trim();
            string t1 = Library.convertToUnSign( getFirstWord(quan).Trim());
            string t2 = Library.convertToUnSign( getFirstWord(CutFristWord(quan.Trim()).Trim()).Trim());
            if (t1 == "PHUONG" || t1 == "XA") return CutFristWord(quan).Replace(" ", "");
            if (t1 + t2 == "THIXA" ) return CutFristWord(CutFristWord(quan).Trim()).Replace(" ", "");
            return quan.Replace(" ", "");
        }

        public static string SpecialChracterSql(string val)
        {
            try
            {
                return val.Replace("'", "''");
            }
            catch(Exception e)
            {
                return val;
            }
        }
        public static string GetTokenWWP()
        {
            try
            {
                var username = MyAppData.config.GetValue<string>("WP:username");
                var pass = MyAppData.config.GetValue<string>("WP:pass");
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{pass}");
                string token = Convert.ToBase64String(byteArray);
                return token;
            }
            catch(Exception e)
            {
                return "";
            }
        }
    }
}
