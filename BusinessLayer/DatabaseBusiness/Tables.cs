using BusinessLayer;
using DataLayer;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System;

public class Tables
{
    private List<Columns> _tableColumns;

    public string TableCatalog { get; set; }
    public string Name { get; set; }
    public List<Columns> TableColumns
    {
        get
        {
            if (_tableColumns == null)
            {
                DataTable DT = DataBasesData.GetColumns(TableCatalog, Name);
                _tableColumns = new List<Columns>();
                foreach (DataRow dr in DT.Rows)
                {
                    _tableColumns.Add(new Columns()
                    {
                        Name = dr["COLUMN_NAME"].ToString(),
                        DataType = dr["DATA_TYPE"].ToString(),
                        IsNullable = dr["IS_NULLABLE"].ToString() == "YES"
                    });
                }
            }
            return _tableColumns;
        }
    }

    public string[] Parameters()
    {
        string[] arr = new string[TableColumns.Count];

        for (int i = 0; i < TableColumns.Count; i++)
        {
            StringBuilder Str = new StringBuilder();
            Str.Append($"{TableColumns[i].GetCsharpDataType()} {TableColumns[i].Name}");
            if (i != TableColumns.Count - 1)
                Str.Append(",");
            arr[i] = Str.ToString();
        }
        return arr;
    }

    public string[] ColumnsName()
    {
        string[] arr = new string[TableColumns.Count];
        for (int i = 0; i < TableColumns.Count; i++)
            arr[i] = $"{TableColumns[i].Name}";
        return arr;
    }

    public string ConvertTo(string ColumnName)
    {
        Columns Col = _tableColumns.FirstOrDefault(C => C.Name == ColumnName);
        switch (Col.GetCsharpDataType())
        {
            case "string":
                return "Convert.ToString(";
            case "int":
                return "Convert.ToInt32(";
            case "float":
                return "Convert.ToSingle(";
            case "double":
                return "Convert.ToDouble(";
            case "byte":
                return "Convert.ToByte(";
            case "bool":
                return "Convert.ToBoolean(";
            case "short":
                return "Convert.ToInt16(";
            case "long long":
                return "Convert.ToInt64(";
            case "decimal":
                return "Convert.ToDecimal(";
            case "DateTime":
                return "Convert.ToDateTime(";
            case "char":
                return "Convert.ToChar(";
        }
        return null;
    }

    public string AssignDefaultValue(string ColumnName)
    {
        Columns Col = _tableColumns.FirstOrDefault(C => C.Name == ColumnName);
        if (Col == null)
            throw new ArgumentNullException($"There is no Column with {ColumnName} Name");
        switch (Col.GetCsharpDataType())
        {
            case "int":
            case "decimal":
            case "float":
            case "double":
            case "short":
            case "byte":
            case "long":
            case "long long":
                return "0";
            case "string":
                return "string.Empty";
            case "DateTime":
                return "new DateTime()";
            case "char":
                return "\\0";
            case "bool":
                return "false";
        }
        return null;
    }

    public Tables()
    {
        
    }

    public Tables(string DatabaseName, string TableName)
    {
        this.TableCatalog = DatabaseName;
        this.Name = TableName;
    }
}
