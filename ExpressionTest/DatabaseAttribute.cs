using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 指定表示数据库的类的特定属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DatabaseAttribute : Attribute
    {
        private string name;

        /// <summary>
        /// 获取或设置数据库的名称。
        /// </summary>
        /// 
        /// <returns>
        /// 名称。
        /// </returns>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
    }
}
