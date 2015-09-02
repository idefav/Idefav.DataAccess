using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class SqlServerCompatibilityCheck
    {
        internal static void ThrowIfUnsupported(SqlNode node, SqlNodeAnnotations annotations, SqlProvider.ProviderMode provider)
        {
            if (!annotations.HasAnnotationType(typeof(SqlServerCompatibilityAnnotation)))
                return;
            SqlServerCompatibilityCheck.Visitor visitor = new SqlServerCompatibilityCheck.Visitor(provider);
            visitor.annotations = annotations;
            visitor.Visit(node);
            if (visitor.reasons.Count > 0)
                throw Error.ExpressionNotSupportedForSqlServerVersion(visitor.reasons);
        }

        private class Visitor : SqlVisitor
        {
            internal Collection<string> reasons = new Collection<string>();
            private SqlProvider.ProviderMode provider;
            internal SqlNodeAnnotations annotations;

            internal Visitor(SqlProvider.ProviderMode provider)
            {
                this.provider = provider;
            }

            internal override SqlNode Visit(SqlNode node)
            {
                if (this.annotations.NodeIsAnnotated(node))
                {
                    foreach (SqlNodeAnnotation sqlNodeAnnotation in this.annotations.Get(node))
                    {
                        SqlServerCompatibilityAnnotation compatibilityAnnotation = sqlNodeAnnotation as SqlServerCompatibilityAnnotation;
                        if (compatibilityAnnotation != null && compatibilityAnnotation.AppliesTo(this.provider))
                            this.reasons.Add(sqlNodeAnnotation.Message);
                    }
                }
                return base.Visit(node);
            }
        }
    }

    internal class SqlServerCompatibilityAnnotation : SqlNodeAnnotation
    {
        private SqlProvider.ProviderMode[] providers;

        internal SqlServerCompatibilityAnnotation(string message, params SqlProvider.ProviderMode[] providers)
          : base(message)
        {
            this.providers = providers;
        }

        internal bool AppliesTo(SqlProvider.ProviderMode provider)
        {
            foreach (SqlProvider.ProviderMode providerMode in this.providers)
            {
                if (providerMode == provider)
                    return true;
            }
            return false;
        }
    }
}
