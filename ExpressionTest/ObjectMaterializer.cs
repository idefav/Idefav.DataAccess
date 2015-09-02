using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 定义用于动态具体化对象的方法。
    /// </summary>
    /// <typeparam name="TDataReader">数据读取器的类型。</typeparam>
    public abstract class ObjectMaterializer<TDataReader> where TDataReader : DbDataReader
    {
        /// <summary>
        /// 表示数据读取器的列序号。
        /// </summary>
        public int[] Ordinals;
        /// <summary>
        /// 捕获快速具体化器的内部状态。
        /// </summary>
        public object[] Globals;
        /// <summary>
        /// 捕获快速具体化器的内部状态。
        /// </summary>
        public object[] Locals;
        /// <summary>
        /// 捕获快速具体化器的内部状态。
        /// </summary>
        public object[] Arguments;
        /// <summary>
        /// 表示数据读取器。
        /// </summary>
        public TDataReader DataReader;
        /// <summary>
        /// 表示以只进方式读取数据行的读取器。
        /// </summary>
        public DbDataReader BufferReader;

        /// <summary>
        /// 当在派生类中重写时，获取一个指示是否已启用延迟加载的值。
        /// </summary>
        /// 
        /// <returns>
        /// 如果启用延迟加载，则为 true；否则为 false。
        /// </returns>
        public abstract bool CanDeferLoad { get; }

        /// <summary>
        /// 初始化 <see cref="T:System.Data.Linq.SqlClient.Implementation.ObjectMaterializer`1"/> 类的新实例。
        /// </summary>
        public ObjectMaterializer()
        {
            this.DataReader = default(TDataReader);
        }

        /// <summary>
        /// 当在派生类中重写时，将某个值插入到数据结构中。
        /// </summary>
        /// 
        /// <returns>
        /// 已插入到数据结构中的值。
        /// </returns>
        /// <param name="globalMetaType"><see cref="T:System.Data.Linq.Mapping.MetaType"/> 的索引。</param><param name="instance">要插入到数据结构中的对象。</param>
        public abstract object InsertLookup(int globalMetaType, object instance);

        /// <summary>
        /// 当在派生类中重写时，调用由 <see cref="P:System.Data.Linq.Mapping.MetaType.OnLoadedMethod"/> 表示的方法。
        /// </summary>
        /// <param name="globalMetaType"><see cref="T:System.Data.Linq.Mapping.MetaType"/> 的索引。</param><param name="instance">要传递给调用的方法的参数。</param>
        public abstract void SendEntityMaterialized(int globalMetaType, object instance);

        /// <summary>
        /// 当在派生类中重写时，执行查询。
        /// </summary>
        /// 
        /// <returns>
        /// 执行查询的结果。
        /// </returns>
        /// <param name="iSubQuery">查询的索引。</param><param name="args">查询的参数。</param>
        public abstract IEnumerable ExecuteSubQuery(int iSubQuery, object[] args);

        /// <summary>
        /// 当在派生类中重写时，创建新的延迟源。
        /// </summary>
        /// 
        /// <returns>
        /// 可枚举的延迟源。
        /// </returns>
        /// <param name="globalLink">链接的索引。</param><param name="localFactory">工厂的索引。</param><param name="keyValues">延迟源的键值。</param><typeparam name="T">结果元素的类型。</typeparam>
        public abstract IEnumerable<T> GetLinkSource<T>(int globalLink, int localFactory, object[] keyValues);

        /// <summary>
        /// 当在派生类中重写时，创建新的延迟源。
        /// </summary>
        /// 
        /// <returns>
        /// 可枚举的延迟源。
        /// </returns>
        /// <param name="globalLink">链接的索引。</param><param name="localFactory">工厂的索引。</param><param name="instance">延迟源的实例。</param><typeparam name="T">结果元素的类型。</typeparam>
        public abstract IEnumerable<T> GetNestedLinkSource<T>(int globalLink, int localFactory, object instance);

        /// <summary>
        /// 当在派生类中重写时，将读取器前移至下一条记录。
        /// </summary>
        /// 
        /// <returns>
        /// 如果存在多个行，则为 true；否则为 false。
        /// </returns>
        public abstract bool Read();

        /// <summary>
        /// 更改指定序列中的每个元素的类型。
        /// </summary>
        /// 
        /// <returns>
        /// 包含已转换类型的元素的序列。
        /// </returns>
        /// <param name="source">包含要转换的元素的序列。</param><typeparam name="TOutput">元素要转换为的类型。</typeparam>
        public static IEnumerable<TOutput> Convert<TOutput>(IEnumerable source)
        {
            foreach (object obj in source)
                yield return DBConvert.ChangeType<TOutput>(obj);
        }

        /// <summary>
        /// 从指定键和值集合创建组。
        /// </summary>
        /// 
        /// <returns>
        /// 包含指定键和指定值集合的组。
        /// </returns>
        /// <param name="key">组键。</param><param name="items">组值。</param><typeparam name="TKey">组键的类型。</typeparam><typeparam name="TElement">组中值的类型。</typeparam>
        public static IGrouping<TKey, TElement> CreateGroup<TKey, TElement>(TKey key, IEnumerable<TElement> items)
        {
            return (IGrouping<TKey, TElement>)new ObjectReaderCompiler.Group<TKey, TElement>(key, items);
        }

        /// <summary>
        /// 从指定值集合创建有序序列。
        /// </summary>
        /// 
        /// <returns>
        /// 包含指定值的有序序列。
        /// </returns>
        /// <param name="items">要放置于有序序列中的值。</param><typeparam name="TElement">有序序列中的值类型。</typeparam>
        public static IOrderedEnumerable<TElement> CreateOrderedEnumerable<TElement>(IEnumerable<TElement> items)
        {
            return (IOrderedEnumerable<TElement>)new ObjectReaderCompiler.OrderedResults<TElement>(items);
        }

        /// <summary>
        /// 返回一个异常，指示试图将 null 值分配给不可以为 null 的值类型。
        /// </summary>
        /// 
        /// <returns>
        /// 一个异常，指示试图将 null 值分配给不可以为 null 的值类型。
        /// </returns>
        /// <param name="type">试图将 null 值分配给的类型。</param>
        public static Exception ErrorAssignmentToNull(Type type)
        {
            return Error.CannotAssignNull((object)type);
        }
    }
}
