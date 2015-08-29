using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Idefav.Utility
{
    public class ExpressionToSQL
    {
        public string ToSQL<T>(Expression<Func<T, bool>> whereExpression)
        {
            string sql = "";
            //ResolveExpression resolveExpression=new ResolveExpression();
            //resolveExpression.ResolveToSql(whereExpression);
            //sql = resolveExpression.SqlWhere;
            sql = JxBody(whereExpression.Body);
            return sql;
        }

        public string JxBody(Expression body)
        {
            string left = "";
            string operation = "";
            string right = "";
            if (body.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression member = (MemberExpression)body;
                left = member.Member.Name;

            }
            else if (body.NodeType == ExpressionType.Constant)
            {
                ConstantExpression constant = body as ConstantExpression;

                if (constant.Type == typeof(String))
                {
                    left = string.Format("'{0}'", constant.Value);
                }
                else
                {
                    left = constant.Value.ToString();
                }

            }
            else if(body.NodeType==ExpressionType.Convert)
            {
                var o = body as UnaryExpression;
                left = o.Operand.ToString();
            }
            else
            {
                BinaryExpression binary = body as BinaryExpression;
                left = JxBody(binary.Left);
                operation = NodeTypeToSQL(body.NodeType);
                right = JxBody(binary.Right);
            }

            return left + operation + right;
        }

        private string NodeTypeToSQL(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Add:
                case ExpressionType.AddAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.AndAssign:
                    return " AND ";
                case ExpressionType.Divide:
                case ExpressionType.DivideAssign:
                    return " / ";
                case ExpressionType.LessThan:
                    return " < ";
                case ExpressionType.LessThanOrEqual:
                    return " <= ";
                case ExpressionType.GreaterThan:
                    return " > ";
                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.MultiplyChecked:
                    return " * ";
                case ExpressionType.OrElse:
                    return " OR ";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractAssign:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.SubtractChecked:
                    return " - ";
                case ExpressionType.Equal:
                    return " = ";
                case ExpressionType.NotEqual:
                    return " <> ";
                default:
                    return "";
            }
        }
    }
}
