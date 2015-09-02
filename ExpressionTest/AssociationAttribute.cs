using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 指定表示数据库关联（如外键关系）的属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class AssociationAttribute : DataAttribute
    {
        private string thisKey;
        private string otherKey;
        private bool isUnique;
        private bool isForeignKey;
        private bool deleteOnNull;
        private string deleteRule;

        /// <summary>
        /// 获取或设置表示关联的此端上的键值的此实体类成员。
        /// </summary>
        /// 
        /// <returns>
        /// 默认值为包含类的 ID。
        /// </returns>
        public string ThisKey
        {
            get
            {
                return this.thisKey;
            }
            set
            {
                this.thisKey = value;
            }
        }

        /// <summary>
        /// 获取或设置在关联的另一端上作为键值的、目标实体类的一个或多个成员。
        /// </summary>
        /// 
        /// <returns>
        /// 默认值为相关类的 ID。
        /// </returns>
        public string OtherKey
        {
            get
            {
                return this.otherKey;
            }
            set
            {
                this.otherKey = value;
            }
        }

        /// <summary>
        /// 获取或设置外键上唯一约束的指示。
        /// </summary>
        /// 
        /// <returns>
        /// 默认值 = false。
        /// </returns>
        public bool IsUnique
        {
            get
            {
                return this.isUnique;
            }
            set
            {
                this.isUnique = value;
            }
        }

        /// <summary>
        /// 获取或设置在表示数据库关系的关联中作为外键的成员。
        /// </summary>
        /// 
        /// <returns>
        /// 默认值 = false。
        /// </returns>
        public bool IsForeignKey
        {
            get
            {
                return this.isForeignKey;
            }
            set
            {
                this.isForeignKey = value;
            }
        }

        /// <summary>
        /// 获取或设置关联的删除行为。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示规则的字符串。
        /// </returns>
        public string DeleteRule
        {
            get
            {
                return this.deleteRule;
            }
            set
            {
                this.deleteRule = value;
            }
        }

        /// <summary>
        /// 当指定其外键成员均不可以为 null 的一对一关联时，如果该关联设置为 null，则删除对象。
        /// </summary>
        /// 
        /// <returns>
        /// 设置为 True 可删除对象。默认值为 False。
        /// </returns>
        public bool DeleteOnNull
        {
            get
            {
                return this.deleteOnNull;
            }
            set
            {
                this.deleteOnNull = value;
            }
        }
    }
}
