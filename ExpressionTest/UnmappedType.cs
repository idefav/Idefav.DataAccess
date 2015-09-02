using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal sealed class UnmappedType : MetaType
    {
        private static ReadOnlyCollection<MetaType> _emptyTypes = new List<MetaType>().AsReadOnly();
        private static ReadOnlyCollection<MetaDataMember> _emptyDataMembers = new List<MetaDataMember>().AsReadOnly();
        private static ReadOnlyCollection<MetaAssociation> _emptyAssociations = new List<MetaAssociation>().AsReadOnly();
        private object locktarget = new object();
        private MetaModel model;
        private Type type;
        private Dictionary<object, MetaDataMember> dataMemberMap;
        private ReadOnlyCollection<MetaDataMember> dataMembers;
        private ReadOnlyCollection<MetaType> inheritanceTypes;

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
                return (MetaTable)null;
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
                return false;
            }
        }

        public override bool CanInstantiate
        {
            get
            {
                return !this.type.IsAbstract;
            }
        }

        public override MetaDataMember DBGeneratedIdentityMember
        {
            get
            {
                return (MetaDataMember)null;
            }
        }

        public override MetaDataMember VersionMember
        {
            get
            {
                return (MetaDataMember)null;
            }
        }

        public override MetaDataMember Discriminator
        {
            get
            {
                return (MetaDataMember)null;
            }
        }

        public override bool HasUpdateCheck
        {
            get
            {
                return false;
            }
        }

        public override ReadOnlyCollection<MetaType> InheritanceTypes
        {
            get
            {
                if (this.inheritanceTypes == null)
                {
                    lock (this.locktarget)
                    {
                        if (this.inheritanceTypes == null)
                        {
                            MetaType[] temp_13 = new MetaType[1];
                            int temp_14 = 0;
                            temp_13[temp_14] = (MetaType)this;
                            this.inheritanceTypes = Enumerable.ToList<MetaType>((IEnumerable<MetaType>)temp_13).AsReadOnly();
                        }
                    }
                }
                return this.inheritanceTypes;
            }
        }

        public override ReadOnlyCollection<MetaType> DerivedTypes
        {
            get
            {
                return UnmappedType._emptyTypes;
            }
        }

        public override bool HasInheritance
        {
            get
            {
                return false;
            }
        }

        public override bool HasInheritanceCode
        {
            get
            {
                return false;
            }
        }

        public override object InheritanceCode
        {
            get
            {
                return (object)null;
            }
        }

        public override MetaType InheritanceRoot
        {
            get
            {
                return (MetaType)this;
            }
        }

        public override MetaType InheritanceBase
        {
            get
            {
                return (MetaType)null;
            }
        }

        public override MetaType InheritanceDefault
        {
            get
            {
                return (MetaType)null;
            }
        }

        public override bool IsInheritanceDefault
        {
            get
            {
                return false;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> DataMembers
        {
            get
            {
                this.InitDataMembers();
                return this.dataMembers;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> PersistentDataMembers
        {
            get
            {
                return UnmappedType._emptyDataMembers;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> IdentityMembers
        {
            get
            {
                this.InitDataMembers();
                return this.dataMembers;
            }
        }

        public override ReadOnlyCollection<MetaAssociation> Associations
        {
            get
            {
                return UnmappedType._emptyAssociations;
            }
        }

        public override MethodInfo OnLoadedMethod
        {
            get
            {
                return (MethodInfo)null;
            }
        }

        public override MethodInfo OnValidateMethod
        {
            get
            {
                return (MethodInfo)null;
            }
        }

        public override bool HasAnyValidateMethod
        {
            get
            {
                return false;
            }
        }

        public override bool HasAnyLoadMethod
        {
            get
            {
                return false;
            }
        }

        internal UnmappedType(MetaModel model, Type type)
        {
            this.model = model;
            this.type = type;
        }

        public override MetaType GetInheritanceType(Type inheritanceType)
        {
            if (inheritanceType == this.type)
                return (MetaType)this;
            return (MetaType)null;
        }

        public override MetaType GetTypeForInheritanceCode(object key)
        {
            return (MetaType)null;
        }

        public override MetaDataMember GetDataMember(MemberInfo mi)
        {
            if (mi == (MemberInfo)null)
                throw Error.ArgumentNull("mi");
            this.InitDataMembers();
            if (this.dataMemberMap == null)
            {
                lock (this.locktarget)
                {
                    if (this.dataMemberMap == null)
                    {
                        Dictionary<object, MetaDataMember> local_4 = new Dictionary<object, MetaDataMember>();
                        foreach (MetaDataMember item_0 in this.dataMembers)
                            local_4.Add(InheritanceRules.DistinguishedMemberName(item_0.Member), item_0);
                        this.dataMemberMap = local_4;
                    }
                }
            }
            MetaDataMember metaDataMember;
            this.dataMemberMap.TryGetValue(InheritanceRules.DistinguishedMemberName(mi), out metaDataMember);
            return metaDataMember;
        }

        private void InitDataMembers()
        {
            if (this.dataMembers != null)
                return;
            lock (this.locktarget)
            {
                if (this.dataMembers != null)
                    return;
                List<MetaDataMember> local_2 = new List<MetaDataMember>();
                int local_3 = 0;
                BindingFlags local_4 = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
                foreach (MemberInfo item_0 in this.type.GetFields(local_4))
                {
                    MetaDataMember local_8 = (MetaDataMember)new UnmappedDataMember((MetaType)this, item_0, local_3);
                    local_2.Add(local_8);
                    ++local_3;
                }
                foreach (MemberInfo item_1 in this.type.GetProperties(local_4))
                {
                    MetaDataMember local_11 = (MetaDataMember)new UnmappedDataMember((MetaType)this, item_1, local_3);
                    local_2.Add(local_11);
                    ++local_3;
                }
                this.dataMembers = local_2.AsReadOnly();
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
