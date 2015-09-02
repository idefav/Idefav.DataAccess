using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal sealed class UnmappedDataMember : MetaDataMember
    {
        private object lockTarget = new object();
        private MetaType declaringType;
        private MemberInfo member;
        private int ordinal;
        private Type type;
        private MetaAccessor accPublic;

        public override MetaType DeclaringType
        {
            get
            {
                return this.declaringType;
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
                return this.member;
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
                return this.accPublic;
            }
        }

        public override MetaAccessor DeferredValueAccessor
        {
            get
            {
                return (MetaAccessor)null;
            }
        }

        public override MetaAccessor DeferredSourceAccessor
        {
            get
            {
                return (MetaAccessor)null;
            }
        }

        public override bool IsDeferred
        {
            get
            {
                return false;
            }
        }

        public override bool IsPersistent
        {
            get
            {
                return false;
            }
        }

        public override bool IsAssociation
        {
            get
            {
                return false;
            }
        }

        public override bool IsPrimaryKey
        {
            get
            {
                return false;
            }
        }

        public override bool IsDbGenerated
        {
            get
            {
                return false;
            }
        }

        public override bool IsVersion
        {
            get
            {
                return false;
            }
        }

        public override bool IsDiscriminator
        {
            get
            {
                return false;
            }
        }

        public override bool CanBeNull
        {
            get
            {
                if (this.type.IsValueType)
                    return TypeSystem.IsNullableType(this.type);
                return true;
            }
        }

        public override string DbType
        {
            get
            {
                return (string)null;
            }
        }

        public override string Expression
        {
            get
            {
                return (string)null;
            }
        }

        public override string MappedName
        {
            get
            {
                return this.member.Name;
            }
        }

        public override UpdateCheck UpdateCheck
        {
            get
            {
                return UpdateCheck.Never;
            }
        }

        public override AutoSync AutoSync
        {
            get
            {
                return AutoSync.Never;
            }
        }

        public override MetaAssociation Association
        {
            get
            {
                return (MetaAssociation)null;
            }
        }

        public override MethodInfo LoadMethod
        {
            get
            {
                return (MethodInfo)null;
            }
        }

        internal UnmappedDataMember(MetaType declaringType, MemberInfo mi, int ordinal)
        {
            this.declaringType = declaringType;
            this.member = mi;
            this.ordinal = ordinal;
            this.type = TypeSystem.GetMemberType(mi);
        }

        private void InitAccessors()
        {
            if (this.accPublic != null)
                return;
            lock (this.lockTarget)
            {
                if (this.accPublic != null)
                    return;
                this.accPublic = UnmappedDataMember.MakeMemberAccessor(this.member.ReflectedType, this.member);
            }
        }

        public override bool IsDeclaredBy(MetaType metaType)
        {
            if (metaType == null)
                throw Error.ArgumentNull("metaType");
            return metaType.Type == this.member.DeclaringType;
        }

        private static MetaAccessor MakeMemberAccessor(Type accessorType, MemberInfo mi)
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
                metaAccessor = PropertyAccessor.Create(accessorType, pi, (MetaAccessor)null);
            }
            return metaAccessor;
        }
    }
}
