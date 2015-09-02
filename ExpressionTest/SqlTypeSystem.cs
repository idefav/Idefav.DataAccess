using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExpressionTest
{
    internal static class SqlTypeSystem
    {
        private static readonly SqlTypeSystem.SqlType theBigInt = new SqlTypeSystem.SqlType(SqlDbType.BigInt);
        private static readonly SqlTypeSystem.SqlType theBit = new SqlTypeSystem.SqlType(SqlDbType.Bit);
        private static readonly SqlTypeSystem.SqlType theChar = new SqlTypeSystem.SqlType(SqlDbType.Char);
        private static readonly SqlTypeSystem.SqlType theDateTime = new SqlTypeSystem.SqlType(SqlDbType.DateTime);
        private static readonly SqlTypeSystem.SqlType theDate = new SqlTypeSystem.SqlType(SqlDbType.Date);
        private static readonly SqlTypeSystem.SqlType theTime = new SqlTypeSystem.SqlType(SqlDbType.Time);
        private static readonly SqlTypeSystem.SqlType theDateTime2 = new SqlTypeSystem.SqlType(SqlDbType.DateTime2);
        private static readonly SqlTypeSystem.SqlType theDateTimeOffset = new SqlTypeSystem.SqlType(SqlDbType.DateTimeOffset);
        private static readonly SqlTypeSystem.SqlType theDefaultDecimal = new SqlTypeSystem.SqlType(SqlDbType.Decimal, 29, 4);
        private static readonly SqlTypeSystem.SqlType theFloat = new SqlTypeSystem.SqlType(SqlDbType.Float);
        private static readonly SqlTypeSystem.SqlType theInt = new SqlTypeSystem.SqlType(SqlDbType.Int);
        private static readonly SqlTypeSystem.SqlType theMoney = new SqlTypeSystem.SqlType(SqlDbType.Money, 19, 4);
        private static readonly SqlTypeSystem.SqlType theReal = new SqlTypeSystem.SqlType(SqlDbType.Real);
        private static readonly SqlTypeSystem.SqlType theUniqueIdentifier = new SqlTypeSystem.SqlType(SqlDbType.UniqueIdentifier);
        private static readonly SqlTypeSystem.SqlType theSmallDateTime = new SqlTypeSystem.SqlType(SqlDbType.SmallDateTime);
        private static readonly SqlTypeSystem.SqlType theSmallInt = new SqlTypeSystem.SqlType(SqlDbType.SmallInt);
        private static readonly SqlTypeSystem.SqlType theSmallMoney = new SqlTypeSystem.SqlType(SqlDbType.SmallMoney, 10, 4);
        private static readonly SqlTypeSystem.SqlType theTimestamp = new SqlTypeSystem.SqlType(SqlDbType.Timestamp);
        private static readonly SqlTypeSystem.SqlType theTinyInt = new SqlTypeSystem.SqlType(SqlDbType.TinyInt);
        private static readonly SqlTypeSystem.SqlType theXml = new SqlTypeSystem.SqlType(SqlDbType.Xml, new int?(-1));
        private static readonly SqlTypeSystem.SqlType theText = new SqlTypeSystem.SqlType(SqlDbType.Text, new int?(-1));
        private static readonly SqlTypeSystem.SqlType theNText = new SqlTypeSystem.SqlType(SqlDbType.NText, new int?(-1));
        private static readonly SqlTypeSystem.SqlType theImage = new SqlTypeSystem.SqlType(SqlDbType.Image, new int?(-1));
        internal const short LargeTypeSizeIndicator = (short)-1;
        private const int defaultDecimalPrecision = 29;
        private const int defaultDecimalScale = 4;

        internal static TypeSystemProvider Create2000Provider()
        {
            return (TypeSystemProvider)new SqlTypeSystem.Sql2000Provider();
        }

        internal static TypeSystemProvider Create2005Provider()
        {
            return (TypeSystemProvider)new SqlTypeSystem.Sql2005Provider();
        }

        internal static TypeSystemProvider Create2008Provider()
        {
            return (TypeSystemProvider)new SqlTypeSystem.Sql2008Provider();
        }

        internal static TypeSystemProvider CreateCEProvider()
        {
            return (TypeSystemProvider)new SqlTypeSystem.SqlCEProvider();
        }

        private static ProviderType Create(SqlDbType type, int size)
        {
            return (ProviderType)new SqlTypeSystem.SqlType(type, new int?(size));
        }

        private static ProviderType Create(SqlDbType type, int precision, int scale)
        {
            if (type != SqlDbType.Decimal && precision == 0 && scale == 0 || type == SqlDbType.Decimal && precision == 29 && scale == 4)
                return SqlTypeSystem.Create(type);
            return (ProviderType)new SqlTypeSystem.SqlType(type, precision, scale);
        }

        private static ProviderType Create(SqlDbType type)
        {
            switch (type)
            {
                case SqlDbType.BigInt:
                    return (ProviderType)SqlTypeSystem.theBigInt;
                case SqlDbType.Bit:
                    return (ProviderType)SqlTypeSystem.theBit;
                case SqlDbType.Char:
                    return (ProviderType)SqlTypeSystem.theChar;
                case SqlDbType.DateTime:
                    return (ProviderType)SqlTypeSystem.theDateTime;
                case SqlDbType.Decimal:
                    return (ProviderType)SqlTypeSystem.theDefaultDecimal;
                case SqlDbType.Float:
                    return (ProviderType)SqlTypeSystem.theFloat;
                case SqlDbType.Image:
                    return (ProviderType)SqlTypeSystem.theImage;
                case SqlDbType.Int:
                    return (ProviderType)SqlTypeSystem.theInt;
                case SqlDbType.Money:
                    return (ProviderType)SqlTypeSystem.theMoney;
                case SqlDbType.NText:
                    return (ProviderType)SqlTypeSystem.theNText;
                case SqlDbType.Real:
                    return (ProviderType)SqlTypeSystem.theReal;
                case SqlDbType.UniqueIdentifier:
                    return (ProviderType)SqlTypeSystem.theUniqueIdentifier;
                case SqlDbType.SmallDateTime:
                    return (ProviderType)SqlTypeSystem.theSmallDateTime;
                case SqlDbType.SmallInt:
                    return (ProviderType)SqlTypeSystem.theSmallInt;
                case SqlDbType.SmallMoney:
                    return (ProviderType)SqlTypeSystem.theSmallMoney;
                case SqlDbType.Text:
                    return (ProviderType)SqlTypeSystem.theText;
                case SqlDbType.Timestamp:
                    return (ProviderType)SqlTypeSystem.theTimestamp;
                case SqlDbType.TinyInt:
                    return (ProviderType)SqlTypeSystem.theTinyInt;
                case SqlDbType.Xml:
                    return (ProviderType)SqlTypeSystem.theXml;
                case SqlDbType.Date:
                    return (ProviderType)SqlTypeSystem.theDate;
                case SqlDbType.Time:
                    return (ProviderType)SqlTypeSystem.theTime;
                case SqlDbType.DateTime2:
                    return (ProviderType)SqlTypeSystem.theDateTime2;
                case SqlDbType.DateTimeOffset:
                    return (ProviderType)SqlTypeSystem.theDateTimeOffset;
                default:
                    return (ProviderType)new SqlTypeSystem.SqlType(type);
            }
        }

        private static Type GetClosestRuntimeType(SqlDbType sqlDbType)
        {
            switch (sqlDbType)
            {
                case SqlDbType.BigInt:
                    return typeof(long);
                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    return typeof(byte[]);
                case SqlDbType.Bit:
                    return typeof(bool);
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                    return typeof(string);
                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Date:
                case SqlDbType.DateTime2:
                    return typeof(DateTime);
                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return typeof(Decimal);
                case SqlDbType.Float:
                    return typeof(double);
                case SqlDbType.Int:
                    return typeof(int);
                case SqlDbType.Real:
                    return typeof(float);
                case SqlDbType.UniqueIdentifier:
                    return typeof(Guid);
                case SqlDbType.SmallInt:
                    return typeof(short);
                case SqlDbType.TinyInt:
                    return typeof(byte);
                case SqlDbType.Udt:
                    throw Error.UnexpectedTypeCode((object)SqlDbType.Udt);
                case SqlDbType.Time:
                    return typeof(TimeSpan);
                case SqlDbType.DateTimeOffset:
                    return typeof(DateTimeOffset);
                default:
                    return typeof(object);
            }
        }

        public class SqlType : ProviderType
        {
            protected SqlDbType sqlDbType;
            protected int? size;
            protected int precision;
            protected int scale;
            private Type runtimeOnlyType;
            private int? applicationTypeIndex;

            internal override bool IsUnicodeType
            {
                get
                {
                    switch (this.SqlDbType)
                    {
                        case SqlDbType.NChar:
                        case SqlDbType.NText:
                        case SqlDbType.NVarChar:
                            return true;
                        default:
                            return false;
                    }
                }
            }

            internal override bool IsRuntimeOnlyType
            {
                get
                {
                    return this.runtimeOnlyType != (Type)null;
                }
            }

            internal override bool IsApplicationType
            {
                get
                {
                    return this.applicationTypeIndex.HasValue;
                }
            }

            internal override bool CanSuppressSizeForConversionToString
            {
                get
                {
                    int num1 = 30;
                    if (this.IsLargeType)
                        return false;
                    if (!this.IsChar && !this.IsString && this.IsFixedSize)
                    {
                        int? size1 = this.Size;
                        int num2 = 0;
                        if ((size1.GetValueOrDefault() > num2 ? (size1.HasValue ? 1 : 0) : 0) != 0)
                        {
                            int? size2 = this.Size;
                            int num3 = num1;
                            if (size2.GetValueOrDefault() >= num3)
                                return false;
                            return size2.HasValue;
                        }
                    }
                    switch (this.SqlDbType)
                    {
                        case SqlDbType.Float:
                        case SqlDbType.Int:
                        case SqlDbType.Money:
                        case SqlDbType.Real:
                        case SqlDbType.SmallInt:
                        case SqlDbType.SmallMoney:
                        case SqlDbType.TinyInt:
                        case SqlDbType.BigInt:
                        case SqlDbType.Bit:
                            return true;
                        default:
                            return false;
                    }
                }
            }

            private bool IsTypeKnownByProvider
            {
                get
                {
                    if (!this.IsApplicationType)
                        return !this.IsRuntimeOnlyType;
                    return false;
                }
            }

            internal override bool SupportsComparison
            {
                get
                {
                    switch (this.sqlDbType)
                    {
                        case SqlDbType.Text:
                        case SqlDbType.Xml:
                        case SqlDbType.Image:
                        case SqlDbType.NText:
                            return false;
                        default:
                            return true;
                    }
                }
            }

            internal override bool SupportsLength
            {
                get
                {
                    switch (this.sqlDbType)
                    {
                        case SqlDbType.Text:
                        case SqlDbType.Xml:
                        case SqlDbType.Image:
                        case SqlDbType.NText:
                            return false;
                        default:
                            return true;
                    }
                }
            }

            internal override bool IsLargeType
            {
                get
                {
                    switch (this.sqlDbType)
                    {
                        case SqlDbType.NVarChar:
                        case SqlDbType.VarBinary:
                        case SqlDbType.VarChar:
                            int? nullable = this.size;
                            int num = -1;
                            if (nullable.GetValueOrDefault() != num)
                                return false;
                            return nullable.HasValue;
                        case SqlDbType.Text:
                        case SqlDbType.Xml:
                        case SqlDbType.Image:
                        case SqlDbType.NText:
                            return true;
                        default:
                            return false;
                    }
                }
            }

            internal override bool HasPrecisionAndScale
            {
                get
                {
                    switch (this.SqlDbType)
                    {
                        case SqlDbType.SmallMoney:
                        case SqlDbType.Time:
                        case SqlDbType.DateTime2:
                        case SqlDbType.DateTimeOffset:
                        case SqlDbType.Decimal:
                        case SqlDbType.Float:
                        case SqlDbType.Money:
                        case SqlDbType.Real:
                            return true;
                        default:
                            return false;
                    }
                }
            }

            internal override bool HasSizeOrIsLarge
            {
                get
                {
                    if (!this.size.HasValue)
                        return this.IsLargeType;
                    return true;
                }
            }

            internal override int? Size
            {
                get
                {
                    return this.size;
                }
            }

            internal int Precision
            {
                get
                {
                    return this.precision;
                }
            }

            internal int Scale
            {
                get
                {
                    return this.scale;
                }
            }

            internal override bool IsFixedSize
            {
                get
                {
                    switch (this.sqlDbType)
                    {
                        case SqlDbType.NVarChar:
                        case SqlDbType.Text:
                        case SqlDbType.VarBinary:
                        case SqlDbType.VarChar:
                        case SqlDbType.Xml:
                        case SqlDbType.Image:
                        case SqlDbType.NText:
                            return false;
                        default:
                            return true;
                    }
                }
            }

            internal SqlDbType SqlDbType
            {
                get
                {
                    return this.sqlDbType;
                }
            }

            internal override bool IsOrderable
            {
                get
                {
                    if (this.IsRuntimeOnlyType)
                        return false;
                    switch (this.sqlDbType)
                    {
                        case SqlDbType.Text:
                        case SqlDbType.Xml:
                        case SqlDbType.Image:
                        case SqlDbType.NText:
                            return false;
                        default:
                            return true;
                    }
                }
            }

            internal override bool IsGroupable
            {
                get
                {
                    if (this.IsRuntimeOnlyType)
                        return false;
                    switch (this.sqlDbType)
                    {
                        case SqlDbType.Text:
                        case SqlDbType.Xml:
                        case SqlDbType.Image:
                        case SqlDbType.NText:
                            return false;
                        default:
                            return true;
                    }
                }
            }

            internal override bool CanBeColumn
            {
                get
                {
                    if (!this.IsApplicationType)
                        return !this.IsRuntimeOnlyType;
                    return false;
                }
            }

            internal override bool CanBeParameter
            {
                get
                {
                    if (!this.IsApplicationType)
                        return !this.IsRuntimeOnlyType;
                    return false;
                }
            }

            internal override bool IsNumeric
            {
                get
                {
                    if (this.IsApplicationType || this.IsRuntimeOnlyType)
                        return false;
                    switch (this.SqlDbType)
                    {
                        case SqlDbType.BigInt:
                        case SqlDbType.Bit:
                        case SqlDbType.Decimal:
                        case SqlDbType.Float:
                        case SqlDbType.Int:
                        case SqlDbType.Money:
                        case SqlDbType.Real:
                        case SqlDbType.SmallInt:
                        case SqlDbType.SmallMoney:
                        case SqlDbType.TinyInt:
                            return true;
                        default:
                            return false;
                    }
                }
            }

            internal override bool IsChar
            {
                get
                {
                    if (this.IsApplicationType || this.IsRuntimeOnlyType)
                        return false;
                    switch (this.SqlDbType)
                    {
                        case SqlDbType.NVarChar:
                        case SqlDbType.VarChar:
                        case SqlDbType.Char:
                        case SqlDbType.NChar:
                            int? size = this.Size;
                            int num = 1;
                            if (size.GetValueOrDefault() != num)
                                return false;
                            return size.HasValue;
                        default:
                            return false;
                    }
                }
            }

            internal override bool IsString
            {
                get
                {
                    if (this.IsApplicationType || this.IsRuntimeOnlyType)
                        return false;
                    switch (this.SqlDbType)
                    {
                        case SqlDbType.Text:
                        case SqlDbType.NText:
                            return true;
                        case SqlDbType.VarChar:
                        case SqlDbType.Char:
                        case SqlDbType.NChar:
                        case SqlDbType.NVarChar:
                            int? size = this.Size;
                            int num1 = 0;
                            if ((size.GetValueOrDefault() == num1 ? (size.HasValue ? 1 : 0) : 0) == 0)
                            {
                                size = this.Size;
                                int num2 = 1;
                                if ((size.GetValueOrDefault() > num2 ? (size.HasValue ? 1 : 0) : 0) == 0)
                                {
                                    size = this.Size;
                                    int num3 = -1;
                                    if (size.GetValueOrDefault() != num3)
                                        return false;
                                    return size.HasValue;
                                }
                            }
                            return true;
                        default:
                            return false;
                    }
                }
            }

            internal SqlTypeSystem.SqlType.TypeCategory Category
            {
                get
                {
                    switch (this.SqlDbType)
                    {
                        case SqlDbType.BigInt:
                        case SqlDbType.Bit:
                        case SqlDbType.Decimal:
                        case SqlDbType.Float:
                        case SqlDbType.Int:
                        case SqlDbType.Money:
                        case SqlDbType.Real:
                        case SqlDbType.SmallInt:
                        case SqlDbType.SmallMoney:
                        case SqlDbType.TinyInt:
                            return SqlTypeSystem.SqlType.TypeCategory.Numeric;
                        case SqlDbType.Binary:
                        case SqlDbType.Timestamp:
                        case SqlDbType.VarBinary:
                            return SqlTypeSystem.SqlType.TypeCategory.Binary;
                        case SqlDbType.Char:
                        case SqlDbType.NChar:
                        case SqlDbType.NVarChar:
                        case SqlDbType.VarChar:
                            return SqlTypeSystem.SqlType.TypeCategory.Char;
                        case SqlDbType.DateTime:
                        case SqlDbType.SmallDateTime:
                        case SqlDbType.Date:
                        case SqlDbType.Time:
                        case SqlDbType.DateTime2:
                        case SqlDbType.DateTimeOffset:
                            return SqlTypeSystem.SqlType.TypeCategory.DateTime;
                        case SqlDbType.Image:
                            return SqlTypeSystem.SqlType.TypeCategory.Image;
                        case SqlDbType.NText:
                        case SqlDbType.Text:
                            return SqlTypeSystem.SqlType.TypeCategory.Text;
                        case SqlDbType.UniqueIdentifier:
                            return SqlTypeSystem.SqlType.TypeCategory.UniqueIdentifier;
                        case SqlDbType.Variant:
                            return SqlTypeSystem.SqlType.TypeCategory.Variant;
                        case SqlDbType.Xml:
                            return SqlTypeSystem.SqlType.TypeCategory.Xml;
                        case SqlDbType.Udt:
                            return SqlTypeSystem.SqlType.TypeCategory.Udt;
                        default:
                            throw Error.UnexpectedTypeCode((object)this);
                    }
                }
            }

            internal SqlType(SqlDbType type)
            {
                this.sqlDbType = type;
            }

            internal SqlType(SqlDbType type, int? size)
            {
                this.sqlDbType = type;
                this.size = size;
            }

            internal SqlType(SqlDbType type, int precision, int scale)
            {
                this.sqlDbType = type;
                this.precision = precision;
                this.scale = scale;
            }

            internal SqlType(Type type)
            {
                this.runtimeOnlyType = type;
            }

            internal SqlType(int applicationTypeIndex)
            {
                this.applicationTypeIndex = new int?(applicationTypeIndex);
            }

            protected static string KeyValue<T>(string key, T value)
            {
                if ((object)value != null)
                    return key + "=" + value.ToString() + " ";
                return string.Empty;
            }

            protected static string SingleValue<T>(T value)
            {
                if ((object)value != null)
                    return value.ToString() + " ";
                return string.Empty;
            }

            public override string ToString()
            {
                string[] strArray = new string[14];
                int index1 = 0;
                string str1 = SqlTypeSystem.SqlType.SingleValue<Type>(this.GetClosestRuntimeType());
                strArray[index1] = str1;
                int index2 = 1;
                string str2 = SqlTypeSystem.SqlType.SingleValue<string>(this.ToQueryString());
                strArray[index2] = str2;
                int index3 = 2;
                string str3 = SqlTypeSystem.SqlType.KeyValue<bool>("IsApplicationType", this.IsApplicationType);
                strArray[index3] = str3;
                int index4 = 3;
                string str4 = SqlTypeSystem.SqlType.KeyValue<bool>("IsUnicodeType", this.IsUnicodeType);
                strArray[index4] = str4;
                int index5 = 4;
                string str5 = SqlTypeSystem.SqlType.KeyValue<bool>("IsRuntimeOnlyType", this.IsRuntimeOnlyType);
                strArray[index5] = str5;
                int index6 = 5;
                string str6 = SqlTypeSystem.SqlType.KeyValue<bool>("SupportsComparison", this.SupportsComparison);
                strArray[index6] = str6;
                int index7 = 6;
                string str7 = SqlTypeSystem.SqlType.KeyValue<bool>("SupportsLength", this.SupportsLength);
                strArray[index7] = str7;
                int index8 = 7;
                string str8 = SqlTypeSystem.SqlType.KeyValue<bool>("IsLargeType", this.IsLargeType);
                strArray[index8] = str8;
                int index9 = 8;
                string str9 = SqlTypeSystem.SqlType.KeyValue<bool>("IsFixedSize", this.IsFixedSize);
                strArray[index9] = str9;
                int index10 = 9;
                string str10 = SqlTypeSystem.SqlType.KeyValue<bool>("IsOrderable", this.IsOrderable);
                strArray[index10] = str10;
                int index11 = 10;
                string str11 = SqlTypeSystem.SqlType.KeyValue<bool>("IsGroupable", this.IsGroupable);
                strArray[index11] = str11;
                int index12 = 11;
                string str12 = SqlTypeSystem.SqlType.KeyValue<bool>("IsNumeric", this.IsNumeric);
                strArray[index12] = str12;
                int index13 = 12;
                string str13 = SqlTypeSystem.SqlType.KeyValue<bool>("IsChar", this.IsChar);
                strArray[index13] = str13;
                int index14 = 13;
                string str14 = SqlTypeSystem.SqlType.KeyValue<bool>("IsString", this.IsString);
                strArray[index14] = str14;
                return string.Concat(strArray);
            }

            internal override ProviderType GetNonUnicodeEquivalent()
            {
                if (!this.IsUnicodeType)
                    return (ProviderType)this;
                switch (this.SqlDbType)
                {
                    case SqlDbType.NChar:
                        return (ProviderType)new SqlTypeSystem.SqlType(SqlDbType.Char, this.Size);
                    case SqlDbType.NText:
                        return (ProviderType)new SqlTypeSystem.SqlType(SqlDbType.Text);
                    case SqlDbType.NVarChar:
                        return (ProviderType)new SqlTypeSystem.SqlType(SqlDbType.VarChar, this.Size);
                    default:
                        return (ProviderType)this;
                }
            }

            internal override bool IsApplicationTypeOf(int index)
            {
                if (!this.IsApplicationType)
                    return false;
                int? nullable = this.applicationTypeIndex;
                int num = index;
                if (nullable.GetValueOrDefault() != num)
                    return false;
                return nullable.HasValue;
            }

            internal override int ComparePrecedenceTo(ProviderType type)
            {
                SqlTypeSystem.SqlType sqlType = (SqlTypeSystem.SqlType)type;
                return (this.IsTypeKnownByProvider ? SqlTypeSystem.SqlType.GetTypeCoercionPrecedence(this.SqlDbType) : int.MinValue).CompareTo(sqlType.IsTypeKnownByProvider ? SqlTypeSystem.SqlType.GetTypeCoercionPrecedence(sqlType.SqlDbType) : int.MinValue);
            }

            internal override bool IsSameTypeFamily(ProviderType type)
            {
                SqlTypeSystem.SqlType sqlType = (SqlTypeSystem.SqlType)type;
                if (this.IsApplicationType || sqlType.IsApplicationType)
                    return false;
                return this.Category == sqlType.Category;
            }

            internal override bool AreValuesEqual(object o1, object o2)
            {
                if (o1 == null || o2 == null)
                    return false;
                switch (this.sqlDbType)
                {
                    case SqlDbType.NVarChar:
                    case SqlDbType.Text:
                    case SqlDbType.VarChar:
                    case SqlDbType.Char:
                    case SqlDbType.NChar:
                        string str1 = o1 as string;
                        if (str1 != null)
                        {
                            string str2 = o2 as string;
                            if (str2 != null)
                            {
                                string str3 = str1;
                                char[] chArray1 = new char[1];
                                int index1 = 0;
                                int num1 = 32;
                                chArray1[index1] = (char)num1;
                                string str4 = str3.TrimEnd(chArray1);
                                string str5 = str2;
                                char[] chArray2 = new char[1];
                                int index2 = 0;
                                int num2 = 32;
                                chArray2[index2] = (char)num2;
                                string str6 = str5.TrimEnd(chArray2);
                                int num3 = 4;
                                return str4.Equals(str6, (StringComparison)num3);
                            }
                            break;
                        }
                        break;
                }
                return o1.Equals(o2);
            }

            internal override Type GetClosestRuntimeType()
            {
                if (this.runtimeOnlyType != (Type)null)
                    return this.runtimeOnlyType;
                return SqlTypeSystem.GetClosestRuntimeType(this.sqlDbType);
            }

            internal override string ToQueryString()
            {
                return this.ToQueryString(QueryFormatOptions.None);
            }

            internal override string ToQueryString(QueryFormatOptions formatFlags)
            {
                if (this.runtimeOnlyType != (Type)null)
                    return this.runtimeOnlyType.ToString();
                StringBuilder stringBuilder = new StringBuilder();
                switch (this.sqlDbType)
                {
                    case SqlDbType.BigInt:
                    case SqlDbType.Bit:
                    case SqlDbType.DateTime:
                    case SqlDbType.Image:
                    case SqlDbType.Int:
                    case SqlDbType.Money:
                    case SqlDbType.NText:
                    case SqlDbType.UniqueIdentifier:
                    case SqlDbType.SmallDateTime:
                    case SqlDbType.SmallInt:
                    case SqlDbType.SmallMoney:
                    case SqlDbType.Text:
                    case SqlDbType.Timestamp:
                    case SqlDbType.TinyInt:
                    case SqlDbType.Xml:
                    case SqlDbType.Udt:
                    case SqlDbType.Date:
                    case SqlDbType.Time:
                    case SqlDbType.DateTime2:
                    case SqlDbType.DateTimeOffset:
                        stringBuilder.Append(this.sqlDbType.ToString());
                        break;
                    case SqlDbType.Binary:
                    case SqlDbType.Char:
                    case SqlDbType.NChar:
                        stringBuilder.Append((object)this.sqlDbType);
                        if ((formatFlags & QueryFormatOptions.SuppressSize) == QueryFormatOptions.None)
                        {
                            stringBuilder.Append("(");
                            stringBuilder.Append((object)this.size);
                            stringBuilder.Append(")");
                            break;
                        }
                        break;
                    case SqlDbType.Decimal:
                    case SqlDbType.Float:
                    case SqlDbType.Real:
                        stringBuilder.Append((object)this.sqlDbType);
                        if (this.precision != 0)
                        {
                            stringBuilder.Append("(");
                            stringBuilder.Append(this.precision);
                            if (this.scale != 0)
                            {
                                stringBuilder.Append(",");
                                stringBuilder.Append(this.scale);
                            }
                            stringBuilder.Append(")");
                            break;
                        }
                        break;
                    case SqlDbType.NVarChar:
                    case SqlDbType.VarBinary:
                    case SqlDbType.VarChar:
                        stringBuilder.Append((object)this.sqlDbType);
                        if (this.size.HasValue)
                        {
                            int? nullable = this.size;
                            int num1 = 0;
                            if ((nullable.GetValueOrDefault() == num1 ? (!nullable.HasValue ? 1 : 0) : 1) != 0 && (formatFlags & QueryFormatOptions.SuppressSize) == QueryFormatOptions.None)
                            {
                                stringBuilder.Append("(");
                                nullable = this.size;
                                int num2 = -1;
                                if ((nullable.GetValueOrDefault() == num2 ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                                    stringBuilder.Append("MAX");
                                else
                                    stringBuilder.Append((object)this.size);
                                stringBuilder.Append(")");
                                break;
                            }
                            break;
                        }
                        break;
                    case SqlDbType.Variant:
                        stringBuilder.Append("sql_variant");
                        break;
                }
                return stringBuilder.ToString();
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                    return true;
                SqlTypeSystem.SqlType sqlType = obj as SqlTypeSystem.SqlType;
                if ((ProviderType)sqlType == (ProviderType)null || !(this.runtimeOnlyType == sqlType.runtimeOnlyType))
                    return false;
                int? nullable1 = this.applicationTypeIndex;
                int? nullable2 = sqlType.applicationTypeIndex;
                if ((nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() ? (nullable1.HasValue == nullable2.HasValue ? 1 : 0) : 0) != 0 && this.sqlDbType == sqlType.sqlDbType)
                {
                    nullable2 = this.Size;
                    int? size = sqlType.Size;
                    if ((nullable2.GetValueOrDefault() == size.GetValueOrDefault() ? (nullable2.HasValue == size.HasValue ? 1 : 0) : 0) != 0 && this.precision == sqlType.precision)
                        return this.scale == sqlType.scale;
                }
                return false;
            }

            public override int GetHashCode()
            {
                int num = 0;
                if (this.runtimeOnlyType != (Type)null)
                    num = this.runtimeOnlyType.GetHashCode();
                else if (this.applicationTypeIndex.HasValue)
                    num = this.applicationTypeIndex.Value;
                return num ^ this.sqlDbType.GetHashCode() ^ (this.size ?? 0) ^ this.precision ^ this.scale << 8;
            }

            private static int GetTypeCoercionPrecedence(SqlDbType type)
            {
                switch (type)
                {
                    case SqlDbType.BigInt:
                        return 15;
                    case SqlDbType.Binary:
                        return 0;
                    case SqlDbType.Bit:
                        return 11;
                    case SqlDbType.Char:
                        return 3;
                    case SqlDbType.DateTime:
                        return 24;
                    case SqlDbType.Decimal:
                        return 18;
                    case SqlDbType.Float:
                        return 20;
                    case SqlDbType.Image:
                        return 8;
                    case SqlDbType.Int:
                        return 14;
                    case SqlDbType.Money:
                        return 17;
                    case SqlDbType.NChar:
                        return 4;
                    case SqlDbType.NText:
                        return 10;
                    case SqlDbType.NVarChar:
                        return 5;
                    case SqlDbType.Real:
                        return 19;
                    case SqlDbType.UniqueIdentifier:
                        return 6;
                    case SqlDbType.SmallDateTime:
                        return 23;
                    case SqlDbType.SmallInt:
                        return 13;
                    case SqlDbType.SmallMoney:
                        return 16;
                    case SqlDbType.Text:
                        return 9;
                    case SqlDbType.Timestamp:
                        return 7;
                    case SqlDbType.TinyInt:
                        return 12;
                    case SqlDbType.VarBinary:
                        return 1;
                    case SqlDbType.VarChar:
                        return 2;
                    case SqlDbType.Variant:
                        return 28;
                    case SqlDbType.Xml:
                        return 27;
                    case SqlDbType.Udt:
                        return 29;
                    case SqlDbType.Date:
                        return 21;
                    case SqlDbType.Time:
                        return 22;
                    case SqlDbType.DateTime2:
                        return 25;
                    case SqlDbType.DateTimeOffset:
                        return 26;
                    default:
                        throw Error.UnexpectedTypeCode((object)type);
                }
            }

            internal enum TypeCategory
            {
                Numeric,
                Char,
                Text,
                Binary,
                Image,
                Xml,
                DateTime,
                UniqueIdentifier,
                Variant,
                Udt,
            }
        }

        private abstract class ProviderBase : TypeSystemProvider
        {
            protected Dictionary<int, SqlTypeSystem.SqlType> applicationTypes = new Dictionary<int, SqlTypeSystem.SqlType>();

            protected abstract bool SupportsMaxSize { get; }

            internal override ProviderType GetApplicationType(int index)
            {
                if (index < 0)
                    throw Error.ArgumentOutOfRange("index");
                SqlTypeSystem.SqlType sqlType = (SqlTypeSystem.SqlType)null;
                if (!this.applicationTypes.TryGetValue(index, out sqlType))
                {
                    sqlType = new SqlTypeSystem.SqlType(index);
                    this.applicationTypes.Add(index, sqlType);
                }
                return (ProviderType)sqlType;
            }

            internal override ProviderType Parse(string stype)
            {
                string s1 = (string)null;
                string s2 = (string)null;
                int val2 = stype.IndexOf('(');
                int val1 = stype.IndexOf(' ');
                int length = val2 == -1 || val1 == -1 ? (val2 != -1 ? val2 : (val1 != -1 ? val1 : -1)) : Math.Min(val1, val2);
                string strA;
                if (length == -1)
                {
                    strA = stype;
                    length = stype.Length;
                }
                else
                    strA = stype.Substring(0, length);
                int index = length;
                if (index < stype.Length && (int)stype[index] == 40)
                {
                    int startIndex1 = index + 1;
                    int num1 = stype.IndexOf(',', startIndex1);
                    int num2;
                    if (num1 > 0)
                    {
                        s1 = stype.Substring(startIndex1, num1 - startIndex1);
                        int startIndex2 = num1 + 1;
                        num2 = stype.IndexOf(')', startIndex2);
                        s2 = stype.Substring(startIndex2, num2 - startIndex2);
                    }
                    else
                    {
                        num2 = stype.IndexOf(')', startIndex1);
                        s1 = stype.Substring(startIndex1, num2 - startIndex1);
                    }
                    int num3 = num2 + 1;
                }
                if (string.Compare(strA, "rowversion", StringComparison.OrdinalIgnoreCase) == 0)
                    strA = "Timestamp";
                if (string.Compare(strA, "numeric", StringComparison.OrdinalIgnoreCase) == 0)
                    strA = "Decimal";
                if (string.Compare(strA, "sql_variant", StringComparison.OrdinalIgnoreCase) == 0)
                    strA = "Variant";
                if (string.Compare(strA, "filestream", StringComparison.OrdinalIgnoreCase) == 0)
                    strA = "Binary";
                string[] names = Enum.GetNames(typeof(SqlDbType));
                Func<string, string> func = (Func<string, string>)(n => n.ToUpperInvariant());
                Func<string, string> selector = null;
                if (!System.Linq.Enumerable.Contains<string>(System.Linq.Enumerable.Select<string, string>((IEnumerable<string>)names, selector), strA.ToUpperInvariant()))
                    throw Error.InvalidProviderType((object)strA);
                int num = 0;
                int scale = 0;
                SqlDbType type = (SqlDbType)Enum.Parse(typeof(SqlDbType), strA, true);
                if (s1 != null)
                {
                    if (string.Compare(s1.Trim(), "max", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        num = -1;
                    }
                    else
                    {
                        num = int.Parse(s1, (IFormatProvider)CultureInfo.InvariantCulture);
                        if (num == int.MaxValue)
                            num = -1;
                    }
                }
                if (s2 != null)
                {
                    if (string.Compare(s2.Trim(), "max", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        scale = -1;
                    }
                    else
                    {
                        scale = int.Parse(s2, (IFormatProvider)CultureInfo.InvariantCulture);
                        if (scale == int.MaxValue)
                            scale = -1;
                    }
                }
                switch (type)
                {
                    case SqlDbType.Binary:
                    case SqlDbType.Char:
                    case SqlDbType.NChar:
                    case SqlDbType.NVarChar:
                    case SqlDbType.VarBinary:
                    case SqlDbType.VarChar:
                        return SqlTypeSystem.Create(type, num);
                    case SqlDbType.Decimal:
                    case SqlDbType.Float:
                    case SqlDbType.Real:
                        return SqlTypeSystem.Create(type, num, scale);
                    default:
                        return SqlTypeSystem.Create(type);
                }
            }

            protected ProviderType GetBestType(SqlDbType targetType, int? size)
            {
                int num = 0;
                if (targetType <= SqlDbType.NChar)
                {
                    if (targetType != SqlDbType.Binary && targetType != SqlDbType.Char)
                    {
                        if (targetType != SqlDbType.NChar)
                            goto label_7;
                    }
                    else
                        goto label_6;
                }
                else if (targetType != SqlDbType.NVarChar)
                {
                    if (targetType == SqlDbType.VarBinary || targetType == SqlDbType.VarChar)
                        goto label_6;
                    else
                        goto label_7;
                }
                num = 4000;
                goto label_7;
                label_6:
                num = 8000;
                label_7:
                if (!size.HasValue)
                    return SqlTypeSystem.Create(targetType, this.SupportsMaxSize ? -1 : num);
                if (size.Value <= num)
                    return SqlTypeSystem.Create(targetType, size.Value);
                return this.GetBestLargeType(SqlTypeSystem.Create(targetType));
            }

            internal override void InitializeParameter(ProviderType type, DbParameter parameter, object value)
            {
                SqlTypeSystem.SqlType sqlType = (SqlTypeSystem.SqlType)type;
                if (sqlType.IsRuntimeOnlyType)
                    throw Error.BadParameterType((object)sqlType.GetClosestRuntimeType());
                System.Data.SqlClient.SqlParameter sqlParameter = parameter as System.Data.SqlClient.SqlParameter;
                if (sqlParameter != null)
                {
                    sqlParameter.SqlDbType = sqlType.SqlDbType;
                    if (sqlType.HasPrecisionAndScale)
                    {
                        sqlParameter.Precision = (byte)sqlType.Precision;
                        sqlParameter.Scale = (byte)sqlType.Scale;
                    }
                }
                else
                {
                    PropertyInfo property1 = parameter.GetType().GetProperty("SqlDbType");
                    if (property1 != (PropertyInfo)null)
                        property1.SetValue((object)parameter, (object)sqlType.SqlDbType, (object[])null);
                    if (sqlType.HasPrecisionAndScale)
                    {
                        PropertyInfo property2 = parameter.GetType().GetProperty("Precision");
                        if (property2 != (PropertyInfo)null)
                            property2.SetValue((object)parameter, Convert.ChangeType((object)sqlType.Precision, property2.PropertyType, (IFormatProvider)CultureInfo.InvariantCulture), (object[])null);
                        PropertyInfo property3 = parameter.GetType().GetProperty("Scale");
                        if (property3 != (PropertyInfo)null)
                            property3.SetValue((object)parameter, Convert.ChangeType((object)sqlType.Scale, property3.PropertyType, (IFormatProvider)CultureInfo.InvariantCulture), (object[])null);
                    }
                }
                parameter.Value = this.GetParameterValue(sqlType, value);
                int? nullable = this.DetermineParameterSize(sqlType, parameter);
                if (!nullable.HasValue)
                    return;
                parameter.Size = nullable.Value;
            }

            internal virtual int? DetermineParameterSize(SqlTypeSystem.SqlType declaredType, DbParameter parameter)
            {
                int? nullable;
                if (parameter.Direction != ParameterDirection.Input || declaredType.IsFixedSize)
                {
                    if (declaredType.Size.HasValue)
                    {
                        int size = parameter.Size;
                        nullable = declaredType.Size;
                        int valueOrDefault = nullable.GetValueOrDefault();
                        if ((size <= valueOrDefault ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                            goto label_4;
                    }
                    if (!declaredType.IsLargeType)
                        goto label_5;
                    label_4:
                    nullable = declaredType.Size;
                    return new int?(nullable.Value);
                }
                label_5:
                nullable = new int?();
                return nullable;
            }

            protected int? GetLargestDeclarableSize(SqlTypeSystem.SqlType declaredType)
            {
                switch (declaredType.SqlDbType)
                {
                    case SqlDbType.NVarChar:
                        return new int?(4000);
                    case SqlDbType.VarChar:
                    case SqlDbType.Binary:
                    case SqlDbType.Image:
                        return new int?(8000);
                    default:
                        return new int?();
                }
            }

            protected object GetParameterValue(SqlTypeSystem.SqlType type, object value)
            {
                if (value == null)
                    return (object)DBNull.Value;
                Type type1 = value.GetType();
                Type closestRuntimeType = type.GetClosestRuntimeType();
                if (closestRuntimeType == type1)
                    return value;
                return DBConvert.ChangeType(value, closestRuntimeType);
            }

            internal override ProviderType PredictTypeForUnary(SqlNodeType unaryOp, ProviderType operandType)
            {
                switch (unaryOp)
                {
                    case SqlNodeType.Treat:
                    case SqlNodeType.ValueOf:
                    case SqlNodeType.Negate:
                    case SqlNodeType.OuterJoinedValue:
                    case SqlNodeType.BitNot:
                        return operandType;
                    case SqlNodeType.Stddev:
                    case SqlNodeType.Sum:
                    case SqlNodeType.Avg:
                        return this.MostPreciseTypeInFamily(operandType);
                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                    case SqlNodeType.IsNull:
                    case SqlNodeType.IsNotNull:
                        return (ProviderType)SqlTypeSystem.theBit;
                    case SqlNodeType.Max:
                    case SqlNodeType.Min:
                        return operandType;
                    case SqlNodeType.LongCount:
                        return this.From(typeof(long));
                    case SqlNodeType.Count:
                        return this.From(typeof(int));
                    case SqlNodeType.ClrLength:
                        if (operandType.IsLargeType)
                            return this.From(typeof(long));
                        return this.From(typeof(int));
                    default:
                        throw Error.UnexpectedNode((object)unaryOp);
                }
            }

            internal override ProviderType PredictTypeForBinary(SqlNodeType binaryOp, ProviderType leftType, ProviderType rightType)
            {
                SqlTypeSystem.SqlType sqlType = !leftType.IsSameTypeFamily(this.From(typeof(string))) || !rightType.IsSameTypeFamily(this.From(typeof(string))) ? (leftType.ComparePrecedenceTo(rightType) > 0 ? (SqlTypeSystem.SqlType)leftType : (SqlTypeSystem.SqlType)rightType) : (SqlTypeSystem.SqlType)this.GetBestType(leftType, rightType);
                switch (binaryOp)
                {
                    case SqlNodeType.Or:
                    case SqlNodeType.LT:
                    case SqlNodeType.GE:
                    case SqlNodeType.GT:
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.LE:
                    case SqlNodeType.And:
                        return (ProviderType)SqlTypeSystem.theInt;
                    case SqlNodeType.Sub:
                    case SqlNodeType.Mod:
                    case SqlNodeType.Mul:
                    case SqlNodeType.Coalesce:
                    case SqlNodeType.Div:
                    case SqlNodeType.Add:
                    case SqlNodeType.BitAnd:
                    case SqlNodeType.BitOr:
                    case SqlNodeType.BitXor:
                        return (ProviderType)sqlType;
                    case SqlNodeType.Concat:
                        if (!sqlType.HasSizeOrIsLarge)
                            return (ProviderType)sqlType;
                        ProviderType bestType = this.GetBestType(sqlType.SqlDbType, new int?());
                        if (!leftType.IsLargeType && (leftType.Size.HasValue && !rightType.IsLargeType) && rightType.Size.HasValue)
                        {
                            int num1 = leftType.Size.Value + rightType.Size.Value;
                            int num2 = num1;
                            int? size = bestType.Size;
                            int valueOrDefault = size.GetValueOrDefault();
                            if ((num2 < valueOrDefault ? (size.HasValue ? 1 : 0) : 0) != 0 || bestType.IsLargeType)
                                return this.GetBestType(sqlType.SqlDbType, new int?(num1));
                        }
                        return bestType;
                    default:
                        throw Error.UnexpectedNode((object)binaryOp);
                }
            }

            internal override ProviderType MostPreciseTypeInFamily(ProviderType type)
            {
                switch (((SqlTypeSystem.SqlType)type).SqlDbType)
                {
                    case SqlDbType.DateTime:
                    case SqlDbType.SmallDateTime:
                    case SqlDbType.Date:
                    case SqlDbType.Time:
                    case SqlDbType.DateTime2:
                        return this.From(typeof(DateTime));
                    case SqlDbType.Float:
                    case SqlDbType.Real:
                        return this.From(typeof(double));
                    case SqlDbType.Int:
                    case SqlDbType.SmallInt:
                    case SqlDbType.TinyInt:
                        return this.From(typeof(int));
                    case SqlDbType.Money:
                    case SqlDbType.SmallMoney:
                        return SqlTypeSystem.Create(SqlDbType.Money);
                    case SqlDbType.DateTimeOffset:
                        return this.From(typeof(DateTimeOffset));
                    default:
                        return type;
                }
            }

            private ProviderType[] GetArgumentTypes(SqlFunctionCall fc)
            {
                ProviderType[] providerTypeArray = new ProviderType[fc.Arguments.Count];
                int index = 0;
                for (int length = providerTypeArray.Length; index < length; ++index)
                    providerTypeArray[index] = fc.Arguments[index].SqlType;
                return providerTypeArray;
            }

            internal override ProviderType ReturnTypeOfFunction(SqlFunctionCall functionCall)
            {
                ProviderType[] argumentTypes = this.GetArgumentTypes(functionCall);
                SqlTypeSystem.SqlType sqlType1 = (SqlTypeSystem.SqlType)argumentTypes[0];
                SqlTypeSystem.SqlType sqlType2 = argumentTypes.Length > 1 ? (SqlTypeSystem.SqlType)argumentTypes[1] : (SqlTypeSystem.SqlType)null;
                string name = functionCall.Name;
                // ISSUE: reference to a compiler-generated method
                uint stringHash =0;
                if (stringHash <= 2188201123U)
                {
                    if (stringHash <= 1327090656U)
                    {
                        if (stringHash <= 272375920U)
                        {
                            if ((int)stringHash != 179158948)
                            {
                                if ((int)stringHash != 272375920 || !(name == "LEFT"))
                                    goto label_67;
                                else
                                    goto label_66;
                            }
                            else if (!(name == "SIGN"))
                                goto label_67;
                        }
                        else if ((int)stringHash != 462935941)
                        {
                            if ((int)stringHash != 885080057)
                            {
                                if ((int)stringHash != 1327090656 || !(name == "CEILING"))
                                    goto label_67;
                            }
                            else if (name == "RTRIM")
                                goto label_66;
                            else
                                goto label_67;
                        }
                        else if (name == "RIGHT")
                            goto label_66;
                        else
                            goto label_67;
                    }
                    else if (stringHash <= 1404026312U)
                    {
                        if ((int)stringHash != 1371097821)
                        {
                            if ((int)stringHash == 1404026312 && name == "SUBSTRING")
                            {
                                if (functionCall.Arguments[2].NodeType == SqlNodeType.Value)
                                {
                                    SqlValue sqlValue = (SqlValue)functionCall.Arguments[2];
                                    if (sqlValue.Value is int)
                                    {
                                        switch (sqlType1.SqlDbType)
                                        {
                                            case SqlDbType.NVarChar:
                                            case SqlDbType.VarChar:
                                            case SqlDbType.Char:
                                            case SqlDbType.NChar:
                                                return SqlTypeSystem.Create(sqlType1.SqlDbType, (int)sqlValue.Value);
                                            default:
                                                return (ProviderType)null;
                                        }
                                    }
                                }
                                switch (sqlType1.SqlDbType)
                                {
                                    case SqlDbType.NVarChar:
                                    case SqlDbType.NChar:
                                        return SqlTypeSystem.Create(SqlDbType.NVarChar);
                                    case SqlDbType.VarChar:
                                    case SqlDbType.Char:
                                        return SqlTypeSystem.Create(SqlDbType.VarChar);
                                    default:
                                        return (ProviderType)null;
                                }
                            }
                            else
                                goto label_67;
                        }
                        else
                        {
                            if (name == "STUFF")
                            {
                                if (functionCall.Arguments.Count == 4)
                                {
                                    SqlValue sqlValue = functionCall.Arguments[2] as SqlValue;
                                    if (sqlValue != null && (int)sqlValue.Value == 0)
                                        return this.PredictTypeForBinary(SqlNodeType.Concat, functionCall.Arguments[0].SqlType, functionCall.Arguments[3].SqlType);
                                }
                                return (ProviderType)null;
                            }
                            goto label_67;
                        }
                    }
                    else if ((int)stringHash != 1668566771)
                    {
                        if ((int)stringHash != 1820608667)
                        {
                            if ((int)stringHash != -2106766173 || !(name == "REPLACE"))
                                goto label_67;
                            else
                                goto label_66;
                        }
                        else if (!(name == "ABS"))
                            goto label_67;
                    }
                    else if (name == "LTRIM")
                        goto label_66;
                    else
                        goto label_67;
                }
                else if (stringHash <= 2906723303U)
                {
                    if (stringHash <= 2515996122U)
                    {
                        if ((int)stringHash != -1932889562)
                        {
                            if ((int)stringHash != -1778971174 || !(name == "LOWER"))
                                goto label_67;
                            else
                                goto label_66;
                        }
                        else if (!(name == "POWER"))
                            goto label_67;
                    }
                    else if ((int)stringHash != -1673822659)
                    {
                        if ((int)stringHash != -1527957656)
                        {
                            if ((int)stringHash != -1388243993 || !(name == "UPPER"))
                                goto label_67;
                            else
                                goto label_66;
                        }
                        else if (name == "INSERT")
                            goto label_66;
                        else
                            goto label_67;
                    }
                    else if (!(name == "FLOOR"))
                        goto label_67;
                }
                else
                {
                    if (stringHash <= 3108329196U)
                    {
                        if ((int)stringHash != -1280205275)
                        {
                            if ((int)stringHash != -1227403749)
                            {
                                if ((int)stringHash != -1186638100 || !(name == "LEN"))
                                    goto label_67;
                            }
                            else if (name == "ROUND")
                                goto label_46;
                            else
                                goto label_67;
                        }
                        else if (name == "REVERSE")
                            goto label_66;
                        else
                            goto label_67;
                    }
                    else
                    {
                        if ((int)stringHash != -748177853)
                        {
                            if ((int)stringHash != -684766898)
                            {
                                if ((int)stringHash != -48818123 || !(name == "DATALENGTH"))
                                    goto label_67;
                                else
                                    goto label_41;
                            }
                            else if (!(name == "PATINDEX"))
                                goto label_67;
                        }
                        else if (!(name == "CHARINDEX"))
                            goto label_67;
                        if (sqlType2.IsLargeType)
                            return SqlTypeSystem.Create(SqlDbType.BigInt);
                        return SqlTypeSystem.Create(SqlDbType.Int);
                    }
                    label_41:
                    switch (sqlType1.SqlDbType)
                    {
                        case SqlDbType.NVarChar:
                        case SqlDbType.VarBinary:
                        case SqlDbType.VarChar:
                            if (sqlType1.IsLargeType)
                                return SqlTypeSystem.Create(SqlDbType.BigInt);
                            return SqlTypeSystem.Create(SqlDbType.Int);
                        default:
                            return SqlTypeSystem.Create(SqlDbType.Int);
                    }
                }
                label_46:
                switch (sqlType1.SqlDbType)
                {
                    case SqlDbType.Real:
                    case SqlDbType.Float:
                        return SqlTypeSystem.Create(SqlDbType.Float);
                    case SqlDbType.SmallInt:
                    case SqlDbType.TinyInt:
                    case SqlDbType.Int:
                        return SqlTypeSystem.Create(SqlDbType.Int);
                    default:
                        return (ProviderType)sqlType1;
                }
                label_66:
                return (ProviderType)sqlType1;
                label_67:
                return (ProviderType)null;
            }

            internal override ProviderType ChangeTypeFamilyTo(ProviderType type, ProviderType toType)
            {
                if (type.IsSameTypeFamily(toType))
                    return type;
                if (type.IsApplicationType || toType.IsApplicationType)
                    return toType;
                SqlTypeSystem.SqlType sqlType1 = (SqlTypeSystem.SqlType)toType;
                SqlTypeSystem.SqlType sqlType2 = (SqlTypeSystem.SqlType)type;
                if (sqlType1.Category != SqlTypeSystem.SqlType.TypeCategory.Numeric || sqlType2.Category != SqlTypeSystem.SqlType.TypeCategory.Char)
                    return toType;
                switch (sqlType2.SqlDbType)
                {
                    case SqlDbType.Char:
                        return SqlTypeSystem.Create(SqlDbType.SmallInt);
                    case SqlDbType.NChar:
                        return SqlTypeSystem.Create(SqlDbType.Int);
                    default:
                        return toType;
                }
            }

            internal override ProviderType GetBestType(ProviderType typeA, ProviderType typeB)
            {
                SqlTypeSystem.SqlType sqlType1 = typeA.ComparePrecedenceTo(typeB) > 0 ? (SqlTypeSystem.SqlType)typeA : (SqlTypeSystem.SqlType)typeB;
                if (typeA.IsApplicationType || typeA.IsRuntimeOnlyType)
                    return typeA;
                if (typeB.IsApplicationType || typeB.IsRuntimeOnlyType)
                    return typeB;
                SqlTypeSystem.SqlType sqlType2 = (SqlTypeSystem.SqlType)typeA;
                SqlTypeSystem.SqlType sqlType3 = (SqlTypeSystem.SqlType)typeB;
                if (sqlType2.HasPrecisionAndScale && sqlType3.HasPrecisionAndScale && sqlType1.SqlDbType == SqlDbType.Decimal)
                {
                    int precision1 = sqlType2.Precision;
                    int scale1 = sqlType2.Scale;
                    int precision2 = sqlType3.Precision;
                    int scale2 = sqlType3.Scale;
                    if (precision1 == 0 && scale1 == 0 && (precision2 == 0 && scale2 == 0))
                        return SqlTypeSystem.Create(sqlType1.SqlDbType);
                    if (precision1 == 0 && scale1 == 0)
                        return SqlTypeSystem.Create(sqlType1.SqlDbType, precision2, scale2);
                    if (precision2 == 0 && scale2 == 0)
                        return SqlTypeSystem.Create(sqlType1.SqlDbType, precision1, scale1);
                    int num1 = Math.Max(precision1 - scale1, precision2 - scale2);
                    int scale3 = Math.Max(scale1, scale2);
                    int num2 = scale3;
                    int precision3 = Math.Min(num1 + num2, 38);
                    return SqlTypeSystem.Create(sqlType1.SqlDbType, precision3, scale3);
                }
                int? size1 = new int?();
                int? size2 = sqlType2.Size;
                if (size2.HasValue)
                {
                    size2 = sqlType3.Size;
                    if (size2.HasValue)
                    {
                        size2 = sqlType3.Size;
                        int? size3 = sqlType2.Size;
                        size1 = (size2.GetValueOrDefault() > size3.GetValueOrDefault() ? (size2.HasValue & size3.HasValue ? 1 : 0) : 0) != 0 ? sqlType3.Size : sqlType2.Size;
                    }
                }
                if (sqlType3.Size.HasValue && sqlType3.Size.Value == -1 || sqlType2.Size.HasValue && sqlType2.Size.Value == -1)
                    size1 = new int?(-1);
                return (ProviderType)new SqlTypeSystem.SqlType(sqlType1.SqlDbType, size1);
            }

            internal override ProviderType From(object o)
            {
                Type type = o != null ? o.GetType() : typeof(object);
                if (type == typeof(string))
                {
                    string str = (string)o;
                    return this.From(type, new int?(str.Length));
                }
                if (type == typeof(bool))
                    return this.From(typeof(int));
                if (type.IsArray)
                {
                    Array array = (Array)o;
                    return this.From(type, new int?(array.Length));
                }
                if (!(type == typeof(Decimal)))
                    return this.From(type);
                int num = (Decimal.GetBits((Decimal)o)[3] & 16711680) >> 16;
                return this.From(type, new int?(num));
            }

            internal override ProviderType From(Type type)
            {
                return this.From(type, new int?());
            }

            internal override ProviderType From(Type type, int? size)
            {
                return this.From(type, size);
            }
        }

        private class Sql2005Provider : SqlTypeSystem.ProviderBase
        {
            protected override bool SupportsMaxSize
            {
                get
                {
                    return true;
                }
            }

            internal override ProviderType From(Type type, int? size)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = type.GetGenericArguments()[0];
                TypeCode typeCode = Type.GetTypeCode(type);
                switch (typeCode)
                {
                    case TypeCode.Object:
                        if (type == typeof(Guid))
                            return SqlTypeSystem.Create(SqlDbType.UniqueIdentifier);
                        if (type == typeof(byte[]) || type == typeof(Binary))
                            return this.GetBestType(SqlDbType.VarBinary, size);
                        if (type == typeof(char[]))
                            return this.GetBestType(SqlDbType.NVarChar, size);
                        if (type == typeof(TimeSpan))
                            return SqlTypeSystem.Create(SqlDbType.BigInt);
                        if (type == typeof(XDocument) || type == typeof(XElement))
                            return (ProviderType)SqlTypeSystem.theXml;
                        return (ProviderType)new SqlTypeSystem.SqlType(type);
                    case TypeCode.Boolean:
                        return SqlTypeSystem.Create(SqlDbType.Bit);
                    case TypeCode.Char:
                        return SqlTypeSystem.Create(SqlDbType.NChar, 1);
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                        return SqlTypeSystem.Create(SqlDbType.SmallInt);
                    case TypeCode.Byte:
                        return SqlTypeSystem.Create(SqlDbType.TinyInt);
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                        return SqlTypeSystem.Create(SqlDbType.Int);
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                        return SqlTypeSystem.Create(SqlDbType.BigInt);
                    case TypeCode.UInt64:
                        return SqlTypeSystem.Create(SqlDbType.Decimal, 20, 0);
                    case TypeCode.Single:
                        return SqlTypeSystem.Create(SqlDbType.Real);
                    case TypeCode.Double:
                        return SqlTypeSystem.Create(SqlDbType.Float);
                    case TypeCode.Decimal:
                        return SqlTypeSystem.Create(SqlDbType.Decimal, 29, size ?? 4);
                    case TypeCode.DateTime:
                        return SqlTypeSystem.Create(SqlDbType.DateTime);
                    case TypeCode.String:
                        return this.GetBestType(SqlDbType.NVarChar, size);
                    default:
                        throw Error.UnexpectedTypeCode((object)typeCode);
                }
            }

            internal override ProviderType GetBestLargeType(ProviderType type)
            {
                switch (((SqlTypeSystem.SqlType)type).SqlDbType)
                {
                    case SqlDbType.Image:
                    case SqlDbType.VarBinary:
                    case SqlDbType.Binary:
                        return SqlTypeSystem.Create(SqlDbType.VarBinary, -1);
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                        return SqlTypeSystem.Create(SqlDbType.NVarChar, -1);
                    case SqlDbType.Text:
                    case SqlDbType.VarChar:
                    case SqlDbType.Char:
                        return SqlTypeSystem.Create(SqlDbType.VarChar, -1);
                    default:
                        return type;
                }
            }

            internal override int? DetermineParameterSize(SqlTypeSystem.SqlType declaredType, DbParameter parameter)
            {
                int? nullable = base.DetermineParameterSize(declaredType, parameter);
                if (nullable.HasValue)
                    return nullable;
                int? largestDeclarableSize = this.GetLargestDeclarableSize(declaredType);
                if (largestDeclarableSize.HasValue && largestDeclarableSize.Value >= parameter.Size)
                    return new int?(largestDeclarableSize.Value);
                return new int?(-1);
            }
        }

        private class Sql2008Provider : SqlTypeSystem.Sql2005Provider
        {
            internal override ProviderType From(Type type, int? size)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = type.GetGenericArguments()[0];
                if (Type.GetTypeCode(type) == TypeCode.Object && type == typeof(DateTimeOffset))
                    return SqlTypeSystem.Create(SqlDbType.DateTimeOffset);
                return base.From(type, size);
            }
        }

        private class Sql2000Provider : SqlTypeSystem.ProviderBase
        {
            protected override bool SupportsMaxSize
            {
                get
                {
                    return false;
                }
            }

            internal override ProviderType From(Type type, int? size)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = type.GetGenericArguments()[0];
                TypeCode typeCode = Type.GetTypeCode(type);
                switch (typeCode)
                {
                    case TypeCode.Object:
                        if (type == typeof(Guid))
                            return SqlTypeSystem.Create(SqlDbType.UniqueIdentifier);
                        if (type == typeof(byte[]) || type == typeof(Binary))
                            return this.GetBestType(SqlDbType.VarBinary, size);
                        if (type == typeof(char[]))
                            return this.GetBestType(SqlDbType.NVarChar, size);
                        if (type == typeof(TimeSpan))
                            return SqlTypeSystem.Create(SqlDbType.BigInt);
                        if (type == typeof(XDocument) || type == typeof(XElement))
                            return (ProviderType)SqlTypeSystem.theNText;
                        return (ProviderType)new SqlTypeSystem.SqlType(type);
                    case TypeCode.Boolean:
                        return SqlTypeSystem.Create(SqlDbType.Bit);
                    case TypeCode.Char:
                        return SqlTypeSystem.Create(SqlDbType.NChar, 1);
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                        return SqlTypeSystem.Create(SqlDbType.SmallInt);
                    case TypeCode.Byte:
                        return SqlTypeSystem.Create(SqlDbType.TinyInt);
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                        return SqlTypeSystem.Create(SqlDbType.Int);
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                        return SqlTypeSystem.Create(SqlDbType.BigInt);
                    case TypeCode.UInt64:
                        return SqlTypeSystem.Create(SqlDbType.Decimal, 20, 0);
                    case TypeCode.Single:
                        return SqlTypeSystem.Create(SqlDbType.Real);
                    case TypeCode.Double:
                        return SqlTypeSystem.Create(SqlDbType.Float);
                    case TypeCode.Decimal:
                        return SqlTypeSystem.Create(SqlDbType.Decimal, 29, size ?? 4);
                    case TypeCode.DateTime:
                        return SqlTypeSystem.Create(SqlDbType.DateTime);
                    case TypeCode.String:
                        return this.GetBestType(SqlDbType.NVarChar, size);
                    default:
                        throw Error.UnexpectedTypeCode((object)typeCode);
                }
            }

            internal override ProviderType GetBestLargeType(ProviderType type)
            {
                switch (((SqlTypeSystem.SqlType)type).SqlDbType)
                {
                    case SqlDbType.NVarChar:
                    case SqlDbType.NChar:
                        return SqlTypeSystem.Create(SqlDbType.NText);
                    case SqlDbType.VarBinary:
                    case SqlDbType.Binary:
                        return SqlTypeSystem.Create(SqlDbType.Image);
                    case SqlDbType.VarChar:
                    case SqlDbType.Char:
                        return SqlTypeSystem.Create(SqlDbType.Text);
                    default:
                        return type;
                }
            }
        }

        private class SqlCEProvider : SqlTypeSystem.Sql2000Provider
        {
        }
    }
}
