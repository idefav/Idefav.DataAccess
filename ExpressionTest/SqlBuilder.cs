using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class SqlBuilder
    {
        internal static string GetCreateDatabaseCommand(string catalog, string dataFilename, string logFilename)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("CREATE DATABASE {0}", (object)SqlIdentifier.QuoteIdentifier(catalog));
            if (dataFilename != null)
            {
                stringBuilder.AppendFormat(" ON PRIMARY (NAME='{0}', FILENAME='{1}')", (object)Path.GetFileName(dataFilename), (object)dataFilename);
                stringBuilder.AppendFormat(" LOG ON (NAME='{0}', FILENAME='{1}')", (object)Path.GetFileName(logFilename), (object)logFilename);
            }
            return stringBuilder.ToString();
        }

        internal static string GetDropDatabaseCommand(string catalog)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string format = "DROP DATABASE {0}";
            string str = SqlIdentifier.QuoteIdentifier(catalog);
            stringBuilder.AppendFormat(format, (object)str);
            return stringBuilder.ToString();
        }

        internal static string GetCreateSchemaForTableCommand(MetaTable table)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<string> list1 = new List<string>(SqlIdentifier.GetCompoundIdentifierParts(table.TableName));
            if (list1.Count < 2)
                return (string)null;
            List<string> list2 = list1;
            int index = list2.Count - 2;
            string str = list2[index];
            if (string.Compare(str, "DBO", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(str, "[DBO]", StringComparison.OrdinalIgnoreCase) != 0)
                stringBuilder.AppendFormat("CREATE SCHEMA {0}", (object)SqlIdentifier.QuoteIdentifier(str));
            return stringBuilder.ToString();
        }

        internal static string GetCreateTableCommand(MetaTable table)
        {
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder sb1 = new StringBuilder();
            SqlBuilder.BuildFieldDeclarations(table, sb1);
            stringBuilder.AppendFormat("CREATE TABLE {0}", (object)SqlIdentifier.QuoteCompoundIdentifier(table.TableName));
            stringBuilder.Append("(");
            stringBuilder.Append(sb1.ToString());
            StringBuilder sb2 = new StringBuilder();
            SqlBuilder.BuildPrimaryKey(table, sb2);
            if (sb2.Length > 0)
            {
                CultureInfo invariantCulture = CultureInfo.InvariantCulture;
                string format = "PK_{0}";
                object[] objArray = new object[1];
                int index = 0;
                string tableName = table.TableName;
                objArray[index] = (object)tableName;
                string s = string.Format((IFormatProvider)invariantCulture, format, objArray);
                stringBuilder.Append(", ");
                stringBuilder.AppendLine();
                stringBuilder.AppendFormat("  CONSTRAINT {0} PRIMARY KEY ({1})", (object)SqlIdentifier.QuoteIdentifier(s), (object)sb2.ToString());
            }
            stringBuilder.AppendLine();
            stringBuilder.Append("  )");
            return stringBuilder.ToString();
        }

        internal static void BuildFieldDeclarations(MetaTable table, StringBuilder sb)
        {
            int num = 0;
            Dictionary<object, string> memberNameToMappedName = new Dictionary<object, string>();
            foreach (MetaType type in table.RowType.InheritanceTypes)
                num += SqlBuilder.BuildFieldDeclarations(type, memberNameToMappedName, sb);
            if (num == 0)
                throw Error.CreateDatabaseFailedBecauseOfClassWithNoMembers((object)table.RowType.Type);
        }

        private static int BuildFieldDeclarations(MetaType type, Dictionary<object, string> memberNameToMappedName, StringBuilder sb)
        {
            int num = 0;
            foreach (MetaDataMember mm in type.DataMembers)
            {
                if (mm.IsDeclaredBy(type) && !mm.IsAssociation && mm.IsPersistent)
                {
                    object key = InheritanceRules.DistinguishedMemberName(mm.Member);
                    string str1;
                    if (memberNameToMappedName.TryGetValue(key, out str1))
                    {
                        if (str1 == mm.MappedName)
                            continue;
                    }
                    else
                        memberNameToMappedName.Add(key, mm.MappedName);
                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.AppendLine();
                    StringBuilder stringBuilder = sb;
                    CultureInfo invariantCulture = CultureInfo.InvariantCulture;
                    string format = "  {0} ";
                    object[] objArray = new object[1];
                    int index = 0;
                    string str2 = SqlIdentifier.QuoteCompoundIdentifier(mm.MappedName);
                    objArray[index] = (object)str2;
                    string str3 = string.Format((IFormatProvider)invariantCulture, format, objArray);
                    stringBuilder.Append(str3);
                    if (!string.IsNullOrEmpty(mm.Expression))
                        sb.Append("AS " + mm.Expression);
                    else
                        sb.Append(SqlBuilder.GetDbType(mm));
                    ++num;
                }
            }
            return num;
        }

        private static void BuildPrimaryKey(MetaTable table, StringBuilder sb)
        {
            foreach (MetaDataMember metaDataMember in table.RowType.IdentityMembers)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(SqlIdentifier.QuoteCompoundIdentifier(metaDataMember.MappedName));
            }
        }

        private static string BuildKey(IEnumerable<MetaDataMember> members)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (MetaDataMember metaDataMember in members)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(SqlIdentifier.QuoteCompoundIdentifier(metaDataMember.MappedName));
            }
            return stringBuilder.ToString();
        }

        internal static IEnumerable<string> GetCreateForeignKeyCommands(MetaTable table)
        {
            foreach (MetaType type in table.RowType.InheritanceTypes)
            {
                foreach (string str in SqlBuilder.GetCreateForeignKeyCommands(type))
                    yield return str;
            }
        }

        private static IEnumerable<string> GetCreateForeignKeyCommands(MetaType type)
        {
            string tableName = type.Table.TableName;
            foreach (MetaDataMember metaDataMember in type.DataMembers)
            {
                if (metaDataMember.IsDeclaredBy(type) && metaDataMember.IsAssociation)
                {
                    MetaAssociation association = metaDataMember.Association;
                    if (association.IsForeignKey)
                    {
                        StringBuilder stringBuilder1 = new StringBuilder();
                        string s1 = SqlBuilder.BuildKey((IEnumerable<MetaDataMember>)association.ThisKey);
                        string s2 = SqlBuilder.BuildKey((IEnumerable<MetaDataMember>)association.OtherKey);
                        string tableName1 = association.OtherType.Table.TableName;
                        string s3 = metaDataMember.MappedName;
                        if (s3 == metaDataMember.Name)
                        {
                            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
                            string format = "FK_{0}_{1}";
                            object[] objArray = new object[2];
                            int index1 = 0;
                            string str = tableName;
                            objArray[index1] = (object)str;
                            int index2 = 1;
                            string name = metaDataMember.Name;
                            objArray[index2] = (object)name;
                            s3 = string.Format((IFormatProvider)invariantCulture, format, objArray);
                        }
                        string str1 = "ALTER TABLE {0}" + Environment.NewLine + "  ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}({4})";
                        MetaDataMember otherMember = metaDataMember.Association.OtherMember;
                        if (otherMember != null)
                        {
                            string deleteRule = otherMember.Association.DeleteRule;
                            if (deleteRule != null)
                                str1 = str1 + Environment.NewLine + "  ON DELETE " + deleteRule;
                        }
                        StringBuilder stringBuilder2 = stringBuilder1;
                        string format1 = str1;
                        object[] objArray1 = new object[5];
                        int index3 = 0;
                        string str2 = SqlIdentifier.QuoteCompoundIdentifier(tableName);
                        objArray1[index3] = (object)str2;
                        int index4 = 1;
                        string str3 = SqlIdentifier.QuoteIdentifier(s3);
                        objArray1[index4] = (object)str3;
                        int index5 = 2;
                        string str4 = SqlIdentifier.QuoteCompoundIdentifier(s1);
                        objArray1[index5] = (object)str4;
                        int index6 = 3;
                        string str5 = SqlIdentifier.QuoteCompoundIdentifier(tableName1);
                        objArray1[index6] = (object)str5;
                        int index7 = 4;
                        string str6 = SqlIdentifier.QuoteCompoundIdentifier(s2);
                        objArray1[index7] = (object)str6;
                        stringBuilder2.AppendFormat(format1, objArray1);
                        yield return stringBuilder1.ToString();
                    }
                }
            }
        }

        private static string GetDbType(MetaDataMember mm)
        {
            string dbType = mm.DbType;
            if (dbType != null)
                return dbType;
            StringBuilder stringBuilder = new StringBuilder();
            Type type = mm.Type;
            bool canBeNull = mm.CanBeNull;
            if (type.IsValueType && SqlBuilder.IsNullable(type))
                type = type.GetGenericArguments()[0];
            if (mm.IsVersion)
                stringBuilder.Append("Timestamp");
            else if (mm.IsPrimaryKey && mm.IsDbGenerated)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                        if (!(type == typeof(Guid)))
                            throw Error.CouldNotDetermineDbGeneratedSqlType((object)type);
                        stringBuilder.Append("UniqueIdentifier");
                        break;
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                        stringBuilder.Append("SmallInt");
                        break;
                    case TypeCode.Byte:
                        stringBuilder.Append("TinyInt");
                        break;
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                        stringBuilder.Append("Int");
                        break;
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                        stringBuilder.Append("BigInt");
                        break;
                    case TypeCode.UInt64:
                    case TypeCode.Decimal:
                        stringBuilder.Append("Decimal(20)");
                        break;
                }
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                        if (type == typeof(Guid))
                        {
                            stringBuilder.Append("UniqueIdentifier");
                            break;
                        }
                        if (type == typeof(byte[]))
                        {
                            stringBuilder.Append("VarBinary(8000)");
                            break;
                        }
                        if (type == typeof(char[]))
                        {
                            stringBuilder.Append("NVarChar(4000)");
                            break;
                        }
                        if (type == typeof(DateTimeOffset))
                        {
                            stringBuilder.Append("DateTimeOffset");
                            break;
                        }
                        if (!(type == typeof(TimeSpan)))
                            throw Error.CouldNotDetermineSqlType((object)type);
                        stringBuilder.Append("Time");
                        break;
                    case TypeCode.Boolean:
                        stringBuilder.Append("Bit");
                        break;
                    case TypeCode.Char:
                        stringBuilder.Append("NChar(1)");
                        break;
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                        stringBuilder.Append("SmallInt");
                        break;
                    case TypeCode.Byte:
                        stringBuilder.Append("TinyInt");
                        break;
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                        stringBuilder.Append("Int");
                        break;
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                        stringBuilder.Append("BigInt");
                        break;
                    case TypeCode.UInt64:
                        stringBuilder.Append("Decimal(20)");
                        break;
                    case TypeCode.Single:
                        stringBuilder.Append("Real");
                        break;
                    case TypeCode.Double:
                        stringBuilder.Append("Float");
                        break;
                    case TypeCode.Decimal:
                        stringBuilder.Append("Decimal(29, 4)");
                        break;
                    case TypeCode.DateTime:
                        stringBuilder.Append("DateTime");
                        break;
                    case TypeCode.String:
                        stringBuilder.Append("NVarChar(4000)");
                        break;
                }
            }
            if (!canBeNull)
                stringBuilder.Append(" NOT NULL");
            if (mm.IsPrimaryKey && mm.IsDbGenerated)
            {
                if (type == typeof(Guid))
                    stringBuilder.Append(" DEFAULT NEWID()");
                else
                    stringBuilder.Append(" IDENTITY");
            }
            return stringBuilder.ToString();
        }

        internal static bool IsNullable(Type type)
        {
            if (type.IsGenericType)
                return typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition());
            return false;
        }
    }

    internal static class InheritanceRules
    {
        internal static object DistinguishedMemberName(MemberInfo mi)
        {
            PropertyInfo propertyInfo = mi as PropertyInfo;
            if (mi as FieldInfo != (FieldInfo)null)
                return (object)new MetaPosition(mi);
            if (!(propertyInfo != (PropertyInfo)null))
                throw Error.ArgumentOutOfRange("mi");
            MethodInfo methodInfo = (MethodInfo)null;
            if (propertyInfo.CanRead)
                methodInfo = propertyInfo.GetGetMethod();
            if (methodInfo == (MethodInfo)null && propertyInfo.CanWrite)
                methodInfo = propertyInfo.GetSetMethod();
            if ((!(methodInfo != (MethodInfo)null) ? 0 : (methodInfo.IsVirtual ? 1 : 0)) != 0)
                return (object)mi.Name;
            return (object)new MetaPosition(mi);
        }

        internal static bool AreSameMember(MemberInfo mi1, MemberInfo mi2)
        {
            return InheritanceRules.DistinguishedMemberName(mi1).Equals(InheritanceRules.DistinguishedMemberName(mi2));
        }

        internal static object InheritanceCodeForClientCompare(object rawCode, ProviderType providerType)
        {
            if (!providerType.IsFixedSize || !(rawCode.GetType() == typeof(string)))
                return rawCode;
            string str = (string)rawCode;
            if (providerType.Size.HasValue)
            {
                int length = str.Length;
                int? size = providerType.Size;
                int valueOrDefault = size.GetValueOrDefault();
                if ((length == valueOrDefault ? (!size.HasValue ? 1 : 0) : 1) != 0)
                    str = str.PadRight(providerType.Size.Value).Substring(0, providerType.Size.Value);
            }
            return (object)str;
        }
    }
}
