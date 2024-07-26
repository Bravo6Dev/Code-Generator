using System;
using System.Linq;
using System.Text;

namespace BusinessLayer
{
    public class DataLayerCreater
    {
        private readonly Databases DB;

        private string CreateConectionString(string Server)
            => $"Server = {Server}; Database = {DB.Name}; Integrated Security = true";
        private string Connstr(string Server)
            => $"        static readonly string Connstr = \"{CreateConectionString(Server)}\";";

        private string CreateGetAllMethod(string TableName)
        {
            StringBuilder Code = new StringBuilder();
            Code.AppendLine("        static public DataTable GetAll()\n        {");
            Code.AppendLine("                DataTable DT = new DataTable();");
            Code.AppendLine("                using (SqlConnection Conn = new SqlConnection(Connstr))\n                {");
            Code.AppendLine($"                    string Query = \"SELECT * FROM {TableName}\";");
            Code.AppendLine("                     Conn.Open();");
            Code.AppendLine("                    using (SqlCommand cmd = new SqlCommand(Query, Conn))\n                     {");
            Code.AppendLine("                        using (SqlDataReader Reader = cmd.ExecuteReader())\n                        {");
            Code.AppendLine("                               DT.Load(Reader);");
            Code.AppendLine("                        }\n                    }\n                }");
            Code.AppendLine("                return DT;");
            Code.AppendLine("        }");

            return Code.ToString();
        }

        private string AddNewQuery(Tables T)
        {
            if (T == null)
                throw new ArgumentNullException("Table was Null");

            StringBuilder Query = new StringBuilder();
            string[] ColumnsName = T.ColumnsName();

            Query.AppendLine($"                    string Query = @\"INSERT INTO {T.Name}");
            Query.Append("                                    (");

            Query.Append(string.Join(", ", T.ColumnsName().Skip(1).Select(c => $"[{c}]")));

            Query.AppendLine(")\n                                     VALUES");
            Query.Append("                                     (");

            Query.Append(string.Join(", ", T.ColumnsName().Skip(1).Select(c => $"@{c}")));

            Query.AppendLine(");\n                                     SELECT SCOPE_IDENTITY();\";");
            return Query.ToString();
        }

        public string UpdateQuery(Tables T)
        {
            if (T == null)
                throw new ArgumentNullException("Table was null");
            StringBuilder Query = new StringBuilder();
            string[] ColumnsName = T.ColumnsName();
            Query.Append($"                    string Query = @\"UPDATE [{T.Name}]\n                                   SET ");
            Query.Append(string.Join("                                      ,", ColumnsName.Skip(1)
                .Select(C => $"[{C}] = @{C}\n")));

            Query.Append($"\n                                    WHERE [{ColumnsName[0]}] = @{ColumnsName[0]}");
            return Query.Append("\";").ToString();
        }

        private string CreateAddNewMethod(string TableName)
        {
            StringBuilder Code = new StringBuilder();
            Tables T = DB.GetTable(TableName);

            if (T == null)
                throw new ArgumentNullException($"Table with {TableName} doesn't found");

            Code.Append("        static public int AddNew(");
            for (int i = 1; i < T.Parameters().Length; i++)
                Code.Append(T.Parameters()[i]);
            Code.AppendLine(")\n        {");
            Code.AppendLine("                using (SqlConnection Conn = new SqlConnection(Connstr))\n                {");
            Code.AppendLine($"{AddNewQuery(T)}");
            Code.AppendLine("                    Conn.Open();");
            Code.AppendLine("                    using (SqlCommand cmd = new SqlCommand(Query, Conn))\n                    {");

            for (int i = 1; i < T.ColumnsName().Length; i++)
                Code.AppendLine($"                        cmd.Parameters.AddWithValue(\"@{T.ColumnsName()[i]}\", {T.ColumnsName()[i]});");

            Code.AppendLine("                        object result = cmd.ExecuteScalar();");
            Code.AppendLine("                        if (result != null && int.TryParse(result.ToString(), out int ID)) { return ID; }");
            Code.AppendLine("                                                return -1;");
            Code.AppendLine("                    }\n                }\n        }");
            return Code.ToString();
        }

