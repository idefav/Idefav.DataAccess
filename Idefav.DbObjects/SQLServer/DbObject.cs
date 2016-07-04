

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using Idefav.IDAL;
using Idefav.Utility;

namespace Idefav.DbObjects.SQLServer
{
    /// <summary>
    /// SQLServer操作类
    /// </summary>
    public class DbObject : IDbObject
    {

        private string _dbconnectStr;

        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DbType => DBType.SQLServer.ToString();



        public DbObject(string connStr)
        {
            DbConnectStr = connStr;
        }
        public string Perfix => "@";

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string DbConnectStr
        {
            get
            {
                return this._dbconnectStr;
            }
            set
            {
                this._dbconnectStr = value;
            }
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="SQLString"></param>
        /// <returns></returns>
        public int ExecuteSql(string SQLString)
        {
            return ExceuteSql(SQLString, null);
        }

        /// <summary>
        /// 执行SQL 支持事务提交
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="transaction"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExceuteSql(string sql, IDbTransaction transaction = null, params KeyValuePair<string, object>[] parameters)
        {
            return DbExcute(cmd =>
            {
                SqlCommand sqlcmd = cmd as SqlCommand;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = sql;

                if (parameters != null)
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                return sqlcmd.ExecuteNonQuery();
            }, transaction);
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        public bool ExceuteTrans(Func<IDbTransaction, bool> proc)
        {
            return DbConnect(conn =>
            {

                SqlTransaction transaction = (conn as SqlConnection).BeginTransaction();
                try
                {
                    bool result = proc(transaction);
                    transaction.Commit();
                    return result;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }

            });
        }

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proc"></param>
        /// <returns></returns>
        public T DbConnect<T>(Func<IDbConnection, T> proc)
        {
            using (SqlConnection connection = new SqlConnection(DbConnectStr))
            {
                connection.Open();
                return proc(connection);
            }
        }

        /// <summary>
        /// 执行Command
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proc"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public T DbExcute<T>(Func<IDbCommand, T> proc, IDbTransaction transaction = null)
        {
            if (transaction != null)
            {
                using (SqlCommand cmd = (SqlCommand)transaction.Connection.CreateCommand())
                {
                    cmd.Transaction = (SqlTransaction)transaction;
                    return proc(cmd);
                }
            }
            else
            {
                return DbConnect(conn =>
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn as SqlConnection;
                        return proc(cmd);
                    }
                });
            }

        }

        public DataSet Query(string SQLString, params KeyValuePair<string, object>[] parameters)
        {
            return DbExcute(cmd =>
            {
                SqlCommand sqlcmd = cmd as SqlCommand;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = SQLString;
                if (parameters != null)
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                SqlDataAdapter adp = new SqlDataAdapter(sqlcmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds;
            });
        }

        public DataTable QueryDataTable(string sql, params KeyValuePair<string, object>[] parameters)
        {
            return DbExcute(cmd =>
            {
                SqlCommand sqlcmd = cmd as SqlCommand;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = sql;
                if (parameters != null)
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                SqlDataAdapter adp = new SqlDataAdapter(sqlcmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds.Tables[0];
            });
        }

        public List<T> QueryModels<T>(string sql, params KeyValuePair<string, object>[] parameters) where T : class, new()
        {
            List<T> models = new List<T>();
            using (IDataReader dr = QueryDataReader(sql, parameters))
            {
                while (dr.Read())
                {
                    T model = new T();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        PropertyInfo pi = typeof(T).GetProperty(dr.GetName(i));
                        if (pi != null)
                        {
                            var v = dr.GetValue(i);
                            if (v == DBNull.Value)
                            {
                                pi.SetValue(model, null, null);
                            }
                            else
                            {
                                pi.SetValue(model, v, null);
                            }
                           
                        }
                    }
                    models.Add(model);
                }
            }
            return models;
        }

        public object ExecuteScalar(string sql, params KeyValuePair<string, object>[] parameters)
        {
            return DbExcute(cmd =>
            {
                SqlCommand sqlcmd = cmd as SqlCommand;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = sql;
                if (parameters != null)
                {
                    sqlcmd.Parameters.Clear();
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                }
                return sqlcmd.ExecuteScalar();
            });
        }

        /// <summary>
        /// 获取数据返回数据模型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T QueryModel<T>(string sql, params KeyValuePair<string, object>[] parameters) where T : class, new()
        {
            T model = new T();
            DataTable dt = QueryDataTable(sql, parameters);
            using (IDataReader dr = QueryDataReader(sql, parameters))
            {
                while (dr.Read())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        PropertyInfo pi = typeof(T).GetProperty(dr.GetName(i));
                        if (pi != null)
                        {
                            pi.SetValue(model, dr.GetValue(i), null);
                        }
                    }
                    break;
                }
            }
            return model;
        }

