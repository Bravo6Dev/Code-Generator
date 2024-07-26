using System;
using System.Net.Http.Headers;
using System.Text;

namespace BusinessLayer
{
    public class Columns
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public string Nullable 
        { 
            get
            {
                return IsNullable ? "NULL" : "NOT NULL";
            }
        }

        public string GetCsharpDataType()
        {
            switch (DataType)
            {
                case "tinyint":
                    return "byte";
                case "smallint":
                    return "short";
                case "int":
                    return "int";
                case "bigint":
                    return "long long";
                case "bit":
                    return "bool";
                case "decimal":
                case "numeric":
                case "money":
                case "smallmoney":
                    return "decimal";
                case "float":
                    return "float";
                case "real":
                    return "double";
                case "datetime":
                case "time":
                case "datetime2":
                case "datetimeoffest":
                case "smalldatetime":
                case "date":
                    return "DateTime";
                case "char":
                case "nchar":
                    return "char";
                case "varchar":
                case "nvarchar":
                case "text":
                case "ntext":
                    return "string";
            }
            return null;
        }
    }
}