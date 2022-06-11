using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.DAL
{
    public class DAL_TEST
    {
        private static int TIME_OUT_SQL_REQUEST = 600;
        private static string conStr;
        public static Exception errorException { get; set; }
        public static void SetConnectionString(string conStr)
        {
            DAL_TEST.conStr = conStr;
        }
        public static SqlConnection GetConnection()
        {
            try
            {
                errorException = null;
                return new SqlConnection(conStr);
            }
            catch (Exception ex)
            {
                errorException = ex;
                throw;
            }
        }

        public static DataSet ExecuteGetlistDataset(string sqlQuery)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = TIME_OUT_SQL_REQUEST;
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(ds);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("sqlQuery: " + sqlQuery, ex);
            }

            return ds;
        }

        public static int ExecuteNonquery(string sqlQuery)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();
                    using (SqlCommand myCommand = new SqlCommand())
                    {
                        myCommand.Connection = conn;
                        myCommand.CommandType = CommandType.Text; // kiểu lệnh: Text hay lấy từ Stored Proceducer.
                        myCommand.CommandText = sqlQuery;   // Lệnh = chuỗi truyền vào.

                        return myCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorException = ex;
                throw new Exception("sqlQuery: " + sqlQuery, ex);
            }
        }
        public static int ExecuteNonquery(String _queryStore, SqlParameter[] sqlParameter)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();
                    using (SqlCommand myCommand = new SqlCommand())
                    {
                        myCommand.Connection = conn;
                        myCommand.CommandType = CommandType.StoredProcedure;
                        myCommand.CommandText = _queryStore;
                        myCommand.Parameters.AddRange(sqlParameter);
                        return myCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                errorException = ex;
                throw new Exception("sqlQuery: " + _queryStore, ex);
            }
        }
    }
}
