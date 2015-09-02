using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class PropertyAccessor
    {
        internal static MetaAccessor Create(Type objectType, PropertyInfo pi, MetaAccessor storageAccessor)
        {
            Delegate delegate1 = (Delegate)null;
            Delegate delegate2 = (Delegate)null;
            Type type1 = typeof(DGet<,>);
            Type[] typeArray1 = new Type[2];
            int index1 = 0;
            Type type2 = objectType;
            typeArray1[index1] = type2;
            int index2 = 1;
            Type propertyType1 = pi.PropertyType;
            typeArray1[index2] = propertyType1;
            Delegate delegate3 = Delegate.CreateDelegate(type1.MakeGenericType(typeArray1), pi.GetGetMethod(true), true);
            if (delegate3 == null)
                throw Error.CouldNotCreateAccessorToProperty((object)objectType, (object)pi.PropertyType, (object)pi);
            if (pi.CanWrite)
            {
                if (!objectType.IsValueType)
                {
                    Type type3 = typeof(DSet<,>);
                    Type[] typeArray2 = new Type[2];
                    int index3 = 0;
                    Type type4 = objectType;
                    typeArray2[index3] = type4;
                    int index4 = 1;
                    Type propertyType2 = pi.PropertyType;
                    typeArray2[index4] = propertyType2;
                    delegate1 = Delegate.CreateDelegate(type3.MakeGenericType(typeArray2), pi.GetSetMethod(true), true);
                }
                else
                {
                    string name = "xset_" + pi.Name;
                    Type returnType = typeof(void);
                    Type[] parameterTypes = new Type[2];
                    int index3 = 0;
                    Type type3 = objectType.MakeByRefType();
                    parameterTypes[index3] = type3;
                    int index4 = 1;
                    Type propertyType2 = pi.PropertyType;
                    parameterTypes[index4] = propertyType2;
                    int num = 1;
                    DynamicMethod dynamicMethod = new DynamicMethod(name, returnType, parameterTypes, num != 0);
                    ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    if (!objectType.IsValueType)
                        ilGenerator.Emit(OpCodes.Ldind_Ref);
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    ilGenerator.Emit(OpCodes.Call, pi.GetSetMethod(true));
                    ilGenerator.Emit(OpCodes.Ret);
                    Type type4 = typeof(DRSet<,>);
                    Type[] typeArray2 = new Type[2];
                    int index5 = 0;
                    Type type5 = objectType;
                    typeArray2[index5] = type5;
                    int index6 = 1;
                    Type propertyType3 = pi.PropertyType;
                    typeArray2[index6] = propertyType3;
                    Type type6 = type4.MakeGenericType(typeArray2);
                    delegate2 = ((MethodInfo)dynamicMethod).CreateDelegate(type6);
                }
            }
            Type type7 = storageAccessor != null ? storageAccessor.Type : pi.PropertyType;
            Type type8 = typeof(PropertyAccessor.Accessor<,,>);
            Type[] typeArray3 = new Type[3];
            int index7 = 0;
            Type type9 = objectType;
            typeArray3[index7] = type9;
            int index8 = 1;
            Type propertyType4 = pi.PropertyType;
            typeArray3[index8] = propertyType4;
            int index9 = 2;
            Type type10 = type7;
            typeArray3[index9] = type10;
            Type type11 = type8.MakeGenericType(typeArray3);
            int num1 = 36;
            // ISSUE: variable of the null type
            //__Null local1 = null;
            object[] args = new object[5];
            int index10 = 0;
            PropertyInfo propertyInfo = pi;
            args[index10] = (object)propertyInfo;
            int index11 = 1;
            Delegate delegate4 = delegate3;
            args[index11] = (object)delegate4;
            int index12 = 2;
            Delegate delegate5 = delegate1;
            args[index12] = (object)delegate5;
            int index13 = 3;
            Delegate delegate6 = delegate2;
            args[index13] = (object)delegate6;
            int index14 = 4;
            MetaAccessor metaAccessor = storageAccessor;
            args[index14] = (object)metaAccessor;
            // ISSUE: variable of the null type
            //__Null local2 = null;
            return (MetaAccessor)Activator.CreateInstance(type11, (BindingFlags)num1, (Binder)null, args, (CultureInfo)null);
        }

        private class Accessor<T, V, V2> : MetaAccessor<T, V> where V2 : V
        {
            private PropertyInfo pi;
            private DGet<T, V> dget;
            private DSet<T, V> dset;
            private DRSet<T, V> drset;
            private MetaAccessor<T, V2> storage;

            internal Accessor(PropertyInfo pi, DGet<T, V> dget, DSet<T, V> dset, DRSet<T, V> drset, MetaAccessor<T, V2> storage)
            {
                this.pi = pi;
                this.dget = dget;
                this.dset = dset;
                this.drset = drset;
                this.storage = storage;
            }

            public override V GetValue(T instance)
            {
                return this.dget(instance);
            }

            public override void SetValue(ref T instance, V value)
            {
                if (this.dset != null)
                    this.dset(instance, value);
                else if (this.drset != null)
                {
                    this.drset(ref instance, value);
                }
                else
                {
                    if (this.storage == null)
                        throw Error.UnableToAssignValueToReadonlyProperty((object)this.pi);
                    this.storage.SetValue(ref instance, (V2)(object)value);
                }
            }
        }
    }

    internal delegate void DRSet<T, V>(ref T t, V v);

    internal delegate void DSet<T, V>(T t, V v);

    internal delegate V DGet<T, V>(T t);
}
