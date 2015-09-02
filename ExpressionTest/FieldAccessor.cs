using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class FieldAccessor
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static MetaAccessor Create(Type objectType, FieldInfo fi)
        {
            if (!fi.ReflectedType.IsAssignableFrom(objectType))
                throw Error.InvalidFieldInfo((object)objectType, (object)fi.FieldType, (object)fi);
            Delegate delegate1 = (Delegate)null;
            Delegate delegate2 = (Delegate)null;
            if (!objectType.IsGenericType)
            {
                string name1 = "xget_" + fi.Name;
                Type fieldType1 = fi.FieldType;
                Type[] parameterTypes1 = new Type[1];
                int index1 = 0;
                Type type1 = objectType;
                parameterTypes1[index1] = type1;
                int num1 = 1;
                DynamicMethod dynamicMethod1 = new DynamicMethod(name1, fieldType1, parameterTypes1, num1 != 0);
                ILGenerator ilGenerator1 = dynamicMethod1.GetILGenerator();
                ilGenerator1.Emit(OpCodes.Ldarg_0);
                ilGenerator1.Emit(OpCodes.Ldfld, fi);
                ilGenerator1.Emit(OpCodes.Ret);
                Type type2 = typeof(DGet<,>);
                Type[] typeArray1 = new Type[2];
                int index2 = 0;
                Type type3 = objectType;
                typeArray1[index2] = type3;
                int index3 = 1;
                Type fieldType2 = fi.FieldType;
                typeArray1[index3] = fieldType2;
                Type type4 = type2.MakeGenericType(typeArray1);
                delegate1 = ((MethodInfo)dynamicMethod1).CreateDelegate(type4);
                string name2 = "xset_" + fi.Name;
                Type returnType = typeof(void);
                Type[] parameterTypes2 = new Type[2];
                int index4 = 0;
                Type type5 = objectType.MakeByRefType();
                parameterTypes2[index4] = type5;
                int index5 = 1;
                Type fieldType3 = fi.FieldType;
                parameterTypes2[index5] = fieldType3;
                int num2 = 1;
                DynamicMethod dynamicMethod2 = new DynamicMethod(name2, returnType, parameterTypes2, num2 != 0);
                ILGenerator ilGenerator2 = dynamicMethod2.GetILGenerator();
                ilGenerator2.Emit(OpCodes.Ldarg_0);
                if (!objectType.IsValueType)
                    ilGenerator2.Emit(OpCodes.Ldind_Ref);
                ilGenerator2.Emit(OpCodes.Ldarg_1);
                ilGenerator2.Emit(OpCodes.Stfld, fi);
                ilGenerator2.Emit(OpCodes.Ret);
                Type type6 = typeof(DRSet<,>);
                Type[] typeArray2 = new Type[2];
                int index6 = 0;
                Type type7 = objectType;
                typeArray2[index6] = type7;
                int index7 = 1;
                Type fieldType4 = fi.FieldType;
                typeArray2[index7] = fieldType4;
                Type type8 = type6.MakeGenericType(typeArray2);
                delegate2 = ((MethodInfo)dynamicMethod2).CreateDelegate(type8);
            }
            Type type9 = typeof(FieldAccessor.Accessor<,>);
            Type[] typeArray = new Type[2];
            int index8 = 0;
            Type type10 = objectType;
            typeArray[index8] = type10;
            int index9 = 1;
            Type fieldType = fi.FieldType;
            typeArray[index9] = fieldType;
            Type type11 = type9.MakeGenericType(typeArray);
            int num = 36;
            // ISSUE: variable of the null type
            //__Null local1 = null;
            object[] args = new object[3];
            int index10 = 0;
            FieldInfo fieldInfo = fi;
            args[index10] = (object)fieldInfo;
            int index11 = 1;
            Delegate delegate3 = delegate1;
            args[index11] = (object)delegate3;
            int index12 = 2;
            Delegate delegate4 = delegate2;
            args[index12] = (object)delegate4;
            // ISSUE: variable of the null type
            //__Null local2 = null;
            return (MetaAccessor)Activator.CreateInstance(type11, (BindingFlags)num, (Binder)null, args, (CultureInfo)null);
        }

        private class Accessor<T, V> : MetaAccessor<T, V>
        {
            private DGet<T, V> dget;
            private DRSet<T, V> drset;
            private FieldInfo fi;

            internal Accessor(FieldInfo fi, DGet<T, V> dget, DRSet<T, V> drset)
            {
                this.fi = fi;
                this.dget = dget;
                this.drset = drset;
            }

            public override V GetValue(T instance)
            {
                if (this.dget != null)
                    return this.dget(instance);
                return (V)this.fi.GetValue((object)instance);
            }

            public override void SetValue(ref T instance, V value)
            {
                if (this.drset != null)
                    this.drset(ref instance, value);
                else
                    this.fi.SetValue((object)instance, (object)value);
            }
        }
    }
}
