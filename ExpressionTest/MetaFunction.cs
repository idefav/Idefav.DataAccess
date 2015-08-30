using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 表示在上下文方法和数据库函数之间的映射。
    /// </summary>
    public abstract class MetaFunction
    {
        /// <summary>
        /// 获取包含此函数的 <see cref="T:System.Data.Linq.Mapping.MetaModel"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 包含此函数的 <see cref="T:System.Data.Linq.Mapping.MetaModel"/> 对象。
        /// </returns>
        public abstract MetaModel Model { get; }

        /// <summary>
        /// 获取基础上下文方法。
        /// </summary>
        /// 
        /// <returns>
        /// 对应于基础上下文方法的 <see cref="T:System.Reflection.MethodInfo"/> 对象。
        /// </returns>
        public abstract MethodInfo Method { get; }

        /// <summary>
        /// 获取方法的名称。
        /// </summary>
        /// 
        /// <returns>
        /// 表示方法名称的 string。
        /// </returns>
        public abstract string Name { get; }

        /// <summary>
        /// 获取数据库函数或过程的名称。
        /// </summary>
        /// 
        /// <returns>
        /// 一个字符串，表示数据库函数或过程的名称。
        /// </returns>
        public abstract string MappedName { get; }

        /// <summary>
        /// 获取一个值，该值指示是否可在查询内编写函数。
        /// </summary>
        /// 
        /// <returns>
        /// 如果可在查询内编写函数，则为 true。
        /// </returns>
        public abstract bool IsComposable { get; }

        /// <summary>
        /// 获取函数参数的枚举。
        /// </summary>
        /// 
        /// <returns>
        /// 参数的集合。
        /// </returns>
        public abstract ReadOnlyCollection<MetaParameter> Parameters { get; }

        /// <summary>
        /// 获取返回参数。
        /// </summary>
        /// 
        /// <returns>
        /// 对应于返回参数的 <see cref="T:System.Data.Linq.Mapping.MetaParameter"/>。
        /// </returns>
        public abstract MetaParameter ReturnParameter { get; }

        /// <summary>
        /// 获取一个值，指示存储过程是否具有多个结果类型。
        /// </summary>
        /// 
        /// <returns>
        /// 如果存储过程具有多个结果类型，则为 true。
        /// </returns>
        public abstract bool HasMultipleResults { get; }

        /// <summary>
        /// 获取可能的结果行类型的枚举。
        /// </summary>
        /// 
        /// <returns>
        /// 可能的类型的集合。
        /// </returns>
        public abstract ReadOnlyCollection<MetaType> ResultRowTypes { get; }
    }
}
