using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataLayer
{
    public class DataBasesData
    {
        private static readonly string Connstr = ConfigurationManager.ConnectionStrings["Connstr"].ConnectionString;

        static async public Task<DataTable> GetDatabasesAsync()
        {
            try
            {
                DataTable DT = new DataTable();
                using (SqlConnection Conn = new SqlConnection(Connstr))
                {
                    string Query = "SELECT database_id, name, create_date FROM sys.databases";
                    await Conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(Query, Conn))
                    {
                        using (SqlDataReader Reader = await cmd.ExecuteReaderAsync())
                        {
                            DT.Load(Reader);
                        }
                    }
                }
                return DT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static public DataTable GetTables(string DatabaseName)
        {
            try
            {
                DataTable DT = new DataTable();
                using (SqlConnection Conn = new SqlConnection(Connstr))
                {
                    string Query = $@"USE {DatabaseName};
                                     SELECT COUNT(*) AS TABLECOUNT FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';";
                    Conn.Open();
                    using (SqlCommand cmd = new SqlCommand(Query, Conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            DT.Load(reader);
                        }
                    }
                }
                return DT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static public DataTable GetColumns(string DatabaseName, string TableName)
        {
            try
            {
                DataTable DT = new DataTable();
                using (SqlConnection Conn = new SqlConnection(Connstr))
                {
                    string Query = $@"USE {DatabaseName}
                                    SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE  FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{TableName}'";
                    Conn.Open();
                    using (SqlCommand cmd = new SqlCommand(Query, Conn))
                    {
                        using (SqlDataReader Reader = cmd.ExecuteReader())
                        {
                            DT.Load(Reader);
                        }
                    }
                }
                return DT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static public bool GetTable(string DatabaseName, string TableName)
        {
            try
            {
                using (SqlConnection Conn = new SqlConnection(Connstr))
                {
                    DataTable DT = new DataTable();
                    string Query = $@"USE {DatabaseName};
                                         SELECT TABLE_CATALOG, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
                                         WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = @TableName;";
                    Conn.Open();
                    using (SqlCommand cmd = new SqlCommand(Query, Conn))
                    {
                        cmd.Parameters.AddWithValue("TableName", TableName);

                        using (SqlDataReader Reader = cmd.ExecuteReader())
                        {
                            return Reader.HasRows;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static public int GetCountOfTable(string DatabaseName, string TableName)
        {
            using (SqlConnection Conn = new SqlConnection(Connstr))
            {
                string query = $@"USE {DatabaseName};
                                 SELECT COUNT(*)"
            }
        }

        static public bool GetDatabase(int Id, ref string name, ref DateTime CreationDate)
        {
            try
            {
                using (SqlConnection Conn = new SqlConnection(Connstr))
                {
                    string Query = @"SELECT database_id, name, create_date FROM sys.databases
                                    WHERE database_id = @ID";
                    Conn.Open();
                    using (SqlCommand cmd = new SqlCommand(Query, Conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", Id);
                        using (SqlDataReader Reader = cmd.ExecuteReader())
                        {
                            if (Reader.Read())
                            {
                                name = Reader["name"].ToString();
                                CreationDate = Convert.ToDateTime(Reader["create_date"]);
                                return true;
                            }
                            else
                                return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static public bool GetDatabase(string name, ref int Id, ref DateTime CreationDate)
        {
            try
            {
                using (SqlConnection Conn = new SqlConnection(Connstr))
                {
                    string Query = @"SELECT database_id, name, create_date FROM sys.databases
                                    WHERE name = @Name";
                    Conn.Open();
                    using (SqlCommand cmd = new SqlCommand(Query, Conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        using (SqlDataReader Reader = cmd.ExecuteReader())
                        {
                            if (Reader.Read())
                            {
                                Id = Convert.ToInt32(Reader["database_id"]);
                                CreationDate = Convert.ToDateTime(Reader["create_date"]);
                                return true;
                            }
                            else
                                return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
