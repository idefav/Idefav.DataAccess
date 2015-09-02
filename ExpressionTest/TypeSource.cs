using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class TypeSource
    {
        internal static MetaType GetSourceMetaType(SqlNode node, MetaModel model)
        {
            TypeSource.Visitor visitor = new TypeSource.Visitor();
            SqlNode node1 = node;
            visitor.Visit(node1);
            Type nonNullableType = TypeSystem.GetNonNullableType(visitor.sourceType);
            return model.GetMetaType(nonNullableType);
        }

        internal static SqlExpression GetTypeSource(SqlExpression expr)
        {
            TypeSource.Visitor visitor = new TypeSource.Visitor();
            SqlExpression sqlExpression = expr;
            visitor.Visit((SqlNode)sqlExpression);
            return visitor.sourceExpression;
        }

        private class Visitor : SqlVisitor
        {
            private TypeSource.Visitor.UnwrapStack UnwrapSequences;
            internal SqlExpression sourceExpression;
            internal Type sourceType;

            internal override SqlNode Visit(SqlNode node)
            {
                if (node == null)
                    return (SqlNode)null;
                this.sourceExpression = node as SqlExpression;
                if (this.sourceExpression != null)
                {
                    Type seqType = this.sourceExpression.ClrType;
                    for (TypeSource.Visitor.UnwrapStack unwrapStack = this.UnwrapSequences; unwrapStack != null; unwrapStack = unwrapStack.Last)
                    {
                        if (unwrapStack.Unwrap)
                            seqType = TypeSystem.GetElementType(seqType);
                    }
                    this.sourceType = seqType;
                }
                if (this.sourceType != (Type)null && TypeSystem.GetNonNullableType(this.sourceType).IsValueType || this.sourceType != (Type)null && TypeSystem.HasIEnumerable(this.sourceType))
                    return node;
                switch (node.NodeType)
                {
                    case SqlNodeType.TypeCase:
                        this.sourceType = ((SqlTypeCase)node).RowType.Type;
                        return node;
                    case SqlNodeType.Value:
                        SqlValue sqlValue = (SqlValue)node;
                        if (sqlValue.Value != null)
                            this.sourceType = sqlValue.Value.GetType();
                        return node;
                    case SqlNodeType.SimpleCase:
                    case SqlNodeType.ScalarSubSelect:
                    case SqlNodeType.SearchedCase:
                    case SqlNodeType.Multiset:
                    case SqlNodeType.New:
                    case SqlNodeType.MethodCall:
                    case SqlNodeType.Member:
                    case SqlNodeType.FunctionCall:
                    case SqlNodeType.DiscriminatedType:
                    case SqlNodeType.Element:
                    case SqlNodeType.ClientCase:
                    case SqlNodeType.Convert:
                        return node;
                    case SqlNodeType.Table:
                        this.sourceType = ((SqlTable)node).RowType.Type;
                        return node;
                    case SqlNodeType.Link:
                        this.sourceType = ((SqlLink)node).RowType.Type;
                        return node;
                    default:
                        return base.Visit(node);
                }
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                this.UnwrapSequences = new TypeSource.Visitor.UnwrapStack(this.UnwrapSequences, true);
                this.VisitExpression(select.Selection);
                this.UnwrapSequences = this.UnwrapSequences.Last;
                return select;
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                if (this.UnwrapSequences != null && this.UnwrapSequences.Unwrap)
                {
                    this.UnwrapSequences = new TypeSource.Visitor.UnwrapStack(this.UnwrapSequences, false);
                    this.VisitAlias(aref.Alias);
                    this.UnwrapSequences = this.UnwrapSequences.Last;
                }
                else
                    this.VisitAlias(aref.Alias);
                return (SqlExpression)aref;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                this.VisitColumn(cref.Column);
                return (SqlExpression)cref;
            }

            private class UnwrapStack
            {
                public TypeSource.Visitor.UnwrapStack Last { get; private set; }

                public bool Unwrap { get; private set; }

                public UnwrapStack(TypeSource.Visitor.UnwrapStack last, bool unwrap)
                {
                    this.Last = last;
                    this.Unwrap = unwrap;
                }
            }
        }
    }
}
