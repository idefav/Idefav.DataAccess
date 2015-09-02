using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 映射 LINQ to SQL 应用程序中的继承层次结构。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class InheritanceMappingAttribute : Attribute
    {
        private object code;
        private Type type;
        private bool isDefault;

        /// <summary>
        /// 获取或设置映射的继承层次结构中的鉴别器代码值。
        /// </summary>
        /// 
        /// <returns>
        /// 必须由用户指定。没有默认值。
        /// </returns>
        public object Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.code = value;
            }
        }

        /// <summary>
        /// 获取或设置层次结构中类的类型。
        /// </summary>
        /// 
        /// <returns>
        /// 必须由用户指定。没有默认值。
        /// </returns>
        public Type Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示当鉴别器值与指定值不匹配时是否实例化此类型的对象。
        /// </summary>
        /// 
        /// <returns>
        /// 默认值 = false。
        /// </returns>
        public bool IsDefault
        {
            get
            {
                return this.isDefault;
            }
            set
            {
                this.isDefault = value;
            }
        }
    }
}
