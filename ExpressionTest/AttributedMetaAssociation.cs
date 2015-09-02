using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class AttributedMetaAssociation : MetaAssociationImpl
    {
        private bool isNullable = true;
        private AttributedMetaDataMember thisMember;
        private MetaDataMember otherMember;
        private ReadOnlyCollection<MetaDataMember> thisKey;
        private ReadOnlyCollection<MetaDataMember> otherKey;
        private MetaType otherType;
        private bool isMany;
        private bool isForeignKey;
        private bool isUnique;
        private bool thisKeyIsPrimaryKey;
        private bool otherKeyIsPrimaryKey;
        private string deleteRule;
        private bool deleteOnNull;

        public override MetaType OtherType
        {
            get
            {
                return this.otherType;
            }
        }

        public override MetaDataMember ThisMember
        {
            get
            {
                return (MetaDataMember)this.thisMember;
            }
        }

        public override MetaDataMember OtherMember
        {
            get
            {
                return this.otherMember;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> ThisKey
        {
            get
            {
                return this.thisKey;
            }
        }

        public override ReadOnlyCollection<MetaDataMember> OtherKey
        {
            get
            {
                return this.otherKey;
            }
        }

        public override bool ThisKeyIsPrimaryKey
        {
            get
            {
                return this.thisKeyIsPrimaryKey;
            }
        }

        public override bool OtherKeyIsPrimaryKey
        {
            get
            {
                return this.otherKeyIsPrimaryKey;
            }
        }

        public override bool IsMany
        {
            get
            {
                return this.isMany;
            }
        }

        public override bool IsForeignKey
        {
            get
            {
                return this.isForeignKey;
            }
        }

        public override bool IsUnique
        {
            get
            {
                return this.isUnique;
            }
        }

        public override bool IsNullable
        {
            get
            {
                return this.isNullable;
            }
        }

        public override string DeleteRule
        {
            get
            {
                return this.deleteRule;
            }
        }

        public override bool DeleteOnNull
        {
            get
            {
                return this.deleteOnNull;
            }
        }

        internal AttributedMetaAssociation(AttributedMetaDataMember member, AssociationAttribute attr)
        {
            this.thisMember = member;
            this.isMany = TypeSystem.IsSequenceType(this.thisMember.Type);
            this.otherType = this.thisMember.DeclaringType.Model.GetMetaType(this.isMany ? TypeSystem.GetElementType(this.thisMember.Type) : this.thisMember.Type);
            this.thisKey = attr.ThisKey != null ? MetaAssociationImpl.MakeKeys(this.thisMember.DeclaringType, attr.ThisKey) : this.thisMember.DeclaringType.IdentityMembers;
            this.otherKey = attr.OtherKey != null ? MetaAssociationImpl.MakeKeys(this.otherType, attr.OtherKey) : this.otherType.IdentityMembers;
            this.thisKeyIsPrimaryKey = MetaAssociationImpl.AreEqual((IEnumerable<MetaDataMember>)this.thisKey, (IEnumerable<MetaDataMember>)this.thisMember.DeclaringType.IdentityMembers);
            this.otherKeyIsPrimaryKey = MetaAssociationImpl.AreEqual((IEnumerable<MetaDataMember>)this.otherKey, (IEnumerable<MetaDataMember>)this.otherType.IdentityMembers);
            this.isForeignKey = attr.IsForeignKey;
            this.isUnique = attr.IsUnique;
            this.deleteRule = attr.DeleteRule;
            this.deleteOnNull = attr.DeleteOnNull;
            foreach (MetaDataMember metaDataMember in this.thisKey)
            {
                if (!metaDataMember.CanBeNull)
                {
                    this.isNullable = false;
                    break;
                }
            }
            if (this.deleteOnNull && (!this.isForeignKey || this.isMany || this.isNullable))
                throw Error.InvalidDeleteOnNullSpecification((object)member);
            if (this.thisKey.Count != this.otherKey.Count && this.thisKey.Count > 0 && this.otherKey.Count > 0)
                throw Error.MismatchedThisKeyOtherKey((object)member.Name, (object)member.DeclaringType.Name);
            foreach (MetaDataMember metaDataMember in this.otherType.PersistentDataMembers)
            {
                AssociationAttribute associationAttribute = (AssociationAttribute)Attribute.GetCustomAttribute(metaDataMember.Member, typeof(AssociationAttribute));
                if (associationAttribute != null && metaDataMember != this.thisMember && associationAttribute.Name == attr.Name)
                {
                    this.otherMember = metaDataMember;
                    break;
                }
            }
        }
    }
}
