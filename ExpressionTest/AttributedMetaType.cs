using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class AttributedMetaType : MetaType
    {
        private object locktarget = new object();
        private MetaModel model;
        private MetaTable table;
        private Type type;
        private Dictionary<MetaPosition, MetaDataMember> dataMemberMap;
        private ReadOnlyCollection<MetaDataMember> dataMembers;
        private ReadOnlyCollection<MetaDataMember> persistentMembers;
        private ReadOnlyCollection<MetaDataMember> identities;
        private MetaDataMember dbGeneratedIdentity;
        private MetaDataMember version;
        private MetaDataMember discriminator;
        private MetaType inheritanceRoot;
        private bool inheritanceBaseSet;
        private MetaType inheritanceBase;
        internal object inheritanceCode;
        private ReadOnlyCollection<MetaType> derivedTypes;
        private ReadOnlyCollection<MetaAssociation> associations;
        private bool hasMethods;
        private bool hasAnyLoadMethod;
        private bool hasAnyValidateMethod;
        private MethodInfo onLoadedMethod;
        private MethodInfo onValidateMethod;

        public override MetaModel Model
        {
            get
            {
                return this.model;
            }
        }

        public override MetaTable Table
        {
            get
            {
                return this.table;
            }
        }

        public override Type Type
        {
            get
            {
                return this.type;
            }
        }

        public override string Name
        {
            get
            {
                return this.type.Name;
            }
        }

        public override bool IsEntity
        {
            get
            {
                if (this.table != null)
                    return this.table.RowType.IdentityMembers.Count > 0;
                return false;
            }
        }

        public override bool CanInstantiate
        {
            get
            {
                if (this.type.IsAbstract)
                    return false;
                if (this != this.InheritanceRoot)
                    return this.HasInheritanceCode;
                return true;
            }
        }

        public override MetaDataMember DBGeneratedIdentityMember
        {
            get
            {
                return this.dbGeneratedIdentity;
            }
        }

        public override MetaDataMember VersionMember
        {
            get
            {
                return this.version;
            }
        }

        public override MetaDataMember Discriminator
        {
            get
            {
                return this.discriminator;
            }
        }

        public override bool HasUpdateCheck
        {
            get
            {
                foreach (MetaDataMember metaDataMember in this.PersistentDataMembers)
                {
                    if (metaDataMember.UpdateCheck != UpdateCheck.Never)
                        return true;
                }
                return false;
            }
        }

        public override bool HasInheritance
        {
            get
            {
                return this.inheritanceRoot.HasInheritance;
            }
        }

        public override bool HasInheritanceCode
        {
            get
            {
                return this.inheritanceCode != null;
            }
        }

        public override object InheritanceCode
        {
            get
            {
                return this.inheritanceCode;
            }
        }

        public override MetaType InheritanceRoot
        {
            get
            {
                return this.inheritanceRoot;
            }
        }

        public override MetaType InheritanceBase
        {
            get
            {
                if (!this.inheritanceBaseSet && this.inheritanceBase == null)
                {
                    lock (this.locktarget)
                    {
                        if (this.inheritanceBase == null)
                        {
                            this.inheritanceBase = InheritanceBaseFinder.FindBase((MetaType)this);
                            this.inheritanceBaseSet = true;
                        }
                    }
                }
                return this.inheritanceBase;
            }
        }

        public override MetaType InheritanceDefault
        {
            get
            {
                return this.InheritanceRoot.InheritanceDefault;
            }
        }

        public override bool IsInheritanceDefault
        {
            get
            {
                return this.InheritanceDefault == this;
            }
        }

        public override ReadOnlyCollection<MetaType> InheritanceTypes
        {
            get
            {
                return this.inheritanceRoot.InheritanceTypes;
            }
        }

        public override ReadOnlyCollection<MetaType> DerivedTypes
        {
            get
            {
                if (this.derivedTypes == null)
                {
                    lock (this.locktarget)
                    {
                        if (this.derivedTypes == null)
                        {
                            List<MetaType> local_2 = new List<MetaType>();
                            foreach (MetaType item_0 in this.InheritanceTypes)
                            {
                                if (item_0.Type.BaseType == this.type)
                                    local_2.Add(item_0);
                            }
                            this.derivedTypes = local_2.AsReadOnly();
                        }
                    }
                }
                return this.derivedTypes;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> DataMembers
        {
            get
            {
                return this.dataMembers;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> PersistentDataMembers
        {
            get
            {
                return this.persistentMembers;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> IdentityMembers
        {
            get
            {
                return this.identities;
            }
        }

        public override ReadOnlyCollection<MetaAssociation> Associations
        {
            get
            {
                if (this.associations == null)
                {
                    lock (this.locktarget)
                    {
                        if (this.associations == null)
                            this.associations = System.Linq.Enumerable.ToList<MetaAssociation>(System.Linq.Enumerable.Select<MetaDataMember, MetaAssociation>(System.Linq.Enumerable.Where<MetaDataMember>((IEnumerable<MetaDataMember>)this.dataMembers, (Func<MetaDataMember, bool>)(m => m.IsAssociation)), (Func<MetaDataMember, MetaAssociation>)(m => m.Association))).AsReadOnly();
                    }
                }
                return this.associations;
            }
        }

        public override MethodInfo OnLoadedMethod
        {
            get
            {
                this.InitMethods();
                return this.onLoadedMethod;
            }
        }

        public override MethodInfo OnValidateMethod
        {
            get
            {
                this.InitMethods();
                return this.onValidateMethod;
            }
        }

        public override bool HasAnyValidateMethod
        {
            get
            {
                this.InitMethods();
                return this.hasAnyValidateMethod;
            }
        }

        public override bool HasAnyLoadMethod
        {
            get
            {
                this.InitMethods();
                return this.hasAnyLoadMethod;
            }
        }

        internal AttributedMetaType(MetaModel model, MetaTable table, Type type, MetaType inheritanceRoot)
        {
            this.model = model;
            this.table = table;
            this.type = type;
            this.inheritanceRoot = inheritanceRoot != null ? inheritanceRoot : (MetaType)this;
            this.InitDataMembers();
            this.identities = System.Linq.Enumerable.ToList<MetaDataMember>(System.Linq.Enumerable.Where<MetaDataMember>((IEnumerable<MetaDataMember>)this.dataMembers, (Func<MetaDataMember, bool>)(m => m.IsPrimaryKey))).AsReadOnly();
            this.persistentMembers = System.Linq.Enumerable.ToList<MetaDataMember>(System.Linq.Enumerable.Where<MetaDataMember>((IEnumerable<MetaDataMember>)this.dataMembers, (Func<MetaDataMember, bool>)(m => m.IsPersistent))).AsReadOnly();
        }

        private void ValidatePrimaryKeyMember(MetaDataMember mm)
        {
            if (mm.IsPrimaryKey && this.inheritanceRoot != this && mm.Member.DeclaringType == this.type)
                throw Error.PrimaryKeyInSubTypeNotSupported((object)this.type.Name, (object)mm.Name);
        }

        private void InitMethods()
        {
            if (this.hasMethods)
                return;
            this.onLoadedMethod = MethodFinder.FindMethod(this.Type, "OnLoaded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes, false);
            Type type1 = this.Type;
            string name = "OnValidate";
            int num1 = 52;
            Type[] argTypes = new Type[1];
            int index = 0;
            Type type2 = typeof(ChangeAction);
            argTypes[index] = type2;
            int num2 = 0;
            this.onValidateMethod = MethodFinder.FindMethod(type1, name, (BindingFlags)num1, argTypes, num2 != 0);
            this.hasAnyLoadMethod = this.onLoadedMethod != (MethodInfo)null || this.InheritanceBase != null && this.InheritanceBase.HasAnyLoadMethod;
            this.hasAnyValidateMethod = this.onValidateMethod != (MethodInfo)null || this.InheritanceBase != null && this.InheritanceBase.HasAnyValidateMethod;
            this.hasMethods = true;
        }

        private void InitDataMembers()
        {
            if (this.dataMembers != null)
                return;
            this.dataMemberMap = new Dictionary<MetaPosition, MetaDataMember>();
            int ordinal = 0;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            FieldInfo[] fieldInfoArray = System.Linq.Enumerable.ToArray<FieldInfo>(TypeSystem.GetAllFields(this.type, flags));
            if (fieldInfoArray != null)
            {
                int index = 0;
                for (int length = fieldInfoArray.Length; index < length; ++index)
                {
                    FieldInfo fieldInfo = fieldInfoArray[index];
                    MetaDataMember mm = (MetaDataMember)new AttributedMetaDataMember(this, (MemberInfo)fieldInfo, ordinal);
                    this.ValidatePrimaryKeyMember(mm);
                    if (mm.IsPersistent || fieldInfo.IsPublic)
                    {
                        this.dataMemberMap.Add(new MetaPosition((MemberInfo)fieldInfo), mm);
                        ++ordinal;
                        if (mm.IsPersistent)
                            this.InitSpecialMember(mm);
                    }
                }
            }
            PropertyInfo[] propertyInfoArray = System.Linq.Enumerable.ToArray<PropertyInfo>(TypeSystem.GetAllProperties(this.type, flags));
            if (propertyInfoArray != null)
            {
                int index = 0;
                for (int length = propertyInfoArray.Length; index < length; ++index)
                {
                    PropertyInfo propertyInfo = propertyInfoArray[index];
                    MetaDataMember mm = (MetaDataMember)new AttributedMetaDataMember(this, (MemberInfo)propertyInfo, ordinal);
                    this.ValidatePrimaryKeyMember(mm);
                    bool flag = propertyInfo.CanRead && propertyInfo.GetGetMethod(false) != (MethodInfo)null && (!propertyInfo.CanWrite || propertyInfo.GetSetMethod(false) != (MethodInfo)null);
                    if (mm.IsPersistent || flag)
                    {
                        this.dataMemberMap.Add(new MetaPosition((MemberInfo)propertyInfo), mm);
                        ++ordinal;
                        if (mm.IsPersistent)
                            this.InitSpecialMember(mm);
                    }
                }
            }
            this.dataMembers = new List<MetaDataMember>((IEnumerable<MetaDataMember>)this.dataMemberMap.Values).AsReadOnly();
        }

        private void InitSpecialMember(MetaDataMember mm)
        {
            if (mm.IsDbGenerated && mm.IsPrimaryKey && string.IsNullOrEmpty(mm.Expression))
            {
                if (this.dbGeneratedIdentity != null)
                    throw Error.TwoMembersMarkedAsPrimaryKeyAndDBGenerated((object)mm.Member, (object)this.dbGeneratedIdentity.Member);
                this.dbGeneratedIdentity = mm;
            }
            if (mm.IsPrimaryKey && !MappingSystem.IsSupportedIdentityType(mm.Type))
                throw Error.IdentityClrTypeNotSupported((object)mm.DeclaringType, (object)mm.Name, (object)mm.Type);
            if (mm.IsVersion)
            {
                if (this.version != null)
                    throw Error.TwoMembersMarkedAsRowVersion((object)mm.Member, (object)this.version.Member);
                this.version = mm;
            }
            if (!mm.IsDiscriminator)
                return;
            if (this.discriminator != null)
                throw Error.TwoMembersMarkedAsInheritanceDiscriminator((object)mm.Member, (object)this.discriminator.Member);
            this.discriminator = mm;
        }

        public override MetaType GetInheritanceType(Type inheritanceType)
        {
            if (inheritanceType == this.type)
                return (MetaType)this;
            return this.inheritanceRoot.GetInheritanceType(inheritanceType);
        }

        public override MetaType GetTypeForInheritanceCode(object key)
        {
            if (this.InheritanceRoot.Discriminator.Type == typeof(string))
            {
                string strB = (string)key;
                foreach (MetaType metaType in this.InheritanceRoot.InheritanceTypes)
                {
                    if (string.Compare((string)metaType.InheritanceCode, strB, StringComparison.OrdinalIgnoreCase) == 0)
                        return metaType;
                }
            }
            else
            {
                foreach (MetaType metaType in this.InheritanceRoot.InheritanceTypes)
                {
                    if (object.Equals(metaType.InheritanceCode, key))
                        return metaType;
                }
            }
            return (MetaType)null;
        }

        public override MetaDataMember GetDataMember(MemberInfo mi)
        {
            if (mi == (MemberInfo)null)
                throw Error.ArgumentNull("mi");
            MetaDataMember metaDataMember = (MetaDataMember)null;
            if (this.dataMemberMap.TryGetValue(new MetaPosition(mi), out metaDataMember))
                return metaDataMember;
            if (mi.DeclaringType.IsInterface)
                throw Error.MappingOfInterfacesMemberIsNotSupported((object)mi.DeclaringType.Name, (object)mi.Name);
            throw Error.UnmappedClassMember((object)mi.DeclaringType.Name, (object)mi.Name);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    internal static class InheritanceBaseFinder
    {
        internal static MetaType FindBase(MetaType derivedType)
        {
            if (derivedType.Type == typeof(object))
                return (MetaType)null;
            Type type1 = derivedType.Type;
            Type type2 = derivedType.InheritanceRoot.Type;
            MetaTable table = derivedType.Table;
            while (!(type1 == typeof(object)) && !(type1 == type2))
            {
                type1 = type1.BaseType;
                MetaType inheritanceType = derivedType.InheritanceRoot.GetInheritanceType(type1);
                if (inheritanceType != null)
                    return inheritanceType;
            }
            return (MetaType)null;
        }
    }
}
