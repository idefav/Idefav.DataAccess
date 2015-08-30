using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 一个表示数据库和域对象之间的映射的抽象。
    /// </summary>
    public abstract class MetaModel
    {
        private object identity = new object();

        /// <summary>
        /// 获取生成此模型的映射源。
        /// </summary>
        /// 
        /// <returns>
        /// 原始的映射源。
        /// </returns>
        public abstract MappingSource MappingSource { get; }

        /// <summary>
        /// 获取此模型所描述的 <see cref="T:System.Data.Linq.DataContext"/> 类型的类型。
        /// </summary>
        /// 
        /// <returns>
        /// 数据上下文类型。
        /// </returns>
        public abstract Type ContextType { get; }

        /// <summary>
        /// 获取数据库的名称。
        /// </summary>
        /// 
        /// <returns>
        /// 用字符串表示的数据库名。
        /// </returns>
        public abstract string DatabaseName { get; }

        /// <summary>
        /// 获取或设置提供程序类型。
        /// </summary>
        /// 
        /// <returns>
        /// 提供程序类型。
        /// </returns>
        public abstract Type ProviderType { get; }

        internal object Identity
        {
            get
            {
                return this.identity;
            }
        }

        /// <summary>
        /// 获取与指定 <see cref="T:System.Type"/> 关联的 <see cref="T:System.Data.Linq.Mapping.MetaTable"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 一个与指定的行类型关联的元表。
        /// </returns>
        /// <param name="rowType">公共语言运行时 (CLR) 行类型。</param>
        public abstract MetaTable GetTable(Type rowType);

        /// <summary>
        /// 获取与数据库函数相对应的 <see cref="T:System.Data.Linq.Mapping.MetaFunction"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 与数据库函数相对应的元函数。
        /// </returns>
        /// <param name="method">对 <see cref="T:System.Data.Linq.DataContext"/> 或表示数据库函数的从属类定义的方法。</param>
        public abstract MetaFunction GetFunction(MethodInfo method);

        /// <summary>
        /// 获取所有表的枚举。
        /// </summary>
        /// 
        /// <returns>
        /// 一个可用于循环访问表的枚举器。
        /// </returns>
        public abstract IEnumerable<MetaTable> GetTables();

        /// <summary>
        /// 获取所有函数的枚举。
        /// </summary>
        /// 
        /// <returns>
        /// 一个可用于循环访问所有函数的枚举。
        /// </returns>
        public abstract IEnumerable<MetaFunction> GetFunctions();

        /// <summary>
        /// 发现指定的 <see cref="T:System.Type"/> 的 <see cref="T:System.Data.Linq.Mapping.MetaType"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 一个与指定的类型相对应的元数据类型。
        /// </returns>
        /// <param name="type">为其查找 <see cref="T:System.Data.Linq.Mapping.MetaType"/> 的类型。</param>
        public abstract MetaType GetMetaType(Type type);
    }
}
