using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlNodeAnnotations
    {
        private Dictionary<SqlNode, List<SqlNodeAnnotation>> annotationMap = new Dictionary<SqlNode, List<SqlNodeAnnotation>>();
        private Dictionary<Type, string> uniqueTypes = new Dictionary<Type, string>();

        internal void Add(SqlNode node, SqlNodeAnnotation annotation)
        {
            List<SqlNodeAnnotation> list = (List<SqlNodeAnnotation>)null;
            if (!this.annotationMap.TryGetValue(node, out list))
            {
                list = new List<SqlNodeAnnotation>();
                this.annotationMap[node] = list;
            }
            this.uniqueTypes[annotation.GetType()] = string.Empty;
            list.Add(annotation);
        }

        internal List<SqlNodeAnnotation> Get(SqlNode node)
        {
            List<SqlNodeAnnotation> list = (List<SqlNodeAnnotation>)null;
            this.annotationMap.TryGetValue(node, out list);
            return list;
        }

        internal bool NodeIsAnnotated(SqlNode node)
        {
            if (node == null)
                return false;
            return this.annotationMap.ContainsKey(node);
        }

        internal bool HasAnnotationType(Type type)
        {
            return this.uniqueTypes.ContainsKey(type);
        }
    }

    internal abstract class SqlNodeAnnotation
    {
        private string message;

        internal string Message
        {
            get
            {
                return this.message;
            }
        }

        internal SqlNodeAnnotation(string message)
        {
            this.message = message;
        }
    }
}
