using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Idefav.Utility
{
    public class CodeCommon
    {
        private static string datatypefile = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\DatatypeMap.cfg";
        private static char[] hexDigits = new char[16]
        {
      '0',
      '1',
      '2',
      '3',
      '4',
      '5',
      '6',
      '7',
      '8',
      '9',
      'A',
      'B',
      'C',
      'D',
      'E',
      'F'
        };

        public static string Space(int num)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < num; ++index)
                stringBuilder.Append("\t");
            return stringBuilder.ToString();
        }

        public static string DbTypeToCS(string dbtype)
        {
            string str = "string";
            if (File.Exists(CodeCommon.datatypefile))
            {
                string valueFromCfg = DatatypeMap.GetValueFromCfg(CodeCommon.datatypefile, "DbToCS", dbtype.ToLower().Trim());
                str = !(valueFromCfg == "") ? valueFromCfg : dbtype.ToLower().Trim();
            }
            return str;
        }

        public static bool isValueType(string cstype)
        {
            bool flag = false;
            if (File.Exists(CodeCommon.datatypefile))
            {
                string valueFromCfg = DatatypeMap.GetValueFromCfg(CodeCommon.datatypefile, "ValueType", cstype.Trim());
                if (valueFromCfg == "true" || valueFromCfg == "是")
                    flag = true;
            }
            return flag;
        }

        public static string DbTypeLength(string dbtype, string datatype, string Length)
        {
            string str = "";
            switch (dbtype)
            {
                case "SQL2000":
                case "SQL2005":
                case "SQL2008":
                case "SQL2012":
                    str = CodeCommon.DbTypeLengthSQL(dbtype, datatype, Length);
                    break;
                case "Oracle":
                    str = CodeCommon.DbTypeLengthOra(datatype, Length);
                    break;
                case "MySQL":
                    str = CodeCommon.DbTypeLengthMySQL(datatype, Length);
                    break;
                case "OleDb":
                    str = CodeCommon.DbTypeLengthOleDb(datatype, Length);
                    break;
                case "SQLite":
                    str = CodeCommon.DbTypeLengthSQLite(datatype, Length);
                    break;
            }
            return str;
        }

        public static string GetDataTypeLenVal(string datatype, string Length)
        {
            string str = "";
            switch (datatype.Trim())
            {
                case "int":
                    str = !(Length == "") ? Length : "4";
                    goto case "image";
                case "char":
                    str = !(Length.Trim() == "") ? Length : "10";
                    goto case "image";
                case "nchar":
                    str = Length;
                    if (Length.Trim() == "")
                    {
                        str = "10";
                        goto case "image";
                    }
                    else
                        goto case "image";
                case "varchar":
                case "nvarchar":
                case "varbinary":
                    str = Length;
                    if (Length.Length == 0 || Length == "0" || Length == "-1")
                    {
                        str = "MAX";
                        goto case "image";
                    }
                    else
                        goto case "image";
                case "bit":
                    str = "1";
                    goto case "image";
                case "float":
                case "numeric":
                case "decimal":
                case "money":
                case "smallmoney":
                case "binary":
                case "smallint":
                case "bigint":
                    str = Length;
                    goto case "image";
                case "image":
                case "datetime":
                case "smalldatetime":
                case "ntext":
                case "text":
                    return str;
                default:
                    str = Length;
                    goto case "image";
            }
        }

        private static string DbTypeLengthSQL(string dbtype, string datatype, string Length)
        {
            string str1 = CodeCommon.GetDataTypeLenVal(datatype, Length);
            string str2;
            if (str1 != "")
            {
                if (str1 == "MAX")
                    str1 = "-1";
                str2 = CodeCommon.CSToProcType(dbtype, datatype) + "," + str1;
            }
            else
                str2 = CodeCommon.CSToProcType(dbtype, datatype);
            return str2;
        }

        private static string DbTypeLengthOra(string datatype, string Length)
        {
            string str = "";
            switch (datatype.Trim().ToLower())
            {
                case "number":
                    str = !(Length == "") ? Length : "4";
                    goto case "date";
                case "varchar2":
                    str = !(Length == "") ? Length : "50";
                    goto case "date";
                case "char":
                    str = !(Length == "") ? Length : "50";
                    goto case "date";
                case "date":
                case "nchar":
                case "nvarchar2":
                case "long":
                case "long raw":
                case "bfile":
                case "blob":
                    return !(str != "") ? CodeCommon.CSToProcType("Oracle", datatype) : CodeCommon.CSToProcType("Oracle", datatype) + "," + str;
                default:
                    str = Length;
                    goto case "date";
            }
        }

        private static string DbTypeLengthMySQL(string datatype, string Length)
        {
            string str = "";
            switch (datatype.Trim().ToLower())
            {
                case "number":
                    str = !(Length == "") ? Length : "4";
                    goto case "date";
                case "varchar2":
                    str = !(Length == "") ? Length : "50";
                    goto case "date";
                case "char":
                    str = !(Length == "") ? Length : "50";
                    goto case "date";
                case "date":
                case "nchar":
                case "nvarchar2":
                case "long":
                case "long raw":
                case "bfile":
                case "blob":
                    return !(str != "") ? CodeCommon.CSToProcType("MySQL", datatype) : CodeCommon.CSToProcType("MySQL", datatype) + "," + str;
                default:
                    str = Length;
                    goto case "date";
            }
        }

        private static string DbTypeLengthOleDb(string datatype, string Length)
        {
            string str = "";
            switch (datatype.Trim())
            {
                case "int":
                    str = !(Length == "") ? Length : "4";
                    goto case "image";
                case "varchar":
                    str = !(Length == "") ? Length : "50";
                    goto case "image";
                case "char":
                    str = !(Length == "") ? Length : "50";
                    goto case "image";
                case "bit":
                    str = "1";
                    goto case "image";
                case "float":
                case "numeric":
                case "decimal":
                case "money":
                case "smallmoney":
                case "binary":
                case "smallint":
                case "bigint":
                    str = Length;
                    goto case "image";
                case "image":
                case "datetime":
                case "smalldatetime":
                case "nchar":
                case "nvarchar":
                case "ntext":
                case "text":
                    return !(str != "") ? CodeCommon.CSToProcType("OleDb", datatype) : CodeCommon.CSToProcType("OleDb", datatype) + "," + str;
                default:
                    str = Length;
                    goto case "image";
            }
        }

        private static string DbTypeLengthSQLite(string datatype, string Length)
        {
            string str = "";
            switch (datatype.Trim())
            {
                case "int":
                case "integer":
                    str = !(Length == "") ? Length : "4";
                    goto case "image";
                case "varchar":
                    str = !(Length == "") ? Length : "50";
                    goto case "image";
                case "char":
                    str = !(Length == "") ? Length : "50";
                    goto case "image";
                case "bit":
                    str = "1";
                    goto case "image";
                case "float":
                case "numeric":
                case "decimal":
                case "money":
                case "smallmoney":
                case "binary":
                case "smallint":
                case "bigint":
                case "blob":
                    str = Length;
                    goto case "image";
                case "image":
                case "datetime":
                case "smalldatetime":
                case "nchar":
                case "nvarchar":
                case "ntext":
                case "text":
                case "time":
                case "date":
                case "boolean":
                    return !(str != "") ? CodeCommon.CSToProcType("SQLite", datatype) : CodeCommon.CSToProcType("SQLite", datatype) + "," + str;
                default:
                    str = Length;
                    goto case "image";
            }
        }

        public static string CSToProcType(string DbType, string cstype)
        {
            string str = cstype;
            switch (DbType)
            {
                case "SQL2000":
                case "SQL2005":
                case "SQL2008":
                case "SQL2012":
                    str = CodeCommon.CSToProcTypeSQL(cstype);
                    break;
                case "Oracle":
                    str = CodeCommon.CSToProcTypeOra(cstype);
                    break;
                case "MySQL":
                    str = CodeCommon.CSToProcTypeMySQL(cstype);
                    break;
                case "OleDb":
                    str = CodeCommon.CSToProcTypeOleDb(cstype);
                    break;
                case "SQLite":
                    str = CodeCommon.CSToProcTypeSQLite(cstype);
                    break;
            }
            return str;
        }

        private static string CSToProcTypeSQL(string cstype)
        {
            string str = cstype;
            if (File.Exists(CodeCommon.datatypefile))
            {
                string valueFromCfg = DatatypeMap.GetValueFromCfg(CodeCommon.datatypefile, "SQLDbType", cstype.ToLower().Trim());
                str = !(valueFromCfg == "") ? valueFromCfg : cstype.ToLower().Trim();
            }
            return str;
        }

        private static string CSToProcTypeOra(string cstype)
        {
            string str = cstype;
            if (File.Exists(CodeCommon.datatypefile))
            {
                string valueFromCfg = DatatypeMap.GetValueFromCfg(CodeCommon.datatypefile, "OraDbType", cstype.ToLower().Trim());
                str = !(valueFromCfg == "") ? valueFromCfg : cstype.ToLower().Trim();
            }
            return str;
        }

        private static string CSToProcTypeMySQL(string cstype)
        {
            string str = cstype;
            if (File.Exists(CodeCommon.datatypefile))
            {
                string valueFromCfg = DatatypeMap.GetValueFromCfg(CodeCommon.datatypefile, "MySQLDbType", cstype.ToLower().Trim());
                str = !(valueFromCfg == "") ? valueFromCfg : cstype.ToLower().Trim();
            }
            return str;
        }

        private static string CSToProcTypeOleDb(string cstype)
        {
            string str = cstype;
            if (File.Exists(CodeCommon.datatypefile))
            {
                string valueFromCfg = DatatypeMap.GetValueFromCfg(CodeCommon.datatypefile, "OleDbDbType", cstype.ToLower().Trim());
                str = !(valueFromCfg == "") ? valueFromCfg : cstype.ToLower().Trim();
            }
            return str;
        }

        private static string CSToProcTypeSQLite(string cstype)
        {
            string str = cstype;
            if (File.Exists(CodeCommon.datatypefile))
            {
                string valueFromCfg = DatatypeMap.GetValueFromCfg(CodeCommon.datatypefile, "SQLiteType", cstype.ToLower().Trim());
                str = !(valueFromCfg == "") ? valueFromCfg : cstype.ToLower().Trim();
            }
            return str;
        }

        public static bool IsAddMark(string columnType)
        {
            bool flag = false;
            if (File.Exists(CodeCommon.datatypefile))
            {
                string valueFromCfg = DatatypeMap.GetValueFromCfg(CodeCommon.datatypefile, "AddMark", columnType.ToLower().Trim());
                if (valueFromCfg == "true" || valueFromCfg == "是")
                    flag = true;
            }
            return flag;
        }

        public static string ToHexString(byte[] bytes)
        {
            char[] chArray = new char[bytes.Length * 2];
            for (int index = 0; index < bytes.Length; ++index)
            {
                int num = (int)bytes[index];
                chArray[index * 2] = CodeCommon.hexDigits[num >> 4];
                chArray[index * 2 + 1] = CodeCommon.hexDigits[num & 15];
            }
            return "0x" + new string(chArray).Substring(0, bytes.Length);
        }

        public static List<ColumnInfo> GetColumnInfos(DataTable dt)
        {
            List<ColumnInfo> list = new List<ColumnInfo>();
            if (dt == null)
                return (List<ColumnInfo>)null;
            ArrayList arrayList = new ArrayList();
            foreach (DataRow dataRow in (InternalDataCollectionBase)dt.Rows)
            {
                string str1 = dataRow["Colorder"].ToString();
                string str2 = dataRow["ColumnName"].ToString();
                string str3 = dataRow["TypeName"].ToString();
                string str4 = dataRow["IsIdentity"].ToString();
                string str5 = dataRow["IsPK"].ToString();
                string str6 = dataRow["Length"].ToString();
                string str7 = dataRow["Preci"].ToString();
                string str8 = dataRow["Scale"].ToString();
                string str9 = dataRow["cisNull"].ToString();
                string str10 = dataRow["DefaultVal"].ToString();
                string str11 = dataRow["DeText"].ToString();
                ColumnInfo columnInfo = new ColumnInfo();
                columnInfo.ColumnOrder = str1;
                columnInfo.ColumnName = str2;
                columnInfo.TypeName = str3;
                columnInfo.IsIdentity = str4 == "√";
                columnInfo.IsPrimaryKey = str5 == "√";
                columnInfo.Length = str6;
                columnInfo.Precision = str7;
                columnInfo.Scale = str8;
                columnInfo.Nullable = str9 == "√";
                columnInfo.DefaultVal = str10;
                columnInfo.Description = str11;
                if (!arrayList.Contains((object)str2))
                {
                    list.Add(columnInfo);
                    arrayList.Add((object)str2);
                }
            }
            return list;
        }

        public static DataTable GetColumnInfoDt(List<ColumnInfo> collist)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("colorder");
            dataTable.Columns.Add("ColumnName");
            dataTable.Columns.Add("TypeName");
            dataTable.Columns.Add("Length");
            dataTable.Columns.Add("Preci");
            dataTable.Columns.Add("Scale");
            dataTable.Columns.Add("IsIdentity");
            dataTable.Columns.Add("isPK");
            dataTable.Columns.Add("cisNull");
            dataTable.Columns.Add("defaultVal");
            dataTable.Columns.Add("deText");
            foreach (ColumnInfo columnInfo in collist)
            {
                DataRow row = dataTable.NewRow();
                row["colorder"] = (object)columnInfo.ColumnOrder;
                row["ColumnName"] = (object)columnInfo.ColumnName;
                row["TypeName"] = (object)columnInfo.TypeName;
                row["Length"] = (object)columnInfo.Length;
                row["Preci"] = (object)columnInfo.Precision;
                row["Scale"] = (object)columnInfo.Scale;
                row["IsIdentity"] = columnInfo.IsIdentity ? (object)"√" : (object)"";
                row["isPK"] = columnInfo.IsPrimaryKey ? (object)"√" : (object)"";
                row["cisNull"] = columnInfo.Nullable ? (object)"√" : (object)"";
                row["defaultVal"] = (object)columnInfo.DefaultVal;
                row["deText"] = (object)columnInfo.Description;
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

        public static ColumnInfo GetIdentityKey(List<ColumnInfo> keys)
        {
            foreach (ColumnInfo columnInfo in keys)
            {
                if (columnInfo.IsIdentity)
                    return columnInfo;
            }
            return (ColumnInfo)null;
        }

        public static bool HasNoIdentityKey(List<ColumnInfo> keys)
        {
            foreach (ColumnInfo columnInfo in keys)
            {
                if (columnInfo.IsPrimaryKey && !columnInfo.IsIdentity)
                    return true;
            }
            return false;
        }

        public static string DbParaHead(string DbType)
        {
            switch (DbType)
            {
                case "SQL2000":
                case "SQL2005":
                case "SQL2008":
                case "SQL2012":
                    return "Sql";
                case "Oracle":
                    return "Oracle";
                case "MySQL":
                    return "MySql";
                case "OleDb":
                    return "OleDb";
                case "SQLite":
                    return "SQLite";
                default:
                    return "Sql";
            }
        }

        public static string DbParaDbType(string DbType)
        {
            switch (DbType)
            {
                case "SQL2000":
                case "SQL2005":
                case "SQL2008":
                case "SQL2012":
                    return "SqlDbType";
                case "Oracle":
                    return "OracleType";
                case "OleDb":
                    return "OleDbType";
                case "MySQL":
                    return "MySqlDbType";
                case "SQLite":
                    return "DbType";
                default:
                    return "SqlDbType";
            }
        }

        public static string preParameter(string DbType)
        {
            string str = "@";
            if (File.Exists(CodeCommon.datatypefile))
            {
                string valueFromCfg = DatatypeMap.GetValueFromCfg(CodeCommon.datatypefile, "ParamePrefix", DbType.ToUpper().Trim());
                str = !(valueFromCfg == "") ? valueFromCfg : DbType.ToUpper().Trim();
            }
            return str;
        }

        public static bool IsHasIdentity(List<ColumnInfo> Keys)
        {
            bool flag = false;
            if (Keys.Count > 0)
            {
                foreach (ColumnInfo columnInfo in Keys)
                {
                    if (columnInfo.IsIdentity)
                        flag = true;
                }
            }
            return flag;
        }

        public static string GetWhereParameterExpression(List<ColumnInfo> keys, bool IdentityisPrior, string DbType)
        {
            StringPlus stringPlus = new StringPlus();
            ColumnInfo identityKey = CodeCommon.GetIdentityKey(keys);
            bool flag = CodeCommon.HasNoIdentityKey(keys);
            if (IdentityisPrior && identityKey != null || !flag && identityKey != null)
            {
                stringPlus.Append(identityKey.ColumnName + "=" + CodeCommon.preParameter(DbType) + identityKey.ColumnName);
            }
            else
            {
                foreach (ColumnInfo columnInfo in keys)
                {
                    if (columnInfo.IsPrimaryKey || !columnInfo.IsIdentity)
                        stringPlus.Append(columnInfo.ColumnName + "=" + CodeCommon.preParameter(DbType) + columnInfo.ColumnName + " and ");
                }
                stringPlus.DelLastChar("and");
            }
            return stringPlus.Value;
        }

        public static string GetPreParameter(List<ColumnInfo> keys, bool IdentityisPrior, string DbType)
        {
            StringPlus stringPlus1 = new StringPlus();
            StringPlus stringPlus2 = new StringPlus();
            stringPlus1.AppendSpaceLine(3, CodeCommon.DbParaHead(DbType) + "Parameter[] parameters = {");
            ColumnInfo identityKey = CodeCommon.GetIdentityKey(keys);
            bool flag = CodeCommon.HasNoIdentityKey(keys);
            if (IdentityisPrior && identityKey != null || !flag && identityKey != null)
            {
                stringPlus1.AppendSpaceLine(5, "new " + CodeCommon.DbParaHead(DbType) + "Parameter(\"" + CodeCommon.preParameter(DbType) + identityKey.ColumnName + "\", " + CodeCommon.DbParaDbType(DbType) + "." + CodeCommon.DbTypeLength(DbType, identityKey.TypeName, "") + ")");
                stringPlus2.AppendSpaceLine(3, "parameters[0].Value = " + identityKey.ColumnName + ";");
            }
            else
            {
                int num = 0;
                foreach (ColumnInfo columnInfo in keys)
                {
                    if (columnInfo.IsPrimaryKey || !columnInfo.IsIdentity)
                    {
                        stringPlus1.AppendSpaceLine(5, "new " + CodeCommon.DbParaHead(DbType) + "Parameter(\"" + CodeCommon.preParameter(DbType) + columnInfo.ColumnName + "\", " + CodeCommon.DbParaDbType(DbType) + "." + CodeCommon.DbTypeLength(DbType, columnInfo.TypeName, columnInfo.Length) + "),");
                        stringPlus2.AppendSpaceLine(3, "parameters[" + num.ToString() + "].Value = " + columnInfo.ColumnName + ";");
                        ++num;
                    }
                }
                stringPlus1.DelLastComma();
            }
            stringPlus1.AppendSpaceLine(3, "};");
            stringPlus1.Append(stringPlus2.Value);
            return stringPlus1.Value;
        }

        public static string GetInParameter(List<ColumnInfo> keys, bool IdentityisPrior)
        {
            StringPlus stringPlus = new StringPlus();
            ColumnInfo identityKey = CodeCommon.GetIdentityKey(keys);
            if (IdentityisPrior && identityKey != null)
            {
                stringPlus.Append(CodeCommon.DbTypeToCS(identityKey.TypeName) + " " + identityKey.ColumnName);
            }
            else
            {
                foreach (ColumnInfo columnInfo in keys)
                {
                    if (columnInfo.IsPrimaryKey || !columnInfo.IsIdentity)
                        stringPlus.Append(CodeCommon.DbTypeToCS(columnInfo.TypeName) + " " + columnInfo.ColumnName + ",");
                }
                stringPlus.DelLastComma();
            }
            return stringPlus.Value;
        }

        public static string GetFieldstrlist(List<ColumnInfo> keys, bool IdentityisPrior)
        {
            StringPlus stringPlus = new StringPlus();
            ColumnInfo identityKey = CodeCommon.GetIdentityKey(keys);
            if (IdentityisPrior && identityKey != null)
            {
                stringPlus.Append(identityKey.ColumnName);
            }
            else
            {
                foreach (ColumnInfo columnInfo in keys)
                {
                    if (columnInfo.IsPrimaryKey || !columnInfo.IsIdentity)
                        stringPlus.Append(columnInfo.ColumnName + ",");
                }
                stringPlus.DelLastComma();
            }
            return stringPlus.Value;
        }

        public static string GetFieldstrlistAdd(List<ColumnInfo> keys, bool IdentityisPrior)
        {
            StringPlus stringPlus = new StringPlus();
            ColumnInfo identityKey = CodeCommon.GetIdentityKey(keys);
            if (IdentityisPrior && identityKey != null)
            {
                stringPlus.Append(identityKey.ColumnName);
            }
            else
            {
                foreach (ColumnInfo columnInfo in keys)
                {
                    if (columnInfo.IsPrimaryKey || !columnInfo.IsIdentity)
                        stringPlus.Append(columnInfo.ColumnName + "+");
                }
                stringPlus.DelLastChar("+");
            }
            return stringPlus.Value;
        }

        public static string GetWhereExpression(List<ColumnInfo> keys, bool IdentityisPrior)
        {
            StringPlus stringPlus = new StringPlus();
            ColumnInfo identityKey = CodeCommon.GetIdentityKey(keys);
            if (IdentityisPrior && identityKey != null)
            {
                if (CodeCommon.IsAddMark(identityKey.TypeName))
                    stringPlus.Append(identityKey.ColumnName + "='\"+" + identityKey.ColumnName + "+\"'");
                else
                    stringPlus.Append(identityKey.ColumnName + "=\"+" + identityKey.ColumnName + "+\"");
            }
            else
            {
                foreach (ColumnInfo columnInfo in keys)
                {
                    if (columnInfo.IsPrimaryKey || !columnInfo.IsIdentity)
                    {
                        if (CodeCommon.IsAddMark(columnInfo.TypeName))
                            stringPlus.Append(columnInfo.ColumnName + "='\"+" + columnInfo.ColumnName + "+\"' and ");
                        else
                            stringPlus.Append(columnInfo.ColumnName + "=\"+" + columnInfo.ColumnName + "+\" and ");
                    }
                }
                stringPlus.DelLastChar("and");
            }
            return stringPlus.Value;
        }

        public static string GetModelWhereExpression(List<ColumnInfo> keys, bool IdentityisPrior)
        {
            StringPlus stringPlus = new StringPlus();
            ColumnInfo identityKey = CodeCommon.GetIdentityKey(keys);
            if (IdentityisPrior && identityKey != null)
            {
                if (CodeCommon.IsAddMark(identityKey.TypeName))
                    stringPlus.Append(identityKey.ColumnName + "='\"+ model." + identityKey.ColumnName + "+\"'");
                else
                    stringPlus.Append(identityKey.ColumnName + "=\"+ model." + identityKey.ColumnName + "+\"");
            }
            else
            {
                foreach (ColumnInfo columnInfo in keys)
                {
                    if (columnInfo.IsPrimaryKey || !columnInfo.IsIdentity)
                    {
                        if (CodeCommon.IsAddMark(columnInfo.TypeName))
                            stringPlus.Append(columnInfo.ColumnName + "='\"+ model." + columnInfo.ColumnName + "+\"' and ");
                        else
                            stringPlus.Append(columnInfo.ColumnName + "=\"+ model." + columnInfo.ColumnName + "+\" and ");
                    }
                }
                stringPlus.DelLastChar("and");
            }
            return stringPlus.Value;
        }

        public static string CutDescText(string descText, int cutLen, string ReplaceText)
        {
            string str;
            if (descText.Trim().Length > 0)
            {
                int val1_1 = descText.IndexOf(";");
                int val2_1 = descText.IndexOf("，");
                int val2_2 = descText.IndexOf(",");
                int val1_2 = Math.Min(val1_1, val2_1);
                if (val1_2 < 0)
                    val1_2 = Math.Max(val1_1, val2_1);
                int length = Math.Min(val1_2, val2_2);
                if (length < 0)
                    length = Math.Max(val1_1, val2_1);
                str = length <= 0 ? (descText.Trim().Length <= cutLen ? descText.Trim() : descText.Trim().Substring(0, cutLen)) : descText.Trim().Substring(0, length);
            }
            else
                str = ReplaceText;
            return str;
        }

        public static int CompareByintOrder(ColumnInfo x, ColumnInfo y)
        {
            if (x == null)
                return y == null ? 0 : -1;
            if (y == null)
                return 1;
            int num1;
            try
            {
                num1 = Convert.ToInt32(x.ColumnOrder);
            }
            catch
            {
                return -1;
            }
            int num2;
            try
            {
                num2 = Convert.ToInt32(y.ColumnOrder);
            }
            catch
            {
                return 1;
            }
            if (num1 < num2)
                return -1;
            return x == y ? 0 : 1;
        }

        public static int CompareByOrder(ColumnInfo x, ColumnInfo y)
        {
            if (x == null)
                return y == null ? 0 : -1;
            if (y == null)
                return 1;
            return x.ColumnOrder.CompareTo(y.ColumnOrder);
        }
    }
}
