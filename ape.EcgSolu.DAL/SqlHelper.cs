using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SQLite;

namespace ape.EcgSolu.DAL
{
    public class SqlHelper
    {
        static string connectionString=ConfigurationManager.ConnectionStrings["sqlitedb"].ConnectionString;

        public static DataTable ExecuteReader(string sqlText,params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection sqliteConn = new SQLiteConnection(connectionString))
            {
                sqliteConn.Open();
                SQLiteCommand sqliteCmd = sqliteConn.CreateCommand();
                sqliteCmd.CommandText = sqlText;
                sqliteCmd.Parameters.AddRange(parameters);
                SQLiteDataAdapter sqliteAdapter = new SQLiteDataAdapter(sqliteCmd);
                DataTable dbTable =new DataTable();
                sqliteAdapter.Fill(dbTable);
                sqliteCmd.Parameters.Clear();
                return dbTable;
            }
        }

        public static int ExecuteNoQuery(string sqlText, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection sqliteConn = new SQLiteConnection(connectionString))
            {
                sqliteConn.Open();
                using (SQLiteCommand sqliteCmd = new SQLiteCommand(sqliteConn))
                {
                    sqliteCmd.CommandText = sqlText;
                    sqliteCmd.Parameters.AddRange(parameters);
                    int res = sqliteCmd.ExecuteNonQuery();
                    sqliteCmd.Parameters.Clear();
                    return res;
                }
            }
        }

        public static object ExecuteScalar(string sqlText, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection sqliteConn = new SQLiteConnection(connectionString))
            {
                sqliteConn.Open();
                using (SQLiteCommand sqliteCmd = new SQLiteCommand(sqliteConn))
                {
                    sqliteCmd.CommandText = sqlText;
                    sqliteCmd.Parameters.AddRange(parameters);
                    object resObj = sqliteCmd.ExecuteScalar();
                    sqliteCmd.Parameters.Clear();
                    return resObj;
                }
            }
        }

        public static object ToDbValue(object value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }
            else
            {
                return value;
            }
        }

        public static object FromDbValue(object value)
        {
            if (value == DBNull.Value)
            {
                return null;
            }
            else
            {
                return value;
            }
        }
    }
}
