using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 提供成员以描述列中数据的属性。
    /// </summary>
    public abstract class DataAttribute : Attribute
    {
        private string name;
        private string storage;

        /// <summary>
        /// 获取或设置列名称。
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

        /// <summary>
        /// 获取或设置私有存储字段以保存列中的值。
        /// </summary>
        /// 
        /// <returns>
        /// 存储字段的名称。
        /// </returns>
        public string Storage
        {
            get
            {
                return this.storage;
            }
            set
            {
                this.storage = value;
            }
        }
    }
}
