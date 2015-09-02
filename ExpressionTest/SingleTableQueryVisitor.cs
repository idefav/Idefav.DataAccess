using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SingleTableQueryVisitor : SqlVisitor
    {
        public bool IsValid;
        private bool IsDistinct;
        private List<MemberInfo> IdentityMembers;

        internal SingleTableQueryVisitor()
        {
            this.IsValid = true;
        }

        private void AddIdentityMembers(IEnumerable<MemberInfo> members)
        {
            this.IdentityMembers = new List<MemberInfo>(members);
        }

        internal override SqlNode Visit(SqlNode node)
        {
            if (this.IsValid && node != null)
                return base.Visit(node);
            return node;
        }

        internal override SqlTable VisitTable(SqlTable tab)
        {
            if (this.IsDistinct)
                return tab;
            if (this.IdentityMembers != null)
                this.IsValid = false;
            else
                this.AddIdentityMembers(Enumerable.Select<MetaDataMember, MemberInfo>((IEnumerable<MetaDataMember>)tab.MetaTable.RowType.IdentityMembers, (Func<MetaDataMember, MemberInfo>)(m => m.Member)));
            return tab;
        }

        internal override SqlSource VisitSource(SqlSource source)
        {
            return base.VisitSource(source);
        }

        internal override SqlSelect VisitSelect(SqlSelect select)
        {
            if (select.IsDistinct)
            {
                this.IsDistinct = true;
                this.AddIdentityMembers((IEnumerable<MemberInfo>)select.Selection.ClrType.GetProperties());
                return select;
            }
            select.From = (SqlSource)base.Visit((SqlNode)select.From);
            if (this.IdentityMembers == null || this.IdentityMembers.Count == 0)
                throw Error.SkipRequiresSingleTableQueryWithPKs();
            switch (select.Selection.NodeType)
            {
                case SqlNodeType.Treat:
                case SqlNodeType.TypeCase:
                    return select;
                case SqlNodeType.Member:
                case SqlNodeType.Column:
                case SqlNodeType.ColumnRef:
                    this.IsValid = this.IdentityMembers.Count == 1 && this.IsValid & SingleTableQueryVisitor.IsColumnMatch(this.IdentityMembers[0], select.Selection);
                    goto case SqlNodeType.Treat;
                case SqlNodeType.New:
                case SqlNodeType.AliasRef:
                    select.Selection = this.VisitExpression(select.Selection);
                    goto case SqlNodeType.Treat;
                default:
                    this.IsValid = false;
                    goto case SqlNodeType.Treat;
            }
        }

        internal override SqlExpression VisitNew(SqlNew sox)
        {
            foreach (MemberInfo column in this.IdentityMembers)
            {
                bool flag = false;
                foreach (SqlExpression expr in sox.Args)
                {
                    flag = SingleTableQueryVisitor.IsColumnMatch(column, expr);
                    if (flag)
                        break;
                }
                if (!flag)
                {
                    foreach (SqlMemberAssign sqlMemberAssign in sox.Members)
                    {
                        SqlExpression expression = sqlMemberAssign.Expression;
                        flag = SingleTableQueryVisitor.IsColumnMatch(column, expression);
                        if (flag)
                            break;
                    }
                }
                this.IsValid = this.IsValid & flag;
                if (!this.IsValid)
                    break;
            }
            return (SqlExpression)sox;
        }

        internal override SqlNode VisitUnion(SqlUnion su)
        {
            if (su.All)
                this.IsValid = false;
            this.IsDistinct = true;
            this.AddIdentityMembers((IEnumerable<MemberInfo>)su.GetClrType().GetProperties());
            return (SqlNode)su;
        }

        private static bool IsColumnMatch(MemberInfo column, SqlExpression expr)
        {
            MemberInfo memberInfo = (MemberInfo)null;
            switch (expr.NodeType)
            {
                case SqlNodeType.Column:
                    memberInfo = ((SqlColumn)expr).MetaMember.Member;
                    break;
                case SqlNodeType.ColumnRef:
                    memberInfo = ((SqlColumnRef)expr).Column.MetaMember.Member;
                    break;
                case SqlNodeType.Member:
                    memberInfo = ((SqlMember)expr).Member;
                    break;
            }
            if (memberInfo != (MemberInfo)null)
                return memberInfo == column;
            return false;
        }
    }
}
