using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal abstract class IdentityManager
    {
        internal abstract object InsertLookup(MetaType type, object instance);

        internal abstract bool RemoveLike(MetaType type, object instance);

        internal abstract object Find(MetaType type, object[] keyValues);

        internal abstract object FindLike(MetaType type, object instance);

        internal static IdentityManager CreateIdentityManager(bool asReadOnly)
        {
            if (asReadOnly)
                return (IdentityManager) new IdentityManager.ReadOnlyIdentityManager();
            return (IdentityManager) new IdentityManager.StandardIdentityManager();
        }

        private class StandardIdentityManager : IdentityManager
        {
            private Dictionary<MetaType, IdentityManager.StandardIdentityManager.IdentityCache> caches;
            private IdentityManager.StandardIdentityManager.IdentityCache currentCache;
            private MetaType currentType;

            internal StandardIdentityManager()
            {
                this.caches = new Dictionary<MetaType, IdentityManager.StandardIdentityManager.IdentityCache>();
            }

            internal override object InsertLookup(MetaType type, object instance)
            {
                this.SetCurrent(type);
                return this.currentCache.InsertLookup(instance);
            }

            internal override bool RemoveLike(MetaType type, object instance)
            {
                this.SetCurrent(type);
                return this.currentCache.RemoveLike(instance);
            }

            internal override object Find(MetaType type, object[] keyValues)
            {
                this.SetCurrent(type);
                return this.currentCache.Find(keyValues);
            }

            internal override object FindLike(MetaType type, object instance)
            {
                this.SetCurrent(type);
                return this.currentCache.FindLike(instance);
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            private void SetCurrent(MetaType type)
            {
                type = type.InheritanceRoot;
                if (this.currentType == type)
                    return;
                if (!this.caches.TryGetValue(type, out this.currentCache))
                {
                    IdentityManager.StandardIdentityManager.KeyManager keyManager1 =
                        IdentityManager.StandardIdentityManager.GetKeyManager(type);
                    Type type1 = typeof (IdentityManager.StandardIdentityManager.IdentityCache<,>);
                    Type[] typeArray = new Type[2];
                    int index1 = 0;
                    Type type2 = type.Type;
                    typeArray[index1] = type2;
                    int index2 = 1;
                    Type keyType = keyManager1.KeyType;
                    typeArray[index2] = keyType;
                    Type type3 = type1.MakeGenericType(typeArray);
                    int num = 52;
                    // ISSUE: variable of the null type
                    //__Null local1 = null;
                    object[] args = new object[1];
                    int index3 = 0;
                    IdentityManager.StandardIdentityManager.KeyManager keyManager2 = keyManager1;
                    args[index3] = (object) keyManager2;
                    // ISSUE: variable of the null type
                    //__Null local2 = null;
                    this.currentCache =
                        (IdentityManager.StandardIdentityManager.IdentityCache)
                            Activator.CreateInstance(type3, (BindingFlags) num, null, args, (CultureInfo) null);
                    this.caches.Add(type, this.currentCache);
                }
                this.currentType = type;
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            private static IdentityManager.StandardIdentityManager.KeyManager GetKeyManager(MetaType type)
            {
                int count = type.IdentityMembers.Count;
                MetaDataMember metaDataMember1 = type.IdentityMembers[0];
                Type type1 = typeof (IdentityManager.StandardIdentityManager.SingleKeyManager<,>);
                Type[] typeArray1 = new Type[2];
                int index1 = 0;
                Type type2 = type.Type;
                typeArray1[index1] = type2;
                int index2 = 1;
                Type type3 = metaDataMember1.Type;
                typeArray1[index2] = type3;
                Type type4 = type1.MakeGenericType(typeArray1);
                int num1 = 52;
                // ISSUE: variable of the null type
                //__Null local1 = null;
                object[] args1 = new object[2];
                int index3 = 0;
                MetaAccessor storageAccessor1 = metaDataMember1.StorageAccessor;
                args1[index3] = (object) storageAccessor1;
                int index4 = 1;
                // ISSUE: variable of a boxed type
                //__Boxed<int> local2 = (ValueType)0;
                args1[index4] = (object) null;
                // ISSUE: variable of the null type
                // __Null local3 = null;
                IdentityManager.StandardIdentityManager.KeyManager keyManager1 =
                    (IdentityManager.StandardIdentityManager.KeyManager)
                        Activator.CreateInstance(type4, (BindingFlags) num1, (Binder) null, args1, (CultureInfo) null);
                for (int index5 = 1; index5 < count; ++index5)
                {
                    MetaDataMember metaDataMember2 = type.IdentityMembers[index5];
                    Type type5 = typeof (IdentityManager.StandardIdentityManager.MultiKeyManager<,,>);
                    Type[] typeArray2 = new Type[3];
                    int index6 = 0;
                    Type type6 = type.Type;
                    typeArray2[index6] = type6;
                    int index7 = 1;
                    Type type7 = metaDataMember2.Type;
                    typeArray2[index7] = type7;
                    int index8 = 2;
                    Type keyType = keyManager1.KeyType;
                    typeArray2[index8] = keyType;
                    Type type8 = type5.MakeGenericType(typeArray2);
                    int num2 = 52;
                    // ISSUE: variable of the null type
                    //__Null local4 = null;
                    object[] args2 = new object[3];
                    int index9 = 0;
                    MetaAccessor storageAccessor2 = metaDataMember2.StorageAccessor;
                    args2[index9] = (object) storageAccessor2;
                    int index10 = 1;
                    // ISSUE: variable of a boxed type
                    //__Boxed<int> local5 = (ValueType)index5;
                    args2[index10] = (object) null;
                    int index11 = 2;
                    IdentityManager.StandardIdentityManager.KeyManager keyManager2 = keyManager1;
                    args2[index11] = (object) keyManager2;
                    // ISSUE: variable of the null type
                    //__Null local6 = null;
                    keyManager1 =
                        (IdentityManager.StandardIdentityManager.KeyManager)
                            Activator.CreateInstance(type8, (BindingFlags) num2, (Binder) null, args2,
                                (CultureInfo) null);
                }
                return keyManager1;
            }

            internal abstract class KeyManager
            {
                internal abstract Type KeyType { get; }
            }

            internal abstract class KeyManager<T, K> : IdentityManager.StandardIdentityManager.KeyManager
            {
                internal abstract IEqualityComparer<K> Comparer { get; }

                internal abstract K CreateKeyFromInstance(T instance);

                internal abstract bool TryCreateKeyFromValues(object[] values, out K k);
            }

            internal class SingleKeyManager<T, V> : IdentityManager.StandardIdentityManager.KeyManager<T, V>
            {
                private bool isKeyNullAssignable;
                private MetaAccessor<T, V> accessor;
                private int offset;
                private IEqualityComparer<V> comparer;

                internal override Type KeyType
                {
                    get { return typeof (V); }
                }

                internal override IEqualityComparer<V> Comparer
                {
                    get
                    {
                        if (this.comparer == null)
                            this.comparer = (IEqualityComparer<V>) EqualityComparer<V>.Default;
                        return this.comparer;
                    }
                }

                internal SingleKeyManager(MetaAccessor<T, V> accessor, int offset)
                {
                    this.accessor = accessor;
                    this.offset = offset;
                    this.isKeyNullAssignable = TypeSystem.IsNullAssignable(typeof (V));
                }

                internal override V CreateKeyFromInstance(T instance)
                {
                    return this.accessor.GetValue(instance);
                }

                internal override bool TryCreateKeyFromValues(object[] values, out V v)
                {
                    object obj = values[this.offset];
                    if (obj == null && !this.isKeyNullAssignable)
                    {
                        v = default(V);
                        return false;
                    }
                    v = (V) obj;
                    return true;
                }
            }

            internal class MultiKeyManager<T, V1, V2> :
                IdentityManager.StandardIdentityManager.KeyManager
                    <T, IdentityManager.StandardIdentityManager.MultiKey<V1, V2>>
            {
                private MetaAccessor<T, V1> accessor;
                private int offset;
                private IdentityManager.StandardIdentityManager.KeyManager<T, V2> next;
                private IEqualityComparer<IdentityManager.StandardIdentityManager.MultiKey<V1, V2>> comparer;

                internal override Type KeyType
                {
                    get { return typeof (IdentityManager.StandardIdentityManager.MultiKey<V1, V2>); }
                }

                internal override IEqualityComparer<IdentityManager.StandardIdentityManager.MultiKey<V1, V2>> Comparer
                {
                    get
                    {
                        if (this.comparer == null)
                            this.comparer =
                                (IEqualityComparer<IdentityManager.StandardIdentityManager.MultiKey<V1, V2>>)
                                    new IdentityManager.StandardIdentityManager.MultiKey<V1, V2>.Comparer(
                                        (IEqualityComparer<V1>) EqualityComparer<V1>.Default, this.next.Comparer);
                        return this.comparer;
                    }
                }

                internal MultiKeyManager(MetaAccessor<T, V1> accessor, int offset,
                    IdentityManager.StandardIdentityManager.KeyManager<T, V2> next)
                {
                    this.accessor = accessor;
                    this.next = next;
                    this.offset = offset;
                }

                internal override IdentityManager.StandardIdentityManager.MultiKey<V1, V2> CreateKeyFromInstance(
                    T instance)
                {
                    return new IdentityManager.StandardIdentityManager.MultiKey<V1, V2>(
                        this.accessor.GetValue(instance), this.next.CreateKeyFromInstance(instance));
                }

                internal override bool TryCreateKeyFromValues(object[] values,
                    out IdentityManager.StandardIdentityManager.MultiKey<V1, V2> k)
                {
                    object obj = values[this.offset];
                    if (obj == null && typeof (V1).IsValueType)
                    {
                        k = new IdentityManager.StandardIdentityManager.MultiKey<V1, V2>();
                        return false;
                    }
                    V2 k1;
                    if (!this.next.TryCreateKeyFromValues(values, out k1))
                    {
                        k = new IdentityManager.StandardIdentityManager.MultiKey<V1, V2>();
                        return false;
                    }
                    k = new IdentityManager.StandardIdentityManager.MultiKey<V1, V2>((V1) obj, k1);
                    return true;
                }
            }

            internal struct MultiKey<T1, T2>
            {
                private T1 value1;
                private T2 value2;

                internal MultiKey(T1 value1, T2 value2)
                {
                    this.value1 = value1;
                    this.value2 = value2;
                }

                internal class Comparer : IEqualityComparer<IdentityManager.StandardIdentityManager.MultiKey<T1, T2>>,
                    IEqualityComparer
                {
                    private IEqualityComparer<T1> comparer1;
                    private IEqualityComparer<T2> comparer2;

                    internal Comparer(IEqualityComparer<T1> comparer1, IEqualityComparer<T2> comparer2)
                    {
                        this.comparer1 = comparer1;
                        this.comparer2 = comparer2;
                    }

                    public bool Equals(IdentityManager.StandardIdentityManager.MultiKey<T1, T2> x,
                        IdentityManager.StandardIdentityManager.MultiKey<T1, T2> y)
                    {
                        if (this.comparer1.Equals(x.value1, y.value1))
                            return this.comparer2.Equals(x.value2, y.value2);
                        return false;
                    }

                    public int GetHashCode(IdentityManager.StandardIdentityManager.MultiKey<T1, T2> x)
                    {
                        return this.comparer1.GetHashCode(x.value1) ^ this.comparer2.GetHashCode(x.value2);
                    }

                    bool IEqualityComparer.Equals(object x, object y)
                    {
                        return this.Equals((IdentityManager.StandardIdentityManager.MultiKey<T1, T2>) x,
                            (IdentityManager.StandardIdentityManager.MultiKey<T1, T2>) y);
                    }

                    int IEqualityComparer.GetHashCode(object x)
                    {
                        return this.GetHashCode((IdentityManager.StandardIdentityManager.MultiKey<T1, T2>) x);
                    }
                }
            }

            internal abstract class IdentityCache
            {
                internal abstract object Find(object[] keyValues);

                internal abstract object FindLike(object instance);

                internal abstract object InsertLookup(object instance);

                internal abstract bool RemoveLike(object instance);
            }

            internal class IdentityCache<T, K> : IdentityManager.StandardIdentityManager.IdentityCache
            {
                private int[] buckets;
                private IdentityManager.StandardIdentityManager.IdentityCache<T, K>.Slot[] slots;
                private int count;
                private int freeList;
                private IdentityManager.StandardIdentityManager.KeyManager<T, K> keyManager;
                private IEqualityComparer<K> comparer;

                public IdentityCache(IdentityManager.StandardIdentityManager.KeyManager<T, K> keyManager)
                {
                    this.keyManager = keyManager;
                    this.comparer = keyManager.Comparer;
                    this.buckets = new int[7];
                    this.slots = new IdentityManager.StandardIdentityManager.IdentityCache<T, K>.Slot[7];
                    this.freeList = -1;
                }

                internal override object InsertLookup(object instance)
                {
                    T instance1 = (T) instance;
                    this.Find(this.keyManager.CreateKeyFromInstance(instance1), ref instance1, true);
                    return (object) instance1;
                }

                internal override bool RemoveLike(object instance)
                {
                    K keyFromInstance = this.keyManager.CreateKeyFromInstance((T) instance);
                    int num = this.comparer.GetHashCode(keyFromInstance) & int.MaxValue;
                    int index1 = num%this.buckets.Length;
                    int index2 = -1;
                    for (int index3 = this.buckets[index1] - 1; index3 >= 0; index3 = this.slots[index3].next)
                    {
                        if (this.slots[index3].hashCode == num &&
                            this.comparer.Equals(this.slots[index3].key, keyFromInstance))
                        {
                            if (index2 < 0)
                                this.buckets[index1] = this.slots[index3].next + 1;
                            else
                                this.slots[index2].next = this.slots[index3].next;
                            this.slots[index3].hashCode = -1;
                            this.slots[index3].value = default(T);
                            this.slots[index3].next = this.freeList;
                            this.freeList = index3;
                            return true;
                        }
                        index2 = index3;
                    }
                    return false;
                }

                internal override object Find(object[] keyValues)
                {
                    K k;
                    if (this.keyManager.TryCreateKeyFromValues(keyValues, out k))
                    {
                        T obj = default(T);
                        if (this.Find(k, ref obj, false))
                            return (object) obj;
                    }
                    return (object) null;
                }

                internal override object FindLike(object instance)
                {
                    T instance1 = (T) instance;
                    if (this.Find(this.keyManager.CreateKeyFromInstance(instance1), ref instance1, false))
                        return (object) instance1;
                    return (object) null;
                }

                private bool Find(K key, ref T value, bool add)
                {
                    int num = this.comparer.GetHashCode(key) & int.MaxValue;
                    for (int index = this.buckets[num%this.buckets.Length] - 1;
                        index >= 0;
                        index = this.slots[index].next)
                    {
                        if (this.slots[index].hashCode == num && this.comparer.Equals(this.slots[index].key, key))
                        {
                            value = this.slots[index].value;
                            return true;
                        }
                    }
                    if (add)
                    {
                        int index1;
                        if (this.freeList >= 0)
                        {
                            index1 = this.freeList;
                            this.freeList = this.slots[index1].next;
                        }
                        else
                        {
                            if (this.count == this.slots.Length)
                                this.Resize();
                            index1 = this.count;
                            this.count = this.count + 1;
                        }
                        int index2 = num%this.buckets.Length;
                        this.slots[index1].hashCode = num;
                        this.slots[index1].key = key;
                        this.slots[index1].value = value;
                        this.slots[index1].next = this.buckets[index2] - 1;
                        this.buckets[index2] = index1 + 1;
                    }
                    return false;
                }

                private void Resize()
                {
                    int length = checked(this.count*2 + 1);
                    int[] numArray = new int[length];
                    IdentityManager.StandardIdentityManager.IdentityCache<T, K>.Slot[] slotArray =
                        new IdentityManager.StandardIdentityManager.IdentityCache<T, K>.Slot[length];
                    Array.Copy((Array) this.slots, 0, (Array) slotArray, 0, this.count);
                    for (int index1 = 0; index1 < this.count; ++index1)
                    {
                        int index2 = slotArray[index1].hashCode%length;
                        slotArray[index1].next = numArray[index2] - 1;
                        numArray[index2] = index1 + 1;
                    }
                    this.buckets = numArray;
                    this.slots = slotArray;
                }

                internal struct Slot
                {
                    internal int hashCode;
                    internal K key;
                    internal T value;
                    internal int next;
                }
            }
        }

        private class ReadOnlyIdentityManager : IdentityManager
        {
            internal ReadOnlyIdentityManager()
            {
            }

            internal override object InsertLookup(MetaType type, object instance)
            {
                return instance;
            }

            internal override bool RemoveLike(MetaType type, object instance)
            {
                return false;
            }

            internal override object Find(MetaType type, object[] keyValues)
            {
                return (object) null;
            }

            internal override object FindLike(MetaType type, object instance)
            {
                return (object) null;
            }
        }
    }

    /// <summary>
    /// <see cref="T:System.Data.Linq.Mapping.MetaAccessor"/> 类的一个强类型版本。
    /// </summary>
    /// <typeparam name="TEntity">源的类型。</typeparam><typeparam name="TMember">该源的成员的类型。</typeparam>
    public abstract class MetaAccessor<TEntity, TMember> : MetaAccessor
    {
        /// <summary>
        /// 获取此访问器访问的成员的类型。
        /// </summary>
        /// 
        /// <returns>
        /// 成员类型。
        /// </returns>
        public override Type Type
        {
            get { return typeof (TMember); }
        }

        /// <summary>
        /// 指定对其设置装箱值的实例。
        /// </summary>
        /// <param name="instance">要将装箱值设置到其中的实例。</param><param name="value">要设置的值。</param>
        public override void SetBoxedValue(ref object instance, object value)
        {
            TEntity instance1 = (TEntity) instance;
            this.SetValue(ref instance1, (TMember) value);
            instance = (object) instance1;
        }

        /// <summary>
        /// 指定对其设置值或从其获取值的一个对象。
        /// </summary>
        /// 
        /// <returns>
        /// 此实例的已装箱值。
        /// </returns>
        /// <param name="instance">从其获取值或对其设置值的实例。</param>
        public override object GetBoxedValue(object instance)
        {
            return (object) this.GetValue((TEntity) instance);
        }

        /// <summary>
        /// 指定强类型值。
        /// </summary>
        /// 
        /// <returns>
        /// 此实例的值。
        /// </returns>
        /// <param name="instance">要从其获取值的实例。</param>
        public abstract TMember GetValue(TEntity instance);

        /// <summary>
        /// 指定对其设置强类型值的实例。
        /// </summary>
        /// <param name="instance">要将值设置到其中的实例。</param><param name="value">要设置的强类型值。</param>
        public abstract void SetValue(ref TEntity instance, TMember value);
    }
}
