using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{/// <summary>
 /// 表示数据库表或视图的抽象。
 /// </summary>
    public abstract class MetaTable
    {
        /// <summary>
        /// 获取包含此 <see cref="T:System.Data.Linq.Mapping.MetaTable"/> 的 <see cref="T:System.Data.Linq.Mapping.MetaModel"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 包含此 MetaTable 的 <see cref="T:System.Data.Linq.Mapping.MetaModel"/>。
        /// </returns>
        public abstract MetaModel Model { get; }

        /// <summary>
        /// 获取由数据库定义的表的名称。
        /// </summary>
        /// 
        /// <returns>
        /// 表示表名称的 string。
        /// </returns>
        public abstract string TableName { get; }

        /// <summary>
        /// 获取描述表行类型的 <see cref="T:System.Data.Linq.Mapping.MetaType"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 表中的行的类型。
        /// </returns>
        public abstract MetaType RowType { get; }

        /// <summary>
        /// 获取用于执行插入操作的 <see cref="T:System.Data.Linq.DataContext"/> 方法。
        /// </summary>
        /// 
        /// <returns>
        /// <see cref="T:System.Reflection.MethodInfo"/>，它对应于用于插入操作的方法。
        /// </returns>
        public abstract MethodInfo InsertMethod { get; }

        /// <summary>
        /// 获取用于执行更新操作的 <see cref="T:System.Data.Linq.DataContext"/> 方法。
        /// </summary>
        /// 
        /// <returns>
        /// <see cref="T:System.Reflection.MethodInfo"/>，它对应于用于更新操作的方法。
        /// </returns>
        public abstract MethodInfo UpdateMethod { get; }

        /// <summary>
        /// 获取用于执行删除操作的 <see cref="T:System.Data.Linq.DataContext"/> 方法。
        /// </summary>
        /// 
        /// <returns>
        /// <see cref="T:System.Reflection.MethodInfo"/>，它对应于用于删除操作的方法。
        /// </returns>
        public abstract MethodInfo DeleteMethod { get; }
    }
}