        /// <summary>
        /// 使用DataReader获取数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDataReader QueryDataReader(string sql, params KeyValuePair<string, object>[] parameters)
        {
            SqlConnection connection = new SqlConnection(DbConnectStr);
            connection.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = connection;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            if (parameters != null)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(MakeParams(parameters).ToArray());
            }
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="sqlstr">SQL语句</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="orderby">排序字段(如:ID,NAME DESC)</param>
        /// <param name="select">筛选</param>
        /// <param name="count"></param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize,out int count,  string orderby,  string select = "*",
            params KeyValuePair<string, object>[] parameters)
        {
            StringBuilder qSql = new StringBuilder();
            StringBuilder sql = new StringBuilder();
            int startIndex = (pageNo - 1) * pageSize + 1; //开始
            int endIndex = pageNo * pageSize;
            sql.Append(" WITH TB1 as (" + sqlstr + "), ");
            sql.Append(" TB2 as(SELECT ROW_NUMBER() OVER(ORDER BY " + orderby  + ") AS RN,* FROM TB1) ");
            sql.Append(" SELECT " + select + " FROM  TB2 ");
            sql.Append(" WHERE RN >= " + startIndex + " AND RN<= " + endIndex + " ");
            sql.Append(" ORDER BY " + orderby  + " ");

            count = GetCount(sqlstr, parameters);
            qSql.Append(string.Format(sql.ToString(), sqlstr));

            return DbExcute(cmd =>
            {
                SqlCommand sqlcmd = cmd as SqlCommand;
                sqlcmd.CommandType = CommandType.Text;

                sqlcmd.CommandText = qSql.ToString();
                if (parameters != null)
                {
                    sqlcmd.Parameters.Clear();
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                }
                SqlDataAdapter adp = new SqlDataAdapter(sqlcmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds.Tables[0];
            });
        }
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="sqlstr">SQL语句</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="orderby">排序字段(如:ID,NAME DESC)</param>
        /// <param name="direction">排序方向</param>
        /// <param name="select">筛选</param>
        /// <param name="count"></param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize,out int count, string orderby, OrderDirection direction = OrderDirection.DESC, string select="*",
            params KeyValuePair<string, object>[] parameters)
        {
            StringBuilder qSql = new StringBuilder();
            StringBuilder sql = new StringBuilder();
            int startIndex = (pageNo - 1) * pageSize + 1; //开始
            int endIndex = pageNo * pageSize;
            sql.Append(" WITH TB1 as (" + sqlstr + "), ");
            sql.Append(" TB2 as(SELECT ROW_NUMBER() OVER(ORDER BY " + orderby+" "+direction + ") AS RN,* FROM TB1) ");
            sql.Append(" SELECT " + select + " FROM  TB2 ");
            sql.Append(" WHERE RN >= " + startIndex + " AND RN<= " + endIndex + " ");
            sql.Append(" ORDER BY " + orderby+" "+direction + " ");

            count = GetCount(sqlstr, parameters);
            qSql.Append(string.Format(sql.ToString(), sqlstr));

            return DbExcute(cmd =>
            {
                SqlCommand sqlcmd = cmd as SqlCommand;
                sqlcmd.CommandType = CommandType.Text;

                sqlcmd.CommandText = qSql.ToString();
                if (parameters != null)
                {
                    sqlcmd.Parameters.Clear();
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                }
                SqlDataAdapter adp = new SqlDataAdapter(sqlcmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds.Tables[0];
            });
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="sqlstr">SQL语句</param>
        /// <param name="offset">偏移量</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="orderby">排序字段(如:ID,NAME DESC)</param>
        /// <param name="direction">排序方向</param>
        /// <param name="select">筛选</param>
        /// <param name="count"></param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public DataTable QueryPageTableOffset(string sqlstr,int offset, int pageNo, int pageSize, out int count, string orderby, OrderDirection direction = OrderDirection.DESC, string select = "*",
           params KeyValuePair<string, object>[] parameters)
        {
            StringBuilder qSql = new StringBuilder();
            StringBuilder sql = new StringBuilder();
            int startIndex = (pageNo - 1) * pageSize + 1; //开始
            int endIndex = pageNo * pageSize;
            sql.Append(" WITH TB1 as (" + sqlstr + "), ");
            sql.Append(" TB2 as(SELECT ROW_NUMBER() OVER(ORDER BY " + orderby + " " + direction + ") AS RN,* FROM TB1) ");
            sql.Append(" SELECT " + select + " FROM  TB2 ");
            sql.Append(" WHERE RN >= " + (startIndex+offset) + " AND RN<= " + (endIndex+offset) + " ");
            sql.Append(" ORDER BY " + orderby + " " + direction + " ");

            count = GetCount(sqlstr, parameters);
            qSql.Append(string.Format(sql.ToString(), sqlstr));

            return DbExcute(cmd =>
            {
                SqlCommand sqlcmd = cmd as SqlCommand;
                sqlcmd.CommandType = CommandType.Text;

                sqlcmd.CommandText = qSql.ToString();
                if (parameters != null)
                {
                    sqlcmd.Parameters.Clear();
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                }
                SqlDataAdapter adp = new SqlDataAdapter(sqlcmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds.Tables[0];
            });
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="sqlstr">SQL语句</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="orderby">排序字段(如:ID,NAME DESC)</param>
        /// <param name="select">筛选</param>
        /// <param name="count"></param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string orderby, string select="*",
            params KeyValuePair<string, object>[] parameters)
        {
            StringBuilder qSql = new StringBuilder();
            StringBuilder sql = new StringBuilder();
            int startIndex = (pageNo - 1) * pageSize + 1; //开始
            int endIndex = pageNo * pageSize;
            sql.Append(" WITH TB1 as (" + sqlstr + "), ");
            sql.Append(" TB2 as(SELECT ROW_NUMBER() OVER(ORDER BY " + orderby + ") AS RN,* FROM TB1) ");
            sql.Append(" SELECT " + select + " FROM  TB2 ");
            sql.Append(" WHERE RN >= " + startIndex + " AND RN<= " + endIndex + " ");
            sql.Append(" ORDER BY " + orderby + " ");

            
            qSql.Append(string.Format(sql.ToString(), sqlstr));

            return DbExcute(cmd =>
            {
                SqlCommand sqlcmd = cmd as SqlCommand;
                sqlcmd.CommandType = CommandType.Text;

                sqlcmd.CommandText = qSql.ToString();
                if (parameters != null)
                {
                    sqlcmd.Parameters.Clear();
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                }
                SqlDataAdapter adp = new SqlDataAdapter(sqlcmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds.Tables[0];
            });
        }

        

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="sqlstr">SQL语句</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="orderby">排序字段(如:ID,NAME DESC)</param>
        /// <param name="direction">排序方向默认DESC</param>
        /// <param name="select">筛选</param>
        /// <param name="count"></param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string orderby,OrderDirection direction=OrderDirection.DESC, string select = "*",
            params KeyValuePair<string, object>[] parameters)
        {
            StringBuilder qSql = new StringBuilder();
            StringBuilder sql = new StringBuilder();
            int startIndex = (pageNo - 1) * pageSize + 1; //开始
            int endIndex = pageNo * pageSize;
            sql.Append(" WITH TB1 as (" + sqlstr + "), ");
            sql.Append(" TB2 as(SELECT ROW_NUMBER() OVER(ORDER BY " + orderby+" "+direction + ") AS RN,* FROM TB1) ");
            sql.Append(" SELECT " + select + " FROM  TB2 ");
            sql.Append(" WHERE RN >= " + startIndex + " AND RN<= " + endIndex + " ");
            sql.Append(" ORDER BY " + orderby+" "+direction + " ");


            qSql.Append(string.Format(sql.ToString(), sqlstr));

            return DbExcute(cmd =>
            {
                SqlCommand sqlcmd = cmd as SqlCommand;
                sqlcmd.CommandType = CommandType.Text;

                sqlcmd.CommandText = qSql.ToString();
                if (parameters != null)
                {
                    sqlcmd.Parameters.Clear();
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                }
                SqlDataAdapter adp = new SqlDataAdapter(sqlcmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds.Tables[0];
            });
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="sqlstr">SQL语句</param>
        /// <param name="offset">偏移量</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="orderby">排序字段(如:ID,NAME DESC)</param>
        /// <param name="direction">排序方向默认DESC</param>
        /// <param name="select">筛选</param>
        /// <param name="count"></param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public DataTable QueryPageTableOffset(string sqlstr,int offset, int pageNo, int pageSize, string orderby, OrderDirection direction = OrderDirection.DESC, string select = "*",
            params KeyValuePair<string, object>[] parameters)
        {
            StringBuilder qSql = new StringBuilder();
            StringBuilder sql = new StringBuilder();
            int startIndex = (pageNo - 1) * pageSize + 1; //开始
            int endIndex = pageNo * pageSize;
            sql.Append(" WITH TB1 as (" + sqlstr + "), ");
            sql.Append(" TB2 as(SELECT ROW_NUMBER() OVER(ORDER BY " + orderby + " " + direction + ") AS RN,* FROM TB1) ");
            sql.Append(" SELECT " + select + " FROM  TB2 ");
            sql.Append(" WHERE RN >= " + (startIndex+offset) + " AND RN<= " + (endIndex+offset) + " ");
            sql.Append(" ORDER BY " + orderby + " " + direction + " ");


            qSql.Append(string.Format(sql.ToString(), sqlstr));

            return DbExcute(cmd =>
            {
                SqlCommand sqlcmd = cmd as SqlCommand;
                sqlcmd.CommandType = CommandType.Text;

                sqlcmd.CommandText = qSql.ToString();
                if (parameters != null)
                {
                    sqlcmd.Parameters.Clear();
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                }
                SqlDataAdapter adp = new SqlDataAdapter(sqlcmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds.Tables[0];
            });
        }


        public int GetCount(string sql, params KeyValuePair<string, object>[] parameters)
        {
            string sqlcount = @"WITH TB1 as (" + sql + ") select COUNT(*) from TB1";
            return DbExcute(cmd =>
            {
                SqlCommand sqlcmd = cmd as SqlCommand;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = sqlcount;
                if (parameters != null)
                {
                    sqlcmd.Parameters.Clear();
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                }
                return int.Parse(sqlcmd.ExecuteScalar().ToString());
            });
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public bool Insert<T>(T model, IDbTransaction transaction = null)
        {
            bool result = false;
            List<KeyValuePair<string, object>> primarykeys = new List<KeyValuePair<string, object>>();


            ClassTableInfo cti = ClassTableInfoFactory.CreateClassTableInfo(model, Perfix);
            string sql = string.Format(" insert {0} ", cti.TableName);

            // 判断记录是否存在
            if (!IsExist(model, true))
            {
                string fieldstr = string.Join(",", cti.Fields.Select(k => k.Key));
                string valuestr = string.Join(",", cti.Fields.Select(k => GetParameterName(k.Key)));
                sql += string.Format(" ({0}) values ({1}) ", fieldstr, valuestr);
                var parm = cti.Fields.Select(k => new KeyValuePair<string, object>(GetParameterName(k.Key), k.Value));
                result = ExceuteSql(sql, transaction, parm.ToArray()) > 0;
            }

            return result;
        }

        public bool Insert<T>(T model, string where, IDbTransaction transaction = null,
            params KeyValuePair<string, object>[] parameters)
        {
            ClassTableInfo cti = ClassTableInfoFactory.CreateClassTableInfo(model, Perfix);
            string sql = string.Format(" insert {0} ", cti.TableName);
            if (!IsExist(where, cti.TableName, parameters))
            {
                string fieldstr = string.Join(",", cti.Fields.Select(k => k.Key));
                string valuestr = string.Join(",", cti.Fields.Select(k => GetParameterName(k.Key)));
                sql += string.Format(" ({0}) values ({1}) ", fieldstr, valuestr);
                var parm = cti.Fields.Select(k => new KeyValuePair<string, object>(GetParameterName(k.Key), k.Value));
                return ExceuteSql(sql, transaction, parm.ToArray()) > 0;
            }
            return false;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public bool Update<T>(T model, IDbTransaction transaction = null)
        {
            ClassTableInfo cti = ClassTableInfoFactory.CreateClassTableInfo(model, Perfix);
            if (cti.PrimaryKeys.Count > 0)
            {
                string sql = "";
                sql += "update " + cti.TableName;
                sql += " set ";
                string fields = string.Join(",", cti.Fields.Select(k => k.Key + "=" + GetParameterName(k.Key)));
                var param =
                    cti.Fields.Select(k => new KeyValuePair<string, object>(GetParameterName(k.Key), k.Value)).ToList();
                sql += fields;
                string where = string.Join(" AND ", cti.PrimaryKeys.Select(k => k.Key + "=" + GetParameterName(k.Key)));
                var wherep =
                    cti.PrimaryKeys.Select(k => new KeyValuePair<string, object>(GetParameterName(k.Key), k.Value))
                        .ToArray();
                sql += " where ";
                sql += where;
                param.AddRange(wherep);
                return ExceuteSql(sql, transaction, param.ToArray()) > 0;
            }
            return false;
        }

        public bool Update(string fields, string where, string tableName, IDbTransaction transaction = null,
            params KeyValuePair<string, object>[] parameters)
        {

            string sql = "";
            sql += "update " + tableName;
            sql += " set ";
            if (string.IsNullOrEmpty(fields))
            {
                return false;
            }
            sql += fields;
            if (string.IsNullOrEmpty(where))
            {
                return false;

            }

            sql += " " + where;

            return ExceuteSql(sql, transaction, parameters) > 0;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public bool Delete<T>(T model, IDbTransaction transaction = null)
        {
            ClassTableInfo cti = ClassTableInfoFactory.CreateClassTableInfo(model, Perfix);
            if (cti.PrimaryKeys.Count > 0)
            {
                string sql = "delete from " + cti.TableName;
                sql += " where ";
                string where = string.Join(" AND ", cti.PrimaryKeys.Select(k => k.Key + "=" + GetParameterName(k.Key)));
                var parm =
                    cti.PrimaryKeys.Select(k => new KeyValuePair<string, object>(GetParameterName(k.Key), k.Value))
                        .ToArray();
                sql += where;
                return ExceuteSql(sql, transaction, parm.ToArray()) > 0;
            }
            return false;

        }

        public bool Delete(string where, string tableName, IDbTransaction transaction = null, params KeyValuePair<string, object>[] parameters)
        {
            string sql = "delete from " + tableName + " ";
            sql += " " + where + " ";
            return ExceuteSql(sql, transaction, parameters) > 0;
        }

        /// <summary>
        /// 判断是否插入的记录是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="ignoreAutoIm">忽略自动增长主键</param>
        /// <returns></returns>
        public bool IsExist<T>(T model, bool ignoreAutoIm = false)
        {
            ClassTableInfo cti = ClassTableInfoFactory.CreateClassTableInfo(model, Perfix);
            if (cti.PrimaryKeys.Count > 0)
            {
                string sql = string.Format(" select count(*) from {0} ", cti.TableName);
                sql += " where ";
                var filterkeys = cti.PrimaryKeys;
                if (ignoreAutoIm)
                {
                    filterkeys =
                        cti.PrimaryKeys.Where(k => cti.AutoIncreFields.Count(c => c.Key == k.Key) <= 0).ToList();
                }
                if (filterkeys.Count > 0)
                {
                    List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
                    string where = string.Join(" AND ", filterkeys.Select(k => k.Key + "=" + GetParameterName(k.Key)));
                    parameters =
                            filterkeys.Select(k => new KeyValuePair<string, object>(GetParameterName(k.Key), k.Value))
                                .ToList();
                    sql += where;

                    return ((int)ExecuteScalar(sql, parameters.ToArray())) > 0;
                }
                else
                {
                    return false;
                }

            }
            return false;
        }

        /// <summary>
        /// 判断记录是否存在
        /// </summary>
        /// <param name="where"></param>
        /// <param name="tableName"></param>
        /// <param name="kv"></param>
        /// <returns></returns>
        public bool IsExist(string where, string tableName, params KeyValuePair<string, object>[] kv)
        {
            string sql = string.Format(" select count(*) from {0} ", tableName);
            sql += where;
            return (int)ExecuteScalar(sql, kv) > 0;

        }

        /// <summary>
        /// 获取数据库参数名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetParameterName(string name)
        {
            return string.Format("{1}{0}", name, Perfix);
        }

        /// <summary>
        /// 参数装换
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<IDbDataParameter> MakeParams(KeyValuePair<string, object>[] parameters)
        {
            List<IDbDataParameter> listParameters = new List<IDbDataParameter>();
            if (parameters != null)
            {
                foreach (var kv in parameters)
                {
                    SqlParameter sqlParameter = new SqlParameter(kv.Key, kv.Value);
                    listParameters.Add(sqlParameter);
                }
            }
            return listParameters;
        }

        public DbObject()
        {

        }
    }


}
