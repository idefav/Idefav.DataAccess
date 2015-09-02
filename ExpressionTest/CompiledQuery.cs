using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 提供对查询的编译和缓存以供重新使用。
    /// </summary>
    public sealed class CompiledQuery
    {
        private LambdaExpression query;
        private ICompiledQuery compiled;
        private MappingSource mappingSource;

        /// <summary>
        /// 将查询返回为 lambda 表达式。
        /// </summary>
        /// 
        /// <returns>
        /// 表示查询的 lambda 表达式。
        /// </returns>
        public LambdaExpression Expression
        {
            get
            {
                return this.query;
            }
        }

        private CompiledQuery(LambdaExpression query)
        {
            this.query = query;
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TResult> Compile<TArg0, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TResult> Compile<TArg0, TArg1, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TResult> Compile<TArg0, TArg1, TArg2, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg4">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg4">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg5">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg4">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg5">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg6">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg4">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg5">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg6">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg7">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg4">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg5">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg6">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg7">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg8">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg4">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg5">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg6">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg7">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg8">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg9">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg4">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg5">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg6">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg7">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg8">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg9">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg10">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg4">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg5">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg6">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg7">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg8">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg9">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg10">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg11">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg4">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg5">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg6">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg7">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg8">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg9">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg10">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg11">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg12">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg4">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg5">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg6">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg7">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg8">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg9">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg10">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg11">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg12">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg13">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg4">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg5">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg6">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg7">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg8">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg9">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg10">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg11">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg12">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg13">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg14">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>);
        }

        /// <summary>
        /// 编译查询。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示已编译查询的泛型委托。
        /// </returns>
        /// <param name="query">要编译的查询表达式。</param><typeparam name="TArg0">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg1">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg2">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg3">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg4">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg5">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg6">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg7">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg8">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg9">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg10">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg11">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg12">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg13">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg14">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TArg15">表示在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时必须传入的参数的类型。</typeparam><typeparam name="TResult">在执行由 <see cref="M:System.Data.Linq.CompiledQuery.Compile``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/> 方法返回的委托时返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 中的 T 的类型。</typeparam>
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(System.Linq.Expressions.Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>> query) where TArg0 : DataContext
        {
            if (query == null)
                Error.ArgumentNull("query");
            if (CompiledQuery.UseExpressionCompile((LambdaExpression)query))
                return query.Compile();
            return new Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(new CompiledQuery((LambdaExpression)query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>);
        }

        private static bool UseExpressionCompile(LambdaExpression query)
        {
            return typeof(ITable).IsAssignableFrom(query.Body.Type);
        }

        private TResult Invoke<TArg0, TResult>(TArg0 arg0) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[1];
            int index = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index] = (object)local2;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TResult>(TArg0 arg0, TArg1 arg1) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
           var local1 = (object)arg0;
            object[] args = new object[2];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[3];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[4];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[5];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            int index5 = 4;
            // ISSUE: variable of a boxed type
            var local6 = (object)arg4;
            args[index5] = (object)local6;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[6];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            int index5 = 4;
            // ISSUE: variable of a boxed type
            var local6 = (object)arg4;
            args[index5] = (object)local6;
            int index6 = 5;
            // ISSUE: variable of a boxed type
            var local7 = (object)arg5;
            args[index6] = (object)local7;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[7];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            int index5 = 4;
            // ISSUE: variable of a boxed type
            var local6 = (object)arg4;
            args[index5] = (object)local6;
            int index6 = 5;
            // ISSUE: variable of a boxed type
            var local7 = (object)arg5;
            args[index6] = (object)local7;
            int index7 = 6;
            // ISSUE: variable of a boxed type
            var local8 = (object)arg6;
            args[index7] = (object)local8;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[8];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            int index5 = 4;
            // ISSUE: variable of a boxed type
            var local6 = (object)arg4;
            args[index5] = (object)local6;
            int index6 = 5;
            // ISSUE: variable of a boxed type
            var local7 = (object)arg5;
            args[index6] = (object)local7;
            int index7 = 6;
            // ISSUE: variable of a boxed type
            var local8 = (object)arg6;
            args[index7] = (object)local8;
            int index8 = 7;
            // ISSUE: variable of a boxed type
            var local9 = (object)arg7;
            args[index8] = (object)local9;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[9];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            int index5 = 4;
            // ISSUE: variable of a boxed type
            var local6 = (object)arg4;
            args[index5] = (object)local6;
            int index6 = 5;
            // ISSUE: variable of a boxed type
            var local7 = (object)arg5;
            args[index6] = (object)local7;
            int index7 = 6;
            // ISSUE: variable of a boxed type
            var local8 = (object)arg6;
            args[index7] = (object)local8;
            int index8 = 7;
            // ISSUE: variable of a boxed type
            var local9 = (object)arg7;
            args[index8] = (object)local9;
            int index9 = 8;
            // ISSUE: variable of a boxed type
            var local10 = (object)arg8;
            args[index9] = (object)local10;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[10];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            int index5 = 4;
            // ISSUE: variable of a boxed type
            var local6 = (object)arg4;
            args[index5] = (object)local6;
            int index6 = 5;
            // ISSUE: variable of a boxed type
            var local7 = (object)arg5;
            args[index6] = (object)local7;
            int index7 = 6;
            // ISSUE: variable of a boxed type
            var local8 = (object)arg6;
            args[index7] = (object)local8;
            int index8 = 7;
            // ISSUE: variable of a boxed type
            var local9 = (object)arg7;
            args[index8] = (object)local9;
            int index9 = 8;
            // ISSUE: variable of a boxed type
            var local10 = (object)arg8;
            args[index9] = (object)local10;
            int index10 = 9;
            // ISSUE: variable of a boxed type
            var local11 = (object)arg9;
            args[index10] = (object)local11;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[11];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            int index5 = 4;
            // ISSUE: variable of a boxed type
            var local6 = (object)arg4;
            args[index5] = (object)local6;
            int index6 = 5;
            // ISSUE: variable of a boxed type
            var local7 = (object)arg5;
            args[index6] = (object)local7;
            int index7 = 6;
            // ISSUE: variable of a boxed type
            var local8 = (object)arg6;
            args[index7] = (object)local8;
            int index8 = 7;
            // ISSUE: variable of a boxed type
            var local9 = (object)arg7;
            args[index8] = (object)local9;
            int index9 = 8;
            // ISSUE: variable of a boxed type
            var local10 = (object)arg8;
            args[index9] = (object)local10;
            int index10 = 9;
            // ISSUE: variable of a boxed type
            var local11 = (object)arg9;
            args[index10] = (object)local11;
            int index11 = 10;
            // ISSUE: variable of a boxed type
            var local12 = (object)arg10;
            args[index11] = (object)local12;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[12];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            int index5 = 4;
            // ISSUE: variable of a boxed type
            var local6 = (object)arg4;
            args[index5] = (object)local6;
            int index6 = 5;
            // ISSUE: variable of a boxed type
            var local7 = (object)arg5;
            args[index6] = (object)local7;
            int index7 = 6;
            // ISSUE: variable of a boxed type
            var local8 = (object)arg6;
            args[index7] = (object)local8;
            int index8 = 7;
            // ISSUE: variable of a boxed type
            var local9 = (object)arg7;
            args[index8] = (object)local9;
            int index9 = 8;
            // ISSUE: variable of a boxed type
            var local10 = (object)arg8;
            args[index9] = (object)local10;
            int index10 = 9;
            // ISSUE: variable of a boxed type
            var local11 = (object)arg9;
            args[index10] = (object)local11;
            int index11 = 10;
            // ISSUE: variable of a boxed type
            var local12 = (object)arg10;
            args[index11] = (object)local12;
            int index12 = 11;
            // ISSUE: variable of a boxed type
            var local13 = (object)arg11;
            args[index12] = (object)local13;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[13];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            int index5 = 4;
            // ISSUE: variable of a boxed type
            var local6 = (object)arg4;
            args[index5] = (object)local6;
            int index6 = 5;
            // ISSUE: variable of a boxed type
            var local7 = (object)arg5;
            args[index6] = (object)local7;
            int index7 = 6;
            // ISSUE: variable of a boxed type
            var local8 = (object)arg6;
            args[index7] = (object)local8;
            int index8 = 7;
            // ISSUE: variable of a boxed type
            var local9 = (object)arg7;
            args[index8] = (object)local9;
            int index9 = 8;
            // ISSUE: variable of a boxed type
            var local10 = (object)arg8;
            args[index9] = (object)local10;
            int index10 = 9;
            // ISSUE: variable of a boxed type
            var local11 = (object)arg9;
            args[index10] = (object)local11;
            int index11 = 10;
            // ISSUE: variable of a boxed type
            var local12 = (object)arg10;
            args[index11] = (object)local12;
            int index12 = 11;
            // ISSUE: variable of a boxed type
            var local13 = (object)arg11;
            args[index12] = (object)local13;
            int index13 = 12;
            // ISSUE: variable of a boxed type
            var local14 = (object)arg12;
            args[index13] = (object)local14;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[14];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            int index5 = 4;
            // ISSUE: variable of a boxed type
            var local6 = (object)arg4;
            args[index5] = (object)local6;
            int index6 = 5;
            // ISSUE: variable of a boxed type
            var local7 = (object)arg5;
            args[index6] = (object)local7;
            int index7 = 6;
            // ISSUE: variable of a boxed type
            var local8 = (object)arg6;
            args[index7] = (object)local8;
            int index8 = 7;
            // ISSUE: variable of a boxed type
            var local9 = (object)arg7;
            args[index8] = (object)local9;
            int index9 = 8;
            // ISSUE: variable of a boxed type
            var local10 = (object)arg8;
            args[index9] = (object)local10;
            int index10 = 9;
            // ISSUE: variable of a boxed type
            var local11 = (object)arg9;
            args[index10] = (object)local11;
            int index11 = 10;
            // ISSUE: variable of a boxed type
            var local12 = (object)arg10;
            args[index11] = (object)local12;
            int index12 = 11;
            // ISSUE: variable of a boxed type
            var local13 = (object)arg11;
            args[index12] = (object)local13;
            int index13 = 12;
            // ISSUE: variable of a boxed type
            var local14 = (object)arg12;
            args[index13] = (object)local14;
            int index14 = 13;
            // ISSUE: variable of a boxed type
            var local15 = (object)arg13;
            args[index14] = (object)local15;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[15];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            int index5 = 4;
            // ISSUE: variable of a boxed type
            var local6 = (object)arg4;
            args[index5] = (object)local6;
            int index6 = 5;
            // ISSUE: variable of a boxed type
            var local7 = (object)arg5;
            args[index6] = (object)local7;
            int index7 = 6;
            // ISSUE: variable of a boxed type
            var local8 = (object)arg6;
            args[index7] = (object)local8;
            int index8 = 7;
            // ISSUE: variable of a boxed type
            var local9 = (object)arg7;
            args[index8] = (object)local9;
            int index9 = 8;
            // ISSUE: variable of a boxed type
            var local10 = (object)arg8;
            args[index9] = (object)local10;
            int index10 = 9;
            // ISSUE: variable of a boxed type
            var local11 = (object)arg9;
            args[index10] = (object)local11;
            int index11 = 10;
            // ISSUE: variable of a boxed type
           var local12 = (object)arg10;
            args[index11] = (object)local12;
            int index12 = 11;
            // ISSUE: variable of a boxed type
            var local13 = (object)arg11;
            args[index12] = (object)local13;
            int index13 = 12;
            // ISSUE: variable of a boxed type
            var local14 = (object)arg12;
            args[index13] = (object)local14;
            int index14 = 13;
            // ISSUE: variable of a boxed type
            var local15 = (object)arg13;
            args[index14] = (object)local15;
            int index15 = 14;
            // ISSUE: variable of a boxed type
            var local16 = (object)arg14;
            args[index15] = (object)local16;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15) where TArg0 : DataContext
        {
            // ISSUE: variable of a boxed type
            var local1 = (object)arg0;
            object[] args = new object[16];
            int index1 = 0;
            // ISSUE: variable of a boxed type
            var local2 = (object)arg0;
            args[index1] = (object)local2;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local3 = (object)arg1;
            args[index2] = (object)local3;
            int index3 = 2;
            // ISSUE: variable of a boxed type
            var local4 = (object)arg2;
            args[index3] = (object)local4;
            int index4 = 3;
            // ISSUE: variable of a boxed type
            var local5 = (object)arg3;
            args[index4] = (object)local5;
            int index5 = 4;
            // ISSUE: variable of a boxed type
            var local6 = (object)arg4;
            args[index5] = (object)local6;
            int index6 = 5;
            // ISSUE: variable of a boxed type
            var local7 = (object)arg5;
            args[index6] = (object)local7;
            int index7 = 6;
            // ISSUE: variable of a boxed type
            var local8 = (object)arg6;
            args[index7] = (object)local8;
            int index8 = 7;
            // ISSUE: variable of a boxed type
            var local9 = (object)arg7;
            args[index8] = (object)local9;
            int index9 = 8;
            // ISSUE: variable of a boxed type
            var local10 = (object)arg8;
            args[index9] = (object)local10;
            int index10 = 9;
            // ISSUE: variable of a boxed type
            var local11 = (object)arg9;
            args[index10] = (object)local11;
            int index11 = 10;
            // ISSUE: variable of a boxed type
            var local12 = (object)arg10;
            args[index11] = (object)local12;
            int index12 = 11;
            // ISSUE: variable of a boxed type
            var local13 = (object)arg11;
            args[index12] = (object)local13;
            int index13 = 12;
            // ISSUE: variable of a boxed type
            var local14 = (object)arg12;
            args[index13] = (object)local14;
            int index14 = 13;
            // ISSUE: variable of a boxed type
            var local15 = (object)arg13;
            args[index14] = (object)local15;
            int index15 = 14;
            // ISSUE: variable of a boxed type
            var local16 = (object)arg14;
            args[index15] = (object)local16;
            int index16 = 15;
            // ISSUE: variable of a boxed type
            var local17 = (object)arg15;
            args[index16] = (object)local17;
            return (TResult)this.ExecuteQuery((DataContext)local1, args);
        }

        private object ExecuteQuery(DataContext context, object[] args)
        {
            if (context == null)
                throw Error.ArgumentNull("context");
            if (this.compiled == null)
            {
                lock (this)
                {
                    if (this.compiled == null)
                    {
                        this.compiled = context.Provider.Compile((System.Linq.Expressions.Expression)this.query);
                        this.mappingSource = context.Mapping.MappingSource;
                    }
                }
            }
            else if (context.Mapping.MappingSource != this.mappingSource)
                throw Error.QueryWasCompiledForDifferentMappingSource();
            return this.compiled.Execute(context.Provider, args).ReturnValue;
        }
    }
}
