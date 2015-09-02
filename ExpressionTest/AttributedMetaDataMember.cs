using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal sealed class AttributedMetaDataMember : MetaDataMember
    {
        private object locktarget = new object();
        private AttributedMetaType metaType;
        private MemberInfo member;
        private MemberInfo storageMember;
        private int ordinal;
        private Type type;
        private Type declaringType;
        private bool hasAccessors;
        private MetaAccessor accPublic;
        private MetaAccessor accPrivate;
        private MetaAccessor accDefValue;
        private MetaAccessor accDefSource;
        private DataAttribute attr;
        private ColumnAttribute attrColumn;
        private AssociationAttribute attrAssoc;
        private AttributedMetaAssociation assoc;
        private bool isNullableType;
        private bool isDeferred;
        private bool hasLoadMethod;
        private MethodInfo loadMethod;

        public override MetaType DeclaringType
        {
            get
            {
                return (MetaType)this.metaType;
            }
        }

        public override MemberInfo Member
        {
            get
            {
                return this.member;
            }
        }

        public override MemberInfo StorageMember
        {
            get
            {
                return this.storageMember;
            }
        }

        public override string Name
        {
            get
            {
                return this.member.Name;
            }
        }

        public override int Ordinal
        {
            get
            {
                return this.ordinal;
            }
        }

        public override Type Type
        {
            get
            {
                return this.type;
            }
        }

        public override MetaAccessor MemberAccessor
        {
            get
            {
                this.InitAccessors();
                return this.accPublic;
            }
        }

        public override MetaAccessor StorageAccessor
        {
            get
            {
                this.InitAccessors();
                return this.accPrivate;
            }
        }

        public override MetaAccessor DeferredValueAccessor
        {
            get
            {
                this.InitAccessors();
                return this.accDefValue;
            }
        }

        public override MetaAccessor DeferredSourceAccessor
        {
            get
            {
                this.InitAccessors();
                return this.accDefSource;
            }
        }

        public override bool IsDeferred
        {
            get
            {
                return this.isDeferred;
            }
        }

        public override bool IsPersistent
        {
            get
            {
                if (this.attrColumn == null)
                    return this.attrAssoc != null;
                return true;
            }
        }

        public override bool IsAssociation
        {
            get
            {
                return this.attrAssoc != null;
            }
        }

        public override bool IsPrimaryKey
        {
            get
            {
                if (this.attrColumn != null)
                    return this.attrColumn.IsPrimaryKey;
                return false;
            }
        }

        public override bool IsDbGenerated
        {
            get
            {
                if (this.attrColumn == null || !this.attrColumn.IsDbGenerated && string.IsNullOrEmpty(this.attrColumn.Expression))
                    return this.IsVersion;
                return true;
            }
        }

        public override bool IsVersion
        {
            get
            {
                if (this.attrColumn != null)
                    return this.attrColumn.IsVersion;
                return false;
            }
        }

        public override bool IsDiscriminator
        {
            get
            {
                if (this.attrColumn != null)
                    return this.attrColumn.IsDiscriminator;
                return false;
            }
        }

        public override bool CanBeNull
        {
            get
            {
                if (this.attrColumn == null)
                    return true;
                if (this.attrColumn.CanBeNullSet)
                    return this.attrColumn.CanBeNull;
                if (!this.isNullableType)
                    return !this.type.IsValueType;
                return true;
            }
        }

        public override string DbType
        {
            get
            {
                if (this.attrColumn != null)
                    return this.attrColumn.DbType;
                return (string)null;
            }
        }

        public override string Expression
        {
            get
            {
                if (this.attrColumn != null)
                    return this.attrColumn.Expression;
                return (string)null;
            }
        }

        public override string MappedName
        {
            get
            {
                if (this.attrColumn != null && this.attrColumn.Name != null)
                    return this.attrColumn.Name;
                if (this.attrAssoc != null && this.attrAssoc.Name != null)
                    return this.attrAssoc.Name;
                return this.member.Name;
            }
        }

        public override UpdateCheck UpdateCheck
        {
            get
            {
                if (this.attrColumn != null)
                    return this.attrColumn.UpdateCheck;
                return UpdateCheck.Never;
            }
        }

        public override AutoSync AutoSync
        {
            get
            {
                if (this.attrColumn != null)
                {
                    if (this.IsDbGenerated && this.IsPrimaryKey)
                        return AutoSync.OnInsert;
                    if (this.attrColumn.AutoSync != AutoSync.Default)
                        return this.attrColumn.AutoSync;
                    if (this.IsDbGenerated)
                        return AutoSync.Always;
                }
                return AutoSync.Never;
            }
        }

        public override MetaAssociation Association
        {
            get
            {
                if (this.IsAssociation && this.assoc == null)
                {
                    lock (this.locktarget)
                    {
                        if (this.assoc == null)
                            this.assoc = new AttributedMetaAssociation(this, this.attrAssoc);
                    }
                }
                return (MetaAssociation)this.assoc;
            }
        }

        public override MethodInfo LoadMethod
        {
            get
            {
                if (!this.hasLoadMethod && (this.IsDeferred || this.IsAssociation))
                {
                    Type contextType = this.metaType.Model.ContextType;
                    string name = "Load" + this.member.Name;
                    int num = 52;
                    Type[] argTypes = new Type[1];
                    int index = 0;
                    Type type = this.DeclaringType.Type;
                    argTypes[index] = type;
                    this.loadMethod = MethodFinder.FindMethod(contextType, name, (BindingFlags)num, argTypes);
                    this.hasLoadMethod = true;
                }
                return this.loadMethod;
            }
        }

        internal AttributedMetaDataMember(AttributedMetaType metaType, MemberInfo mi, int ordinal)
        {
            this.declaringType = mi.DeclaringType;
            this.metaType = metaType;
            this.member = mi;
            this.ordinal = ordinal;
            this.type = TypeSystem.GetMemberType(mi);
            this.isNullableType = TypeSystem.IsNullableType(this.type);
            this.attrColumn = (ColumnAttribute)Attribute.GetCustomAttribute(mi, typeof(ColumnAttribute));
            this.attrAssoc = (AssociationAttribute)Attribute.GetCustomAttribute(mi, typeof(AssociationAttribute));
            this.attr = this.attrColumn != null ? (DataAttribute)this.attrColumn : (DataAttribute)this.attrAssoc;
            if (this.attr != null && this.attr.Storage != null)
            {
                MemberInfo[] member = mi.DeclaringType.GetMember(this.attr.Storage, BindingFlags.Instance | BindingFlags.NonPublic);
                if (member == null || member.Length != 1)
                    throw Error.BadStorageProperty((object)this.attr.Storage, (object)mi.DeclaringType, (object)mi.Name);
                this.storageMember = member[0];
            }
            this.isDeferred = this.IsDeferredType(this.storageMember != (MemberInfo)null ? TypeSystem.GetMemberType(this.storageMember) : this.type);
            if (this.attrColumn != null && this.attrColumn.IsDbGenerated && (this.attrColumn.IsPrimaryKey && this.attrColumn.AutoSync != AutoSync.Default) && this.attrColumn.AutoSync != AutoSync.OnInsert)
                throw Error.IncorrectAutoSyncSpecification((object)mi.Name);
        }

        private void InitAccessors()
        {
            if (this.hasAccessors)
                return;
            lock (this.locktarget)
            {
                if (this.hasAccessors)
                    return;
                if (this.storageMember != (MemberInfo)null)
                {
                    this.accPrivate = AttributedMetaDataMember.MakeMemberAccessor(this.member.ReflectedType, this.storageMember, (MetaAccessor)null);
                    if (this.isDeferred)
                        AttributedMetaDataMember.MakeDeferredAccessors(this.member.ReflectedType, this.accPrivate, out this.accPrivate, out this.accDefValue, out this.accDefSource);
                    this.accPublic = AttributedMetaDataMember.MakeMemberAccessor(this.member.ReflectedType, this.member, this.accPrivate);
                }
                else
                {
                    this.accPublic = this.accPrivate = AttributedMetaDataMember.MakeMemberAccessor(this.member.ReflectedType, this.member, (MetaAccessor)null);
                    if (this.isDeferred)
                        AttributedMetaDataMember.MakeDeferredAccessors(this.member.ReflectedType, this.accPrivate, out this.accPrivate, out this.accDefValue, out this.accDefSource);
                }
                this.hasAccessors = true;
            }
        }

        public override bool IsDeclaredBy(MetaType declaringMetaType)
        {
            if (declaringMetaType == null)
                throw Error.ArgumentNull("declaringMetaType");
            return declaringMetaType.Type == this.declaringType;
        }

        private bool IsDeferredType(Type entityType)
        {
            if (entityType == (Type)null || entityType == typeof(object) || !entityType.IsGenericType)
                return false;
            Type genericTypeDefinition = entityType.GetGenericTypeDefinition();
            if (!(genericTypeDefinition == typeof(Link<>)) && !typeof(EntitySet<>).IsAssignableFrom(genericTypeDefinition) && !typeof(EntityRef<>).IsAssignableFrom(genericTypeDefinition))
                return this.IsDeferredType(entityType.BaseType);
            return true;
        }

        private static MetaAccessor MakeMemberAccessor(Type accessorType, MemberInfo mi, MetaAccessor storage)
        {
            FieldInfo fi = mi as FieldInfo;
            MetaAccessor metaAccessor;
            if (fi != (FieldInfo)null)
            {
                metaAccessor = FieldAccessor.Create(accessorType, fi);
            }
            else
            {
                PropertyInfo pi = (PropertyInfo)mi;
                metaAccessor = PropertyAccessor.Create(accessorType, pi, storage);
            }
            return metaAccessor;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void MakeDeferredAccessors(Type objectDeclaringType, MetaAccessor accessor, out MetaAccessor accessorValue, out MetaAccessor accessorDeferredValue, out MetaAccessor accessorDeferredSource)
        {
            if (accessor.Type.IsGenericType)
            {
                Type genericTypeDefinition = accessor.Type.GetGenericTypeDefinition();
                Type type1 = accessor.Type.GetGenericArguments()[0];
                if (genericTypeDefinition == typeof(Link<>))
                {
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                   // MetaAccessor & local1 = @accessorValue;
                    Type type2 = typeof(LinkValueAccessor<,>);
                    Type[] typeArray1 = new Type[2];
                    int index1 = 0;
                    Type type3 = objectDeclaringType;
                    typeArray1[index1] = type3;
                    int index2 = 1;
                    Type type4 = type1;
                    typeArray1[index2] = type4;
                    Type accessorType1 = type2.MakeGenericType(typeArray1);
                    object[] objArray1 = new object[1];
                    int index3 = 0;
                    MetaAccessor metaAccessor1 = accessor;
                    objArray1[index3] = (object)metaAccessor1;
                    MetaAccessor accessor1 = AttributedMetaDataMember.CreateAccessor(accessorType1, objArray1);
          // ISSUE: explicit reference operation
         // ^ local1 = accessor1;
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    //MetaAccessor & local2 = @accessorDeferredValue;
                    Type type5 = typeof(LinkDefValueAccessor<,>);
                    Type[] typeArray2 = new Type[2];
                    int index4 = 0;
                    Type type6 = objectDeclaringType;
                    typeArray2[index4] = type6;
                    int index5 = 1;
                    Type type7 = type1;
                    typeArray2[index5] = type7;
                    Type accessorType2 = type5.MakeGenericType(typeArray2);
                    object[] objArray2 = new object[1];
                    int index6 = 0;
                    MetaAccessor metaAccessor2 = accessor;
                    objArray2[index6] = (object)metaAccessor2;
                    MetaAccessor accessor2 = AttributedMetaDataMember.CreateAccessor(accessorType2, objArray2);
          // ISSUE: explicit reference operation
         // ^ local2 = accessor2;
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    //MetaAccessor & local3 = @accessorDeferredSource;
                    Type type8 = typeof(LinkDefSourceAccessor<,>);
                    Type[] typeArray3 = new Type[2];
                    int index7 = 0;
                    Type type9 = objectDeclaringType;
                    typeArray3[index7] = type9;
                    int index8 = 1;
                    Type type10 = type1;
                    typeArray3[index8] = type10;
                    Type accessorType3 = type8.MakeGenericType(typeArray3);
                    object[] objArray3 = new object[1];
                    int index9 = 0;
                    MetaAccessor metaAccessor3 = accessor;
                    objArray3[index9] = (object)metaAccessor3;
                    MetaAccessor accessor3 = AttributedMetaDataMember.CreateAccessor(accessorType3, objArray3);
          // ISSUE: explicit reference operation
          //^ local3 = accessor3;
                    //return;
                }
                if (typeof(EntityRef<>).IsAssignableFrom(genericTypeDefinition))
                {
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    //MetaAccessor & local1 = @accessorValue;
                    Type type2 = typeof(EntityRefValueAccessor<,>);
                    Type[] typeArray1 = new Type[2];
                    int index1 = 0;
                    Type type3 = objectDeclaringType;
                    typeArray1[index1] = type3;
                    int index2 = 1;
                    Type type4 = type1;
                    typeArray1[index2] = type4;
                    Type accessorType1 = type2.MakeGenericType(typeArray1);
                    object[] objArray1 = new object[1];
                    int index3 = 0;
                    MetaAccessor metaAccessor1 = accessor;
                    objArray1[index3] = (object)metaAccessor1;
                    MetaAccessor accessor1 = AttributedMetaDataMember.CreateAccessor(accessorType1, objArray1);
          // ISSUE: explicit reference operation
         // ^ local1 = accessor1;
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    //MetaAccessor & local2 = @accessorDeferredValue;
                    Type type5 = typeof(EntityRefDefValueAccessor<,>);
                    Type[] typeArray2 = new Type[2];
                    int index4 = 0;
                    Type type6 = objectDeclaringType;
                    typeArray2[index4] = type6;
                    int index5 = 1;
                    Type type7 = type1;
                    typeArray2[index5] = type7;
                    Type accessorType2 = type5.MakeGenericType(typeArray2);
                    object[] objArray2 = new object[1];
                    int index6 = 0;
                    MetaAccessor metaAccessor2 = accessor;
                    objArray2[index6] = (object)metaAccessor2;
                    MetaAccessor accessor2 = AttributedMetaDataMember.CreateAccessor(accessorType2, objArray2);
          // ISSUE: explicit reference operation
          //^ local2 = accessor2;
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    //MetaAccessor & local3 = @accessorDeferredSource;
                    Type type8 = typeof(EntityRefDefSourceAccessor<,>);
                    Type[] typeArray3 = new Type[2];
                    int index7 = 0;
                    Type type9 = objectDeclaringType;
                    typeArray3[index7] = type9;
                    int index8 = 1;
                    Type type10 = type1;
                    typeArray3[index8] = type10;
                    Type accessorType3 = type8.MakeGenericType(typeArray3);
                    object[] objArray3 = new object[1];
                    int index9 = 0;
                    MetaAccessor metaAccessor3 = accessor;
                    objArray3[index9] = (object)metaAccessor3;
                    MetaAccessor accessor3 = AttributedMetaDataMember.CreateAccessor(accessorType3, objArray3);
          // ISSUE: explicit reference operation
          //^ local3 = accessor3;
                    //return;
                }
                if (typeof(EntitySet<>).IsAssignableFrom(genericTypeDefinition))
                {
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    //MetaAccessor & local1 = @accessorValue;
                    Type type2 = typeof(EntitySetValueAccessor<,>);
                    Type[] typeArray1 = new Type[2];
                    int index1 = 0;
                    Type type3 = objectDeclaringType;
                    typeArray1[index1] = type3;
                    int index2 = 1;
                    Type type4 = type1;
                    typeArray1[index2] = type4;
                    Type accessorType1 = type2.MakeGenericType(typeArray1);
                    object[] objArray1 = new object[1];
                    int index3 = 0;
                    MetaAccessor metaAccessor1 = accessor;
                    objArray1[index3] = (object)metaAccessor1;
                    MetaAccessor accessor1 = AttributedMetaDataMember.CreateAccessor(accessorType1, objArray1);
          // ISSUE: explicit reference operation
          //^ local1 = accessor1;
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    //MetaAccessor & local2 = @accessorDeferredValue;
                    Type type5 = typeof(EntitySetDefValueAccessor<,>);
                    Type[] typeArray2 = new Type[2];
                    int index4 = 0;
                    Type type6 = objectDeclaringType;
                    typeArray2[index4] = type6;
                    int index5 = 1;
                    Type type7 = type1;
                    typeArray2[index5] = type7;
                    Type accessorType2 = type5.MakeGenericType(typeArray2);
                    object[] objArray2 = new object[1];
                    int index6 = 0;
                    MetaAccessor metaAccessor2 = accessor;
                    objArray2[index6] = (object)metaAccessor2;
                    MetaAccessor accessor2 = AttributedMetaDataMember.CreateAccessor(accessorType2, objArray2);
          // ISSUE: explicit reference operation
          //^ local2 = accessor2;
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    //MetaAccessor & local3 = @accessorDeferredSource;
                    Type type8 = typeof(EntitySetDefSourceAccessor<,>);
                    Type[] typeArray3 = new Type[2];
                    int index7 = 0;
                    Type type9 = objectDeclaringType;
                    typeArray3[index7] = type9;
                    int index8 = 1;
                    Type type10 = type1;
                    typeArray3[index8] = type10;
                    Type accessorType3 = type8.MakeGenericType(typeArray3);
                    object[] objArray3 = new object[1];
                    int index9 = 0;
                    MetaAccessor metaAccessor3 = accessor;
                    objArray3[index9] = (object)metaAccessor3;
                    MetaAccessor accessor3 = AttributedMetaDataMember.CreateAccessor(accessorType3, objArray3);
          // ISSUE: explicit reference operation
          //^ local3 = accessor3;
                    //return;
                }
            }
            throw Error.UnhandledDeferredStorageType((object)accessor.Type);
        }

        private static MetaAccessor CreateAccessor(Type accessorType, params object[] args)
        {
            return (MetaAccessor)Activator.CreateInstance(accessorType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, args, (CultureInfo)null);
        }

        internal class EntitySetDefValueAccessor<T, V> : MetaAccessor<T, IEnumerable<V>> where V : class
        {
            private MetaAccessor<T, EntitySet<V>> acc;

            internal EntitySetDefValueAccessor(MetaAccessor<T, EntitySet<V>> acc)
            {
                this.acc = acc;
            }

            public override IEnumerable<V> GetValue(T instance)
            {
                return this.acc.GetValue(instance).GetUnderlyingValues();
            }

            public override void SetValue(ref T instance, IEnumerable<V> value)
            {
                EntitySet<V> entitySet = this.acc.GetValue(instance);
                if (entitySet == null)
                {
                    entitySet = new EntitySet<V>();
                    this.acc.SetValue(ref instance, entitySet);
                }
                entitySet.Assign(value);
            }
        }
    }

    internal static class MethodFinder
    {
        internal static MethodInfo FindMethod(Type type, string name, BindingFlags flags, Type[] argTypes)
        {
            return MethodFinder.FindMethod(type, name, flags, argTypes, true);
        }

        internal static MethodInfo FindMethod(Type type, string name, BindingFlags flags, Type[] argTypes, bool allowInherit)
        {
            for (; type != typeof(object); type = type.BaseType)
            {
                MethodInfo method = type.GetMethod(name, flags | BindingFlags.DeclaredOnly, (Binder)null, argTypes, (ParameterModifier[])null);
                if (method != (MethodInfo)null || !allowInherit)
                    return method;
            }
            return (MethodInfo)null;
        }
    }

    internal class LinkDefValueAccessor<T, V> : MetaAccessor<T, V>
    {
        private MetaAccessor<T, Link<V>> acc;

        internal LinkDefValueAccessor(MetaAccessor<T, Link<V>> acc)
        {
            this.acc = acc;
        }

        public override V GetValue(T instance)
        {
            return this.acc.GetValue(instance).UnderlyingValue;
        }

        public override void SetValue(ref T instance, V value)
        {
            this.acc.SetValue(ref instance, new Link<V>(value));
        }
    }

    internal class LinkDefSourceAccessor<T, V> : MetaAccessor<T, IEnumerable<V>>
    {
        private MetaAccessor<T, Link<V>> acc;

        internal LinkDefSourceAccessor(MetaAccessor<T, Link<V>> acc)
        {
            this.acc = acc;
        }

        public override IEnumerable<V> GetValue(T instance)
        {
            return this.acc.GetValue(instance).Source;
        }

        public override void SetValue(ref T instance, IEnumerable<V> value)
        {
            Link<V> link = this.acc.GetValue(instance);
            if (link.HasAssignedValue || link.HasLoadedValue)
                throw Error.LinkAlreadyLoaded();
            this.acc.SetValue(ref instance, new Link<V>(value));
        }
    }

    internal class EntityRefValueAccessor<T, V> : MetaAccessor<T, V> where V : class
    {
        private MetaAccessor<T, EntityRef<V>> acc;

        internal EntityRefValueAccessor(MetaAccessor<T, EntityRef<V>> acc)
        {
            this.acc = acc;
        }

        public override V GetValue(T instance)
        {
            return this.acc.GetValue(instance).Entity;
        }

        public override void SetValue(ref T instance, V value)
        {
            this.acc.SetValue(ref instance, new EntityRef<V>(value));
        }

        public override bool HasValue(object instance)
        {
            return this.acc.GetValue((T)instance).HasValue;
        }

        public override bool HasAssignedValue(object instance)
        {
            return this.acc.GetValue((T)instance).HasAssignedValue;
        }

        public override bool HasLoadedValue(object instance)
        {
            return this.acc.GetValue((T)instance).HasLoadedValue;
        }
    }

    internal class EntityRefDefValueAccessor<T, V> : MetaAccessor<T, V> where V : class
    {
        private MetaAccessor<T, EntityRef<V>> acc;

        internal EntityRefDefValueAccessor(MetaAccessor<T, EntityRef<V>> acc)
        {
            this.acc = acc;
        }

        public override V GetValue(T instance)
        {
            return this.acc.GetValue(instance).UnderlyingValue;
        }

        public override void SetValue(ref T instance, V value)
        {
            this.acc.SetValue(ref instance, new EntityRef<V>(value));
        }
    }

    internal class EntityRefDefSourceAccessor<T, V> : MetaAccessor<T, IEnumerable<V>> where V : class
    {
        private MetaAccessor<T, EntityRef<V>> acc;

        internal EntityRefDefSourceAccessor(MetaAccessor<T, EntityRef<V>> acc)
        {
            this.acc = acc;
        }

        public override IEnumerable<V> GetValue(T instance)
        {
            return this.acc.GetValue(instance).Source;
        }

        public override void SetValue(ref T instance, IEnumerable<V> value)
        {
            EntityRef<V> entityRef = this.acc.GetValue(instance);
            if (entityRef.HasAssignedValue || entityRef.HasLoadedValue)
                throw Error.EntityRefAlreadyLoaded();
            this.acc.SetValue(ref instance, new EntityRef<V>(value));
        }
    }

    internal class EntitySetValueAccessor<T, V> : MetaAccessor<T, EntitySet<V>> where V : class
    {
        private MetaAccessor<T, EntitySet<V>> acc;

        internal EntitySetValueAccessor(MetaAccessor<T, EntitySet<V>> acc)
        {
            this.acc = acc;
        }

        public override EntitySet<V> GetValue(T instance)
        {
            return this.acc.GetValue(instance);
        }

        public override void SetValue(ref T instance, EntitySet<V> value)
        {
            EntitySet<V> entitySet = this.acc.GetValue(instance);
            if (entitySet == null)
            {
                entitySet = new EntitySet<V>();
                this.acc.SetValue(ref instance, entitySet);
            }
            entitySet.Assign((IEnumerable<V>)value);
        }

        public override bool HasValue(object instance)
        {
            EntitySet<V> entitySet = this.acc.GetValue((T)instance);
            if (entitySet != null)
                return entitySet.HasValues;
            return false;
        }

        public override bool HasAssignedValue(object instance)
        {
            EntitySet<V> entitySet = this.acc.GetValue((T)instance);
            if (entitySet != null)
                return entitySet.HasAssignedValues;
            return false;
        }

        public override bool HasLoadedValue(object instance)
        {
            EntitySet<V> entitySet = this.acc.GetValue((T)instance);
            if (entitySet != null)
                return entitySet.HasLoadedValues;
            return false;
        }
    }

    internal class EntitySetDefValueAccessor<T, T1>
    {
    }

    internal class LinkValueAccessor<T, V> : MetaAccessor<T, V>
    {
        private MetaAccessor<T, Link<V>> acc;

        internal LinkValueAccessor(MetaAccessor<T, Link<V>> acc)
        {
            this.acc = acc;
        }

        public override bool HasValue(object instance)
        {
            return this.acc.GetValue((T)instance).HasValue;
        }

        public override bool HasAssignedValue(object instance)
        {
            return this.acc.GetValue((T)instance).HasAssignedValue;
        }

        public override bool HasLoadedValue(object instance)
        {
            return this.acc.GetValue((T)instance).HasLoadedValue;
        }

        public override V GetValue(T instance)
        {
            return this.acc.GetValue(instance).Value;
        }

        public override void SetValue(ref T instance, V value)
        {
            this.acc.SetValue(ref instance, new Link<V>(value));
        }
    }

    internal class EntitySetDefSourceAccessor<T, V> : MetaAccessor<T, IEnumerable<V>> where V : class
    {
        private MetaAccessor<T, EntitySet<V>> acc;

        internal EntitySetDefSourceAccessor(MetaAccessor<T, EntitySet<V>> acc)
        {
            this.acc = acc;
        }

        public override IEnumerable<V> GetValue(T instance)
        {
            return this.acc.GetValue(instance).Source;
        }

        public override void SetValue(ref T instance, IEnumerable<V> value)
        {
            EntitySet<V> entitySet = this.acc.GetValue(instance);
            if (entitySet == null)
            {
                entitySet = new EntitySet<V>();
                this.acc.SetValue(ref instance, entitySet);
            }
            entitySet.SetSource(value);
        }
    }
}
