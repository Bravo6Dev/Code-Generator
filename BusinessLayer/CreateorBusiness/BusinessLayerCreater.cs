using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public enum enMode { AddNew, Update }
    public class BusinessLayerCreater
    {
        Databases DB;
        Tables Table;

        private string CreateEnum()
        {
            StringBuilder Enum = new StringBuilder();
            Enum.AppendLine("        enum enMode { AddNew = 1, Update = 2 }");
            Enum.AppendLine("        enMode Mode;");
            return Enum.ToString();
        }

        private string CreateProprty(string TableName,Columns Col)
        {
            Table = DB.GetTable(TableName);
            if (Table == null)
                throw new ArgumentNullException($"Table with {TableName} not found");
            return $"        public {Col.GetCsharpDataType()} {Col.Name} {{ get; set; }}";
        }

        private string[] CreateProprties(string TableName)
        {
            Table = DB.GetTable(TableName);

            if (Table == null)
                throw new ArgumentNullException($"Table With {TableName} doesn't found");
            string[] Proprties = new string[Table.TableColumns.Count];

            for (int i = 0; i < Proprties.Length; i++)
                Proprties[i] = CreateProprty(TableName, Table.TableColumns[i]);
            return Proprties;
        }

        private string CreateCtor(string TableName, enMode Mode)
        {
            Table = DB.GetTable(TableName);

            if (Table == null)
                throw new ArgumentNullException($"Table with {TableName} not found");

            StringBuilder ctor = new StringBuilder();
            switch (Mode)
            {
                case enMode.AddNew:
                    ctor.AppendLine($"        public {TableName}()\n        {{");
                    ctor.AppendLine("            Mode = enMode.AddNew;\n        }");
                    return ctor.ToString();
                case enMode.Update:
                    ctor.Append($"        public {TableName}(");
                    ctor.Append(string.Join("", Table.Parameters()));
                    ctor.AppendLine(")\n         {");
                    ctor.AppendLine(string.Join("", Table.ColumnsName().Select(C => $"            this.{C} = {C};\n")));
                    ctor.AppendLine("            Mode = enMode.Update;");
                    ctor.AppendLine("        }");
                    return ctor.ToString();
            }
            return null;
        }

        private string CreateAddNewMethod(string TableName)
        {
            Table = DB.GetTable(TableName);
            if (Table == null)
                throw new ArgumentNullException($"Table with {TableName} not found");
            StringBuilder Code = new StringBuilder();

            Code.AppendLine("        private bool AddNew()\n        {");
            Code.AppendLine($"            {Table.TableColumns[0].Name} = {Table.Name + "Data"}.AddNew({string.Join(",", Table.ColumnsName().Skip(1))});");
            Code.AppendLine($"            return {Table.TableColumns[0].Name} != -1;");
            Code.AppendLine("        }");
            return Code.ToString();
        }

        private string CreateUpdateMethod(string TableName)
        {
            Table = DB.GetTable(TableName);
            if (Table == null)
                throw new ArgumentNullException($"Table with {TableName} not found");

            StringBuilder Code = new StringBuilder();

            Code.AppendLine("        private bool Update()\n        {");
            Code.AppendLine($"            return {TableName + "Data"}.Update({string.Join(",", Table.ColumnsName())});\n        }}");
            return Code.ToString();
        }

        private string CreateDeleteMethod(string TableName)
        {
            Table = DB.GetTable(TableName);
            if (Table == null)
                throw new ArgumentNullException($"Table with {TableName} doesn't exist");
            StringBuilder Code = new StringBuilder();

            Code.AppendLine($"        static public bool Delete(int {Table.TableColumns[0].Name})");
            Code.AppendLine($"                => {TableName + "Data"}.Delete({Table.TableColumns[0].Name});");
            return Code.ToString();
        }

        private string CreateFindMethod(string TableName)
        {
            Table = DB.GetTable(TableName);

            if (Table == null)
                throw new ArgumentNullException($"Table with {TableName} not found");

            StringBuilder Code = new StringBuilder();
            string[] ColumnNames = Table.ColumnsName();
            string[] Params = Table.Parameters();

            Code.AppendLine($"        static public {TableName} GetById(int ID)\n        {{");
            for (int i = 0; i < ColumnNames.Length; i++)
            {
                string Param = Params[i].EndsWith(",") ? Params[i].Remove(Params[i].Length - 1) : Params[i];
                if (i == 0) Param = null;
                if (Param != null)
                    Code.AppendLine($"                {Param} = {Table.AssignDefaultValue(ColumnNames[i])}; ");
            }

            Code.AppendLine($"\n                return {TableName + "Data"}.Find({ColumnNames[0]}, {string.Join(",", ColumnNames.Skip(1).Select(C => $"ref {C}"))}) ?");
            Code.AppendLine($"                    new {TableName}({string.Join(",", ColumnNames.Select(C => $"{C}"))}) : null;");
            Code.AppendLine("        }");
            return Code.ToString();
        }
        
        private string CreateGetAllMethod(string TableName)
        {
            Table = DB.GetTable(TableName);

            if (Table == null)
                throw new ArgumentNullException($"Table with {TableName} not found");
            StringBuilder Code = new StringBuilder();

            Code.AppendLine("        static public DataTable GetAll()\n        {");
            Code.AppendLine($"                DataTable dt = {TableName + "Data"}.GetAll();");
            Code.AppendLine("                return dt;\n        }");
            return Code.ToString();
        }

        public string CreateClass(string TableName)
        {
            StringBuilder Class = new StringBuilder();

            Class.AppendLine("using System;");
            Class.AppendLine("using DataLayer;");
            Class.AppendLine("namespace BuisnessLayer\n{");
            Class.AppendLine($"    public class {TableName}\n    {{");
            Class.AppendLine(CreateEnum());
            Class.AppendLine(string.Join("\n", CreateProprties(TableName).Select(P => P)));
            Class.AppendLine(CreateCtor(TableName, enMode.AddNew));
            Class.AppendLine(CreateCtor(TableName, enMode.Update));
            Class.AppendLine(CreateAddNewMethod(TableName));
            Class.AppendLine(CreateGetAllMethod(TableName));
            Class.AppendLine(CreateUpdateMethod(TableName));
            Class.AppendLine(CreateDeleteMethod(TableName));
            Class.AppendLine(CreateFindMethod(TableName));
            Class.AppendLine("    }\n}");
            return Class.ToString();
        }

        public BusinessLayerCreater(string Dbname)
        {
            try
            {
                DB = new Databases(Dbname);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public BusinessLayerCreater(Databases DB)
        {
            this.DB = DB;
        }
    }
}
