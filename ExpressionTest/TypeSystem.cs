using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class TypeSystem
    {
        private static ILookup<string, MethodInfo> _sequenceMethods;
        private static ILookup<string, MethodInfo> _queryMethods;

        internal static bool IsSequenceType(Type seqType)
        {
            if (seqType != typeof(string) && seqType != typeof(byte[]) && seqType != typeof(char[]))
                return TypeSystem.FindIEnumerable(seqType) != (Type)null;
            return false;
        }

        internal static bool HasIEnumerable(Type seqType)
        {
            return TypeSystem.FindIEnumerable(seqType) != (Type)null;
        }

        private static Type FindIEnumerable(Type seqType)
        {
            if (seqType == (Type)null || seqType == typeof(string))
                return (Type)null;
            if (seqType.IsArray)
            {
                Type type = typeof(IEnumerable<>);
                Type[] typeArray = new Type[1];
                int index = 0;
                Type elementType = seqType.GetElementType();
                typeArray[index] = elementType;
                return type.MakeGenericType(typeArray);
            }
            if (seqType.IsGenericType)
            {
                foreach (Type type1 in seqType.GetGenericArguments())
                {
                    Type type2 = typeof(IEnumerable<>);
                    Type[] typeArray = new Type[1];
                    int index = 0;
                    Type type3 = type1;
                    typeArray[index] = type3;
                    Type type4 = type2.MakeGenericType(typeArray);
                    if (type4.IsAssignableFrom(seqType))
                        return type4;
                }
            }
            Type[] interfaces = seqType.GetInterfaces();
            if (interfaces != null && interfaces.Length != 0)
            {
                foreach (Type seqType1 in interfaces)
                {
                    Type ienumerable = TypeSystem.FindIEnumerable(seqType1);
                    if (ienumerable != (Type)null)
                        return ienumerable;
                }
            }
            if (seqType.BaseType != (Type)null && seqType.BaseType != typeof(object))
                return TypeSystem.FindIEnumerable(seqType.BaseType);
            return (Type)null;
        }

        internal static Type GetFlatSequenceType(Type elementType)
        {
            Type ienumerable = TypeSystem.FindIEnumerable(elementType);
            if (ienumerable != (Type)null)
                return ienumerable;
            Type type1 = typeof(IEnumerable<>);
            Type[] typeArray = new Type[1];
            int index = 0;
            Type type2 = elementType;
            typeArray[index] = type2;
            return type1.MakeGenericType(typeArray);
        }

        internal static Type GetSequenceType(Type elementType)
        {
            Type type1 = typeof(IEnumerable<>);
            Type[] typeArray = new Type[1];
            int index = 0;
            Type type2 = elementType;
            typeArray[index] = type2;
            return type1.MakeGenericType(typeArray);
        }

        internal static Type GetElementType(Type seqType)
        {
            Type ienumerable = TypeSystem.FindIEnumerable(seqType);
            if (ienumerable == (Type)null)
                return seqType;
            return ienumerable.GetGenericArguments()[0];
        }

        internal static bool IsNullableType(Type type)
        {
            if (type != (Type)null && type.IsGenericType)
                return type.GetGenericTypeDefinition() == typeof(Nullable<>);
            return false;
        }

        internal static bool IsNullAssignable(Type type)
        {
            if (type.IsValueType)
                return TypeSystem.IsNullableType(type);
            return true;
        }

        internal static Type GetNonNullableType(Type type)
        {
            if (TypeSystem.IsNullableType(type))
                return type.GetGenericArguments()[0];
            return type;
        }

        internal static Type GetMemberType(MemberInfo mi)
        {
            FieldInfo fieldInfo = mi as FieldInfo;
            if (fieldInfo != (FieldInfo)null)
                return fieldInfo.FieldType;
            PropertyInfo propertyInfo = mi as PropertyInfo;
            if (propertyInfo != (PropertyInfo)null)
                return propertyInfo.PropertyType;
            EventInfo eventInfo = mi as EventInfo;
            if (eventInfo != (EventInfo)null)
                return eventInfo.EventHandlerType;
            return (Type)null;
        }

        internal static IEnumerable<FieldInfo> GetAllFields(Type type, BindingFlags flags)
        {
            Dictionary<MetaPosition, FieldInfo> dictionary = new Dictionary<MetaPosition, FieldInfo>();
            Type type1 = type;
            do
            {
                foreach (FieldInfo fieldInfo in type1.GetFields(flags))
                {
                    if (fieldInfo.IsPrivate || type == type1)
                    {
                        MetaPosition index = new MetaPosition((MemberInfo)fieldInfo);
                        dictionary[index] = fieldInfo;
                    }
                }
                type1 = type1.BaseType;
            }
            while (type1 != (Type)null);
            return (IEnumerable<FieldInfo>)dictionary.Values;
        }

        internal static IEnumerable<PropertyInfo> GetAllProperties(Type type, BindingFlags flags)
        {
            Dictionary<MetaPosition, PropertyInfo> dictionary = new Dictionary<MetaPosition, PropertyInfo>();
            Type type1 = type;
            do
            {
                foreach (PropertyInfo pi in type1.GetProperties(flags))
                {
                    if (type == type1 || TypeSystem.IsPrivate(pi))
                    {
                        MetaPosition index = new MetaPosition((MemberInfo)pi);
                        dictionary[index] = pi;
                    }
                }
                type1 = type1.BaseType;
            }
            while (type1 != (Type)null);
            return (IEnumerable<PropertyInfo>)dictionary.Values;
        }

        private static bool IsPrivate(PropertyInfo pi)
        {
            MethodInfo methodInfo = pi.GetGetMethod() ?? pi.GetSetMethod();
            if (methodInfo != (MethodInfo)null)
                return methodInfo.IsPrivate;
            return true;
        }

        internal static MethodInfo FindSequenceMethod(string name, Type[] args, params Type[] typeArgs)
        {
            if (TypeSystem._sequenceMethods == null)
                TypeSystem._sequenceMethods = Enumerable.ToLookup<MethodInfo, string>((IEnumerable<MethodInfo>)typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public), (Func<MethodInfo, string>)(m => m.Name));
            MethodInfo methodInfo = Enumerable.FirstOrDefault<MethodInfo>(TypeSystem._sequenceMethods[name], (Func<MethodInfo, bool>)(m => TypeSystem.ArgsMatchExact(m, args, typeArgs)));
            if (methodInfo == (MethodInfo)null)
                return (MethodInfo)null;
            if (typeArgs != null)
                return methodInfo.MakeGenericMethod(typeArgs);
            return methodInfo;
        }

        internal static MethodInfo FindSequenceMethod(string name, IEnumerable sequence)
        {
            string name1 = name;
            Type[] args = new Type[1];
            int index1 = 0;
            Type type = sequence.GetType();
            args[index1] = type;
            Type[] typeArray = new Type[1];
            int index2 = 0;
            Type elementType = TypeSystem.GetElementType(sequence.GetType());
            typeArray[index2] = elementType;
            return TypeSystem.FindSequenceMethod(name1, args, typeArray);
        }

        internal static MethodInfo FindQueryableMethod(string name, Type[] args, params Type[] typeArgs)
        {
            if (TypeSystem._queryMethods == null)
                TypeSystem._queryMethods = Enumerable.ToLookup<MethodInfo, string>((IEnumerable<MethodInfo>)typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public), (Func<MethodInfo, string>)(m => m.Name));
            MethodInfo methodInfo = Enumerable.FirstOrDefault<MethodInfo>(TypeSystem._queryMethods[name], (Func<MethodInfo, bool>)(m => TypeSystem.ArgsMatchExact(m, args, typeArgs)));
            if (methodInfo == (MethodInfo)null)
                throw Error.NoMethodInTypeMatchingArguments((object)typeof(Queryable));
            if (typeArgs != null)
                return methodInfo.MakeGenericMethod(typeArgs);
            return methodInfo;
        }

        internal static MethodInfo FindStaticMethod(Type type, string name, Type[] args, params Type[] typeArgs)
        {
            MethodInfo methodInfo = Enumerable.FirstOrDefault<MethodInfo>((IEnumerable<MethodInfo>)type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), (Func<MethodInfo, bool>)(m =>
            {
                if (m.Name == name)
                    return TypeSystem.ArgsMatchExact(m, args, typeArgs);
                return false;
            }));
            if (methodInfo == (MethodInfo)null)
                throw Error.NoMethodInTypeMatchingArguments((object)type);
            if (typeArgs != null)
                return methodInfo.MakeGenericMethod(typeArgs);
            return methodInfo;
        }

        private static bool ArgsMatchExact(MethodInfo m, Type[] argTypes, Type[] typeArgs)
        {
            ParameterInfo[] parameters = m.GetParameters();
            if (parameters.Length != argTypes.Length)
                return false;
            if (!m.IsGenericMethodDefinition && m.IsGenericMethod && m.ContainsGenericParameters)
                m = m.GetGenericMethodDefinition();
            if (m.IsGenericMethodDefinition)
            {
                if (typeArgs == null || typeArgs.Length == 0 || m.GetGenericArguments().Length != typeArgs.Length)
                    return false;
                m = m.MakeGenericMethod(typeArgs);
                parameters = m.GetParameters();
            }
            else if (typeArgs != null && typeArgs.Length != 0)
                return false;
            int index = 0;
            for (int length = argTypes.Length; index < length; ++index)
            {
                Type parameterType = parameters[index].ParameterType;
                if (parameterType == (Type)null)
                    return false;
                Type c = argTypes[index];
                if (!parameterType.IsAssignableFrom(c))
                    return false;
            }
            return true;
        }

        internal static bool IsSimpleType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments()[0];
            if (type.IsEnum || type == typeof(Guid))
                return true;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Object:
                    if (!(typeof(TimeSpan) == type))
                        return typeof(DateTimeOffset) == type;
                    return true;
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return true;
                default:
                    return false;
            }
        }
    }
}
