

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
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

        public string DbType
        {
            get { return "SQLServer"; }
        }

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

        public int ExecuteSql(string SQLString)
        {
            return ExceuteSql(SQLString, null);
        }

        public int ExceuteSql(string sql, IDbTransaction transaction = null, params KeyValuePair<string, object>[] parameters)
        {
            return DbExcute(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                if (parameters != null)
                    cmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                return cmd.ExecuteNonQuery();
            }, transaction);
        }

        public bool ExceuteTrans(Func<IDbTransaction, bool> proc)
        {
            return DbConnect(conn =>
            {
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    proc(transaction);
                    transaction.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return false;
                }

            });
        }

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proc"></param>
        /// <returns></returns>
        public T DbConnect<T>(Func<SqlConnection, T> proc)
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
        public T DbExcute<T>(Func<SqlCommand, T> proc, IDbTransaction transaction = null)
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
                        cmd.Connection = conn;
                        return proc(cmd);
                    }
                });
            }

        }

        public DataSet Query(string SQLString, params KeyValuePair<string, object>[] parameters)
        {
            return DbExcute(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SQLString;
                if (parameters != null)
                    cmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                SqlDataAdapter adp = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds;
            });
        }

        public DataTable QueryDataTable(string sql, params KeyValuePair<string, object>[] parameters)
        {
            return DbExcute(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                if (parameters != null)
                    cmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                SqlDataAdapter adp = new SqlDataAdapter(cmd);
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
                            pi.SetValue(model, dr.GetValue(i), null);
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
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                if (parameters != null)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                }
                return cmd.ExecuteScalar();
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
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string orderby, string select,
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
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = qSql.ToString();
                if (parameters != null)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                }
                SqlDataAdapter adp = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds.Tables[0];
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
            try
            {
                var classAttribute = typeof(T).GetCustomAttributes(true);
                var tablenameAttribute = classAttribute.SingleOrDefault(c => c is TableNameAttribute) as TableNameAttribute;
                string table =
                    tablenameAttribute != null ? tablenameAttribute.Name : typeof(T).Name;
                string sql = string.Format(" insert {0} ", table);
                List<KeyValuePair<string, object>> fields = new List<KeyValuePair<string, object>>();
                foreach (var propertyInfo in typeof(T).GetProperties())
                {
                    var propertyAttributes = propertyInfo.GetCustomAttributes(true);

                    // 是否是数据库自动增长字段
                    var isAutoIncre = propertyAttributes.Count(c => c is AutoIncrementAttribute) > 0;
                    var tableField =
                        propertyAttributes.SingleOrDefault(c => c is TableFieldAttribute) as TableFieldAttribute;

                    // 是否是主键
                    var isPrimaryKey = propertyAttributes.Count(c => c is PrimaryKeyAttribute) > 0;
                    if (isPrimaryKey)
                    {
                        // 判断是否存在
                        if (primarykeys.Count(c => c.Key == propertyInfo.Name) <= 0)
                        {
                            primarykeys.Add(new KeyValuePair<string, object>(propertyInfo.Name, propertyInfo.GetValue(model, null)));
                        }
                    }
                    if (isAutoIncre)
                    {
                        continue;
                    }

                    string tableFieldName = tableField != null && !string.IsNullOrEmpty(tableField.FieldName) ? tableField.FieldName : propertyInfo.Name;
                    object tableFieldValue = propertyInfo.GetValue(model, null);

                    KeyValuePair<string, object> kv = new KeyValuePair<string, object>(tableFieldName, tableFieldValue);
                    fields.Add(kv);
                }
                // 判断记录是否存在
                if (!IsExist(model, true))
                {
                    string fieldstr = string.Join(",", fields.Select(k => k.Key));
                    string valuestr = string.Join(",", fields.Select(k => GetParameterName(k.Key)));
                    sql += string.Format(" ({0}) values ({1}) ", fieldstr, valuestr);
                    var parm = fields.Select(k => new KeyValuePair<string, object>(GetParameterName(k.Key), k.Value));
                    result = ExceuteSql(sql, transaction, parm.ToArray()) > 0;
                }

            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
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
            List<KeyValuePair<string, object>> primarykeys = new List<KeyValuePair<string, object>>();
            var classAttribute = typeof(T).GetCustomAttributes(true);
            var tablenameAttribute = classAttribute.SingleOrDefault(c => c is TableNameAttribute) as TableNameAttribute;
            string table =
                tablenameAttribute != null ? tablenameAttribute.Name : typeof(T).Name;
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var propertyAttributes = propertyInfo.GetCustomAttributes(true);

                // 是否是数据库自动增长字段
                var isAutoIncre = propertyAttributes.Count(c => c is AutoIncrementAttribute) > 0;
                var tableField =
                    propertyAttributes.SingleOrDefault(c => c is TableFieldAttribute) as TableFieldAttribute;

                // 是否是主键
                var isPrimaryKey = propertyAttributes.Count(c => c is PrimaryKeyAttribute) > 0;
                if (isAutoIncre && ignoreAutoIm)
                {
                    continue;
                }
                if (isPrimaryKey)
                {
                    // 判断是否存在
                    if (primarykeys.Count(c => c.Key == propertyInfo.Name) <= 0)
                    {
                        primarykeys.Add(new KeyValuePair<string, object>(propertyInfo.Name, propertyInfo.GetValue(model, null)));
                    }
                }

            }
            if (primarykeys.Count > 0)
            {
                string sql = string.Format(" select count(*) from {0} ", table);
                sql += " where ";
                List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
                for (int i = 0; i < primarykeys.Count; i++)
                {
                    if (i == primarykeys.Count - 1)
                    {
                        sql += string.Format(" {0}={1} ", primarykeys[i].Key, GetParameterName(primarykeys[i].Key));
                    }
                    else
                    {
                        sql += string.Format(" {0}={1} AND ", primarykeys[i].Key, GetParameterName(primarykeys[i].Key));
                    }
                    parameters.Add(new KeyValuePair<string, object>(GetParameterName(primarykeys[i].Key), primarykeys[i].Value));
                }

                return ((int)ExecuteScalar(sql, parameters.ToArray())) > 0;
            }
            return false;
        }

        /// <summary>
        /// 获取数据库参数名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetParameterName(string name)
        {
            return string.Format("@{0}", name);
        }

        /// <summary>
        /// 参数装换
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private List<SqlParameter> MakeParams(KeyValuePair<string, object>[] parameters)
        {
            List<SqlParameter> listParameters = new List<SqlParameter>();
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
