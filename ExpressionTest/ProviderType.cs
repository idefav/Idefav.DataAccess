using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal abstract class ProviderType
    {
        internal abstract bool IsUnicodeType { get; }

        internal abstract bool IsRuntimeOnlyType { get; }

        internal abstract bool IsApplicationType { get; }

        internal abstract bool SupportsComparison { get; }

        internal abstract bool SupportsLength { get; }

        internal abstract bool IsLargeType { get; }

        internal abstract bool IsFixedSize { get; }

        internal abstract bool HasSizeOrIsLarge { get; }

        internal abstract int? Size { get; }

        internal abstract bool IsOrderable { get; }

        internal abstract bool IsGroupable { get; }

        internal abstract bool CanBeColumn { get; }

        internal abstract bool CanBeParameter { get; }

        internal abstract bool IsChar { get; }

        internal abstract bool IsString { get; }

        internal abstract bool IsNumeric { get; }

        internal abstract bool HasPrecisionAndScale { get; }

        internal abstract bool CanSuppressSizeForConversionToString { get; }

        public static bool operator ==(ProviderType typeA, ProviderType typeB)
        {
            if (typeA == typeB)
                return true;
            if (typeA != null)
                return typeA.Equals((object)typeB);
            return false;
        }

        public static bool operator !=(ProviderType typeA, ProviderType typeB)
        {
            if (typeA == typeB)
                return false;
            if (typeA != null)
                return !typeA.Equals((object)typeB);
            return true;
        }

        internal abstract ProviderType GetNonUnicodeEquivalent();

        internal abstract bool IsApplicationTypeOf(int index);

        internal abstract Type GetClosestRuntimeType();

        internal abstract int ComparePrecedenceTo(ProviderType type);

        internal abstract bool IsSameTypeFamily(ProviderType type);

        internal abstract bool AreValuesEqual(object o1, object o2);

        internal abstract string ToQueryString();

        internal abstract string ToQueryString(QueryFormatOptions formatOptions);

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