        private string CreateUpdateMethod(string TableName)
        {
            StringBuilder Code = new StringBuilder();
            Tables T = DB.GetTable(TableName);

            if (T == null)
                throw new ArgumentNullException($"Table with {TableName} doesn't found");

            Code.Append("        static public bool Update(");
            foreach (string Param in T.Parameters())
                Code.Append(Param);

            Code.AppendLine(")\n        {");
            Code.AppendLine("                using (SqlConnection Conn = new SqlConnection(Connstr))\n                {");
            Code.AppendLine(UpdateQuery(T));
            Code.AppendLine("                    Conn.Open();");
            Code.AppendLine("                    using (SqlCommand cmd = new SqlCommand(Query, Conn))");
            Code.AppendLine("                    {");
            for (int i = 0; i < T.ColumnsName().Length; i++)
                Code.AppendLine($"                        cmd.Parameters.AddWithValue(\"@{T.ColumnsName()[i]}\", {T.ColumnsName()[i]});");
            Code.AppendLine($"                        return cmd.ExecuteNonQuery() > 0;");
            Code.Append("                    }\n                }\n        }");
            return Code.ToString();
        }

        private string CreateDeleteMethod(string TableName)
        {
            Tables T = DB.GetTable(TableName);

            if (T == null)
                throw new ArgumentNullException($"Table with {TableName} doesn't found");

            StringBuilder Code = new StringBuilder();
            Code.AppendLine($"        static public bool Delete(int {T.ColumnsName()[0]})\n        {{");
            Code.AppendLine("                using (SqlConnection Conn = new SqlConnection(Connstr))\n                 {");
            Code.AppendLine($"                                    string Query = @\"DELETE FROM {T.Name}");
            Code.AppendLine($"                                    WHERE {T.ColumnsName()[0]} = @{T.ColumnsName()[0]}\";");
            Code.AppendLine($"                    Conn.Open();");
            Code.AppendLine("                    using (SqlCommand cmd = new SqlCommand(Query, Conn))\n                    {");
            Code.AppendLine($"                        cmd.Parameters.AddWithValue(\"@{T.ColumnsName()[0]}\", {T.ColumnsName()[0]});");
            Code.AppendLine($"                        return cmd.ExecuteNonQuery() > 0;");
            Code.AppendLine("                    }\n                }\n        }");
            return Code.ToString();
        }

        private string CreateFindMethod(string TableName)
        {
            Tables T = DB.GetTable(TableName);

            if (T == null) throw new ArgumentNullException($"Table with {TableName} doesn't exist");
            StringBuilder Code = new StringBuilder();

            string[] ColumnsName = T.ColumnsName();
            string[] Parameters = T.Parameters();

            Code.Append("        static public bool Find(");
            for (int i = 0; i < Parameters.Length; i++)
            {
                if (i == 0)
                    Code.Append($"{Parameters[i]}");
                else
                    Code.Append($"ref {Parameters[i]}");
            }
            Code.AppendLine(")\n         {");
            Code.AppendLine("                using (SqlConnection Conn = new SqlConnection(Connstr))\n                 {");
            Code.AppendLine($@"                    string Query = @""SELECT * FROM {TableName}
                                    WHERE [{ColumnsName[0]}] = @{ColumnsName[0]}"";");
            Code.AppendLine("                    Conn.Open();");
            Code.AppendLine("                    using (SqlCommand cmd = new SqlCommand(Query, Conn))\n                    {");
            Code.AppendLine($"                        cmd.Parameters.AddWithValue(\"@{ColumnsName[0]}\", {ColumnsName[0]});");
            Code.AppendLine("                        using (SqlDataReader Reader = cmd.ExecuteReader())\n                         {");
            Code.AppendLine("                            if (Reader.Read())\n                             {");

            Code.AppendLine(string.Join("", ColumnsName.Skip(1)
                .Select(C => $@"                                {C} = {T.ConvertTo(C)}Reader[""{C}""]);{"\n"}")));

            Code.AppendLine("                        }\n                            return true;");
            Code.AppendLine("                        }\n                    }\n                }\n        }");
            return Code.ToString();
        }

        public string CreateClass(string namespaceName, string TableName)
        {
            StringBuilder Class = new StringBuilder();
            Class.AppendLine("using System;");
            Class.AppendLine("using System.Data;");
            Class.AppendLine("using Microsoft.Data.SqlClient;");
            Class.AppendLine($"namespace {namespaceName}\n{{");
            Class.AppendLine($"    public class {TableName + "Data"}\n    {{");
            Class.AppendLine(Connstr("."));
            Class.AppendLine(CreateGetAllMethod(TableName));
            Class.AppendLine(CreateAddNewMethod(TableName));
            Class.AppendLine(CreateDeleteMethod(TableName));
            Class.AppendLine(CreateUpdateMethod(TableName));
            Class.AppendLine(CreateFindMethod(TableName));
            Class.AppendLine("    }\n}");
            return Class.ToString();
        }

        public DataLayerCreater(string databasename)
        {
            try
            {
                DB = new Databases(databasename);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataLayerCreater(Databases DB)
        {
            this.DB = DB;
        }
    }
}
