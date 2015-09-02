using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExpressionTest
{
    public static class DBConvert
    {
        private static Type[] StringArg;

        static DBConvert()
        {
            Type[] typeArray = new Type[1];
            int index = 0;
            Type type = typeof(string);
            typeArray[index] = type;
            DBConvert.StringArg = typeArray;
        }

        /// <summary>
        /// 将指定的值更改为当前类型。
        /// </summary>
        /// 
        /// <returns>
        /// 一个包含转换值的指定类型的对象。
        /// </returns>
        /// <param name="value">要转换的对象。</param><typeparam name="T">要更改为的类型。</typeparam>
        public static T ChangeType<T>(object value)
        {
            return (T)DBConvert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// 将指定的值更改为指定类型。
        /// </summary>
        /// 
        /// <returns>
        /// 一个包含指定类型的转换值的对象。
        /// </returns>
        /// <param name="value">要转换的对象。</param><param name="type">对象要转换为的类型。</param>
        public static object ChangeType(object value, Type type)
        {
            if (value == null)
                return (object)null;
            Type nonNullableType = TypeSystem.GetNonNullableType(type);
            Type type1 = value.GetType();
            if (nonNullableType.IsAssignableFrom(type1))
                return value;
            if (nonNullableType == typeof(Binary))
            {
                if (type1 == typeof(byte[]))
                    return (object)new Binary((byte[])value);
                if (type1 == typeof(Guid))
                    return (object)new Binary(((Guid)value).ToByteArray());
                byte[] numArray;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    new BinaryFormatter().Serialize((Stream)memoryStream, value);
                    numArray = memoryStream.ToArray();
                }
                return (object)new Binary(numArray);
            }
            if (nonNullableType == typeof(byte[]))
            {
                if (type1 == typeof(Binary))
                    return (object)((Binary)value).ToArray();
                if (type1 == typeof(Guid))
                    return (object)((Guid)value).ToByteArray();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    new BinaryFormatter().Serialize((Stream)memoryStream, value);
                    return (object)memoryStream.ToArray();
                }
            }
            else if (type1 == typeof(byte[]))
            {
                if (nonNullableType == typeof(Guid))
                    return (object)new Guid((byte[])value);
                using (MemoryStream memoryStream = new MemoryStream((byte[])value))
                    return DBConvert.ChangeType(new BinaryFormatter().Deserialize((Stream)memoryStream), nonNullableType);
            }
            else if (type1 == typeof(Binary))
            {
                if (nonNullableType == typeof(Guid))
                    return (object)new Guid(((Binary)value).ToArray());
                using (MemoryStream memoryStream = new MemoryStream(((Binary)value).ToArray(), false))
                    return DBConvert.ChangeType(new BinaryFormatter().Deserialize((Stream)memoryStream), nonNullableType);
            }
            else
            {
                if (nonNullableType.IsEnum)
                {
                    if (!(type1 == typeof(string)))
                        return Enum.ToObject(nonNullableType, Convert.ChangeType(value, Enum.GetUnderlyingType(nonNullableType), (IFormatProvider)CultureInfo.InvariantCulture));
                    string str = ((string)value).Trim();
                    return Enum.Parse(nonNullableType, str);
                }
                if (type1.IsEnum)
                {
                    if (nonNullableType == typeof(string))
                        return (object)Enum.GetName(type1, value);
                    return Convert.ChangeType(Convert.ChangeType(value, Enum.GetUnderlyingType(type1), (IFormatProvider)CultureInfo.InvariantCulture), nonNullableType, (IFormatProvider)CultureInfo.InvariantCulture);
                }
                if (nonNullableType == typeof(TimeSpan))
                {
                    if (type1 == typeof(string))
                        return (object)TimeSpan.Parse(value.ToString(), (IFormatProvider)CultureInfo.InvariantCulture);
                    if (type1 == typeof(DateTime))
                        return (object)DateTime.Parse(value.ToString(), (IFormatProvider)CultureInfo.InvariantCulture).TimeOfDay;
                    if (type1 == typeof(DateTimeOffset))
                        return (object)DateTimeOffset.Parse(value.ToString(), (IFormatProvider)CultureInfo.InvariantCulture).TimeOfDay;
                    return (object)new TimeSpan((long)Convert.ChangeType(value, typeof(long), (IFormatProvider)CultureInfo.InvariantCulture));
                }
                if (type1 == typeof(TimeSpan))
                {
                    if (nonNullableType == typeof(string))
                        return (object)((TimeSpan)value).ToString("", (IFormatProvider)CultureInfo.InvariantCulture);
                    if (nonNullableType == typeof(DateTime))
                        return (object)new DateTime().Add((TimeSpan)value);
                    if (nonNullableType == typeof(DateTimeOffset))
                        return (object)new DateTimeOffset().Add((TimeSpan)value);
                    return Convert.ChangeType((object)((TimeSpan)value).Ticks, nonNullableType, (IFormatProvider)CultureInfo.InvariantCulture);
                }
                if (nonNullableType == typeof(DateTime) && type1 == typeof(DateTimeOffset))
                    return (object)((DateTimeOffset)value).DateTime;
                if (nonNullableType == typeof(DateTimeOffset) && type1 == typeof(DateTime))
                    return (object)new DateTimeOffset((DateTime)value);
                if (nonNullableType == typeof(string) && !typeof(IConvertible).IsAssignableFrom(type1))
                {
                    if (type1 == typeof(char[]))
                        return (object)new string((char[])value);
                    return (object)value.ToString();
                }
                if (type1 == typeof(string))
                {
                    if (nonNullableType == typeof(Guid))
                        return (object)new Guid((string)value);
                    if (nonNullableType == typeof(char[]))
                        return (object)((string)value).ToCharArray();
                    if (nonNullableType == typeof(XDocument) && (string)value == string.Empty)
                        return (object)new XDocument();
                    if (!typeof(IConvertible).IsAssignableFrom(nonNullableType))
                    {
                        MethodInfo method1;
                        if ((method1 = nonNullableType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, (Binder)null, DBConvert.StringArg, (ParameterModifier[])null)) != (MethodInfo)null)
                        {
                            try
                            {
                                MethodInfo method2 = method1;
                                // ISSUE: variable of the null type
                                //__Null local = null;
                                object[] args = new object[1];
                                int index = 0;
                                object obj = value;
                                args[index] = obj;
                                return SecurityUtils.MethodInfoInvoke(method2, (object)null, args);
                            }
                            catch (TargetInvocationException ex)
                            {
                                throw ex.GetBaseException();
                            }
                        }
                    }
                    return Convert.ChangeType(value, nonNullableType, (IFormatProvider)CultureInfo.InvariantCulture);
                }
                if (nonNullableType.IsGenericType && nonNullableType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                {
                    Type type2 = typeof(IEnumerable<>);
                    Type[] typeArray = new Type[1];
                    int index = 0;
                    Type type3 = nonNullableType.GetGenericArguments()[0];
                    typeArray[index] = type3;
                    if (type2.MakeGenericType(typeArray).IsAssignableFrom(type1))
                        return (object)Queryable.AsQueryable((IEnumerable)value);
                }
                try
                {
                    return Convert.ChangeType(value, nonNullableType, (IFormatProvider)CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException ex)
                {
                    throw Error.CouldNotConvert((object)type1, (object)nonNullableType);
                }
            }
        }
    }

    /// <summary>
    /// 表示不可变的二进制数据块。
    /// </summary>
    [DataContract]
    [Serializable]
    public sealed class Binary : IEquatable<Binary>
    {
        [DataMember(Name = "Bytes")]
        private byte[] bytes;
        private int? hashCode;

        /// <summary>
        /// 获取二进制对象的长度。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示长度的整数。
        /// </returns>
        public int Length
        {
            get
            {
                return this.bytes.Length;
            }
        }

        /// <summary>
        /// 初始化 <see cref="T:System.Data.Linq.Binary"/> 类的新实例。
        /// </summary>
        /// <param name="value">表示二进制数据的字节。</param>
        public Binary(byte[] value)
        {
            if (value == null)
            {
                this.bytes = new byte[0];
            }
            else
            {
                this.bytes = new byte[value.Length];
                Array.Copy((Array)value, (Array)this.bytes, value.Length);
            }
            this.ComputeHash();
        }

        /// <summary>
        /// 允许用某种编程语言将字节数组隐式地强制转换为 <see cref="T:System.Data.Linq.Binary"/> 类型。
        /// </summary>
        /// 
        /// <returns>
        /// 一个包含被强制的值的 <see cref="T:System.Data.Linq.Binary"/> 类。
        /// </returns>
        /// <param name="value">要转换为 <see cref="T:System.Data.Linq.Binary"/> 类型的实例的字节数组。</param>
        public static implicit operator Binary(byte[] value)
        {
            return new Binary(value);
        }

        /// <summary>
        /// 描述两个二进制对象之间的相等关系。
        /// </summary>
        /// 
        /// <returns>
        /// 如果两个二进制对象相等，则为 true；否则为 false。
        /// </returns>
        /// <param name="binary1">第一个二进制对象。</param><param name="binary2">第二个二进制对象。</param>
        public static bool operator ==(Binary binary1, Binary binary2)
        {
            if (binary1 == binary2 || binary1 == null && binary2 == null)
                return true;
            if (binary1 == null || binary2 == null)
                return false;
            return binary1.EqualsTo(binary2);
        }

        /// <summary>
        /// 描述两个二进制对象之间的不等关系。
        /// </summary>
        /// 
        /// <returns>
        /// 如果两个二进制对象不相等，则为 true；否则为 false。
        /// </returns>
        /// <param name="binary1">第一个二进制对象。</param><param name="binary2">第二个二进制对象。</param>
        public static bool operator !=(Binary binary1, Binary binary2)
        {
            if (binary1 == binary2 || binary1 == null && binary2 == null)
                return false;
            if (binary1 == null || binary2 == null)
                return true;
            return !binary1.EqualsTo(binary2);
        }

        /// <summary>
        /// 返回表示当前二进制对象的字节数组。
        /// </summary>
        /// 
        /// <returns>
        /// 包含当前二进制对象的值的字节数组。
        /// </returns>
        public byte[] ToArray()
        {
            byte[] numArray = new byte[this.bytes.Length];
            Array.Copy((Array)this.bytes, (Array)numArray, numArray.Length);
            return numArray;
        }

        /// <summary>
        /// 确定两个二进制对象是否相等。
        /// </summary>
        /// 
        /// <returns>
        /// 如果两个二进制对象相等，则为 true；否则为 false。
        /// </returns>
        /// <param name="other">正在和当前对象进行比较的 <see cref="T:System.Object"/>。</param>
        public bool Equals(Binary other)
        {
            return this.EqualsTo(other);
        }

        /// <summary>
        /// 确定指定的 <see cref="T:System.Object"/> 是否等于当前的 <see cref="T:System.Object"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 如果两个二进制对象相等，则为 true；否则为 false。
        /// </returns>
        /// <param name="obj">与当前的 <see cref="T:System.Object"/> 进行比较的 <see cref="T:System.Object"/>。</param>
        public override bool Equals(object obj)
        {
            return this.EqualsTo(obj as Binary);
        }

        /// <summary>
        /// 用作特定类型的哈希函数。
        /// </summary>
        /// 
        /// <returns>
        /// 当前二进制对象的哈希代码。
        /// </returns>
        public override int GetHashCode()
        {
            if (!this.hashCode.HasValue)
                this.ComputeHash();
            return this.hashCode.Value;
        }

        /// <summary>
        /// 返回表示当前二进制对象的 <see cref="T:System.String"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示当前二进制对象的 <see cref="T:System.String"/>。
        /// </returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string str1 = "\"";
            stringBuilder.Append(str1);
            string str2 = Convert.ToBase64String(this.bytes, 0, this.bytes.Length);
            stringBuilder.Append(str2);
            string str3 = "\"";
            stringBuilder.Append(str3);
            return stringBuilder.ToString();
        }

        private bool EqualsTo(Binary binary)
        {
            if (this == binary)
                return true;
            if (binary == null || this.bytes.Length != binary.bytes.Length || this.GetHashCode() != binary.GetHashCode())
                return false;
            int index = 0;
            for (int length = this.bytes.Length; index < length; ++index)
            {
                if ((int)this.bytes[index] != (int)binary.bytes[index])
                    return false;
            }
            return true;
        }

        private void ComputeHash()
        {
            int num1 = 314;
            int num2 = 159;
            this.hashCode = new int?(0);
            for (int index = 0; index < this.bytes.Length; ++index)
            {
                int? nullable1 = this.hashCode;
                int num3 = num1;
                int? nullable2 = nullable1.HasValue ? new int?(nullable1.GetValueOrDefault() * num3) : new int?();
                int num4 = (int)this.bytes[index];
                this.hashCode = nullable2.HasValue ? new int?(nullable2.GetValueOrDefault() + num4) : new int?();
                num1 *= num2;
            }
        }
    }
}
