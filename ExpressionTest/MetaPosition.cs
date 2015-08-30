using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal struct MetaPosition : IEqualityComparer<MetaPosition>, IEqualityComparer
    {
        private int metadataToken;
        private Assembly assembly;

        internal MetaPosition(MemberInfo mi)
        {
            this = new MetaPosition(mi.DeclaringType.Assembly, mi.MetadataToken);
        }

        private MetaPosition(Assembly assembly, int metadataToken)
        {
            this.assembly = assembly;
            this.metadataToken = metadataToken;
        }

        public static bool operator ==(MetaPosition x, MetaPosition y)
        {
            return MetaPosition.AreEqual(x, y);
        }

        public static bool operator !=(MetaPosition x, MetaPosition y)
        {
            return !MetaPosition.AreEqual(x, y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
                return false;
            return MetaPosition.AreEqual(this, (MetaPosition)obj);
        }

        public override int GetHashCode()
        {
            return this.metadataToken;
        }

        public bool Equals(MetaPosition x, MetaPosition y)
        {
            return MetaPosition.AreEqual(x, y);
        }

        public int GetHashCode(MetaPosition obj)
        {
            return obj.metadataToken;
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            return this.Equals((MetaPosition)x, (MetaPosition)y);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return this.GetHashCode((MetaPosition)obj);
        }

        private static bool AreEqual(MetaPosition x, MetaPosition y)
        {
            if (x.metadataToken == y.metadataToken)
                return x.assembly == y.assembly;
            return false;
        }

        internal static bool AreSameMember(MemberInfo x, MemberInfo y)
        {
            return x.MetadataToken == y.MetadataToken && !(x.DeclaringType.Assembly != y.DeclaringType.Assembly);
        }
    }
}
