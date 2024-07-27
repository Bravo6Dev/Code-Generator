using DataLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class Databases
    {
        public int DatabaseId { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }

        public DataTable LoadTables()
        {
            try
            {
                return DataBasesData.GetTables(Name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static public async Task<DataTable> GetDatabases()
        {
            try
            {
                return await DataBasesData.GetDatabasesAsync();
            }
            catch(Exception ex) { throw ex; }
        }

        static public Databases GetDatabase(int Id)
        {
            string name = "";
            DateTime create_date = new DateTime();
            try
            {
                if (DataBasesData.GetDatabase(Id, ref name, ref create_date))
                    return new Databases(Id, name, create_date);
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static public Databases GetDatabase(string name)
        {
            int ID = -1;
            DateTime create_date = new DateTime();
            try
            {
                if (DataBasesData.GetDatabase(name, ref ID, ref create_date))
                    return new Databases(ID, name, create_date);
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Tables GetTable(string TableName)
        {
            try
            {
                return DataBasesData.GetTable(Name, TableName) ?
                    new Tables(Name, TableName) : null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int NumbersOfTables()
        {
            try
            {
                return DataBasesData.GetCountOfTable(Name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Databases(string name)
        {
            try
            {
                string Name = ""; int Id = -1; DateTime date = new DateTime();
                if (!DataBasesData.GetDatabase(name, ref Id, ref date))
                    throw new NotImplementedException($"database with {name} not found");
                else
                {
                    this.DatabaseId = Id;
                    this.Name = name;
                    this.CreationDate = date;
                    LoadTables();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Databases(int ID, string name, DateTime Create_Date)
        {
            this.DatabaseId = ID;
            this.Name = name;
            this.CreationDate = Create_Date;
            LoadTables();
        }
    }
}
