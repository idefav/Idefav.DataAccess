using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 表示方法参数和数据库函数参数之间的映射。
    /// </summary>
    public abstract class MetaParameter
    {
        /// <summary>
        /// 获取基础方法参数。
        /// </summary>
        /// 
        /// <returns>
        /// 基础方法参数。
        /// </returns>
        public abstract ParameterInfo Parameter { get; }

        /// <summary>
        /// 获取参数名。
        /// </summary>
        /// 
        /// <returns>
        /// 字符串形式的参数名。
        /// </returns>
        public abstract string Name { get; }

        /// <summary>
        /// 获取数据库函数中的参数名。
        /// </summary>
        /// 
        /// <returns>
        /// 字符串形式的名称。
        /// </returns>
        public abstract string MappedName { get; }

        /// <summary>
        /// 获取参数的公共语言运行时 (CLR) 类型。
        /// </summary>
        /// 
        /// <returns>
        /// 类型。
        /// </returns>
        public abstract Type ParameterType { get; }

        /// <summary>
        /// 获取参数的数据库类型。
        /// </summary>
        /// 
        /// <returns>
        /// 字符串形式的参数的数据库类型。
        /// </returns>
        public abstract string DbType { get; }
    }
}
