using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Idefav.Utility;

namespace Idefav.IDAL
{
    public interface IDbObject
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        string DbType { get; }

        /// <summary>
        /// 链接字符串
        /// </summary>
        string DbConnectStr { get; set; }

        /// <summary>
        /// 数据库前缀
        /// </summary>
        string Perfix { get; }

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proc"></param>
        /// <returns></returns>
        T DbConnect<T>(Func<IDbConnection, T> proc);

        /// <summary>
        /// 执行sql语句,并返回影响的行数
        /// </summary>
        /// <param name="SQLString">SQL</param>
        /// <returns></returns>
        int ExecuteSql(string SQLString);

        /// <summary>
        /// 执行SQL,并支持事务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proc">执行逻辑</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        T DbExcute<T>(Func<IDbCommand, T> proc, IDbTransaction transaction = null);

        /// <summary>
        /// 执行SQL,并支持事务,返回受影响的行数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="transaction">事务</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        int ExceuteSql(string sql, IDbTransaction transaction = null, params KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="proc">处理逻辑</param>
        /// <returns></returns>
        bool ExceuteTrans(Func<IDbTransaction, bool> proc);

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="SQLString">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        DataSet Query(string SQLString,params KeyValuePair<string,object>[] parameters);

        /// <summary>
        /// 查询 并返回DataTable
        /// </summary>
        /// <param name="sql">Sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        DataTable QueryDataTable(string sql,params KeyValuePair<string,object>[] parameters);

        /// <summary>
        /// 使用DataReader查询
        /// </summary>
        /// <param name="sql">Sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        IDataReader QueryDataReader(string sql, params KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="sqlstr">查询语句</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="select">选择的字段</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string orderby, string select,
            params KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="sqlstr">sql语句</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="orderby">排序</param>
        /// <param name="direction">排序方向</param>
        /// <param name="select">选择的字段</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string orderby,OrderDirection direction, string select,
            params KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 分页查询 支持偏移分页
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="offset"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <param name="orderby"></param>
        /// <param name="direction"></param>
        /// <param name="select"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataTable QueryPageTableOffset(string sqlstr, int offset, int pageNo, int pageSize, out int count, string orderby,
            OrderDirection direction = OrderDirection.DESC, string select = "*",
            params KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 分页查询 并返回总记录数量
        /// </summary>
        /// <param name="sqlstr">SQL语句</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="count">总记录数量</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="select">选择的字段</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, out int count, string orderby, string select, params KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 分页查询 并返回总记录数量
        /// </summary>
        /// <param name="sqlstr">sql</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="count">总记录数量</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="direction">排序方向</param>
        /// <param name="select">选择的字段</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, out int count, string orderby,OrderDirection direction, string select, params KeyValuePair<string,object>[] parameters);

        /// <summary>
        /// 分页查询 带偏移
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="offset"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderby"></param>
        /// <param name="direction"></param>
        /// <param name="select"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataTable QueryPageTableOffset(string sqlstr, int offset, int pageNo, int pageSize, string orderby,
           OrderDirection direction = OrderDirection.DESC, string select = "*",
           params KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 获取参数名称,带各种数据库的前缀
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetParameterName(string name);

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        T QueryModel<T>(string sql, params KeyValuePair<string, object>[] parameters) where T : class, new();

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        List<T> QueryModels<T>(string sql, params KeyValuePair<string, object>[] parameters) where T : class, new();

        /// <summary>
        /// 执行sql查询语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        object ExecuteScalar(string sql, params KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 插入
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="model">模型</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        bool Insert<T>(T model, IDbTransaction transaction = null);

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="model">模型</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        bool Update<T>(T model, IDbTransaction transaction = null);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="fields">字段</param>
        /// <param name="where">条件</param>
        /// <param name="tableName">表名</param>
        /// <param name="transaction">事务</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        bool Update(string fields, string where, string tableName, IDbTransaction transaction = null,
            params KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="model">模型</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        bool Delete<T>(T model, IDbTransaction transaction = null);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="where">where条件</param>
        /// <param name="tableName">表名</param>
        /// <param name="transaction">事务</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        bool Delete(string where, string tableName, IDbTransaction transaction = null, params KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="model">模型</param>
        /// <param name="ignoreAutoIm">是否忽略自动增长字段</param>
        /// <returns></returns>
        bool IsExist<T>(T model, bool ignoreAutoIm = false);

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="where">where条件</param>
        /// <param name="tableName">表名</param>
        /// <param name="kv">参数</param>
        /// <returns></returns>
        bool IsExist(string where, string tableName, params KeyValuePair<string, object>[] kv);

        /// <summary>
        /// 参数组合
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        List<IDbDataParameter> MakeParams(KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 获取数据数据量
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数名称</param>
        /// <returns></returns>
        int GetCount(string sql, params KeyValuePair<string, object>[] parameters);
    }
}
