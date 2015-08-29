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
                if (transaction != null)
                    cmd.Transaction = (SqlTransaction)transaction;
                if (parameters != null)
                    cmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                return cmd.ExecuteNonQuery();
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

        public T DbExcute<T>(Func<SqlCommand, T> proc)
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
                    if (isAutoIncre && !isPrimaryKey)
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
                    string tableFieldName = tableField != null ? tableField.FieldName : propertyInfo.Name;
                    object tableFieldValue = propertyInfo.GetValue(model, null);

                    KeyValuePair<string, object> kv = new KeyValuePair<string, object>(tableFieldName, tableFieldValue);
                    fields.Add(kv);
                }
                string fieldstr = string.Join(",", fields.Select(k => k.Key));
                string valuestr = string.Join(",", fields.Select(k => GetParameterName(k.Key)));
                sql += string.Format(" ({0}) values ({1}) ", fieldstr, valuestr);
                var parm = fields.Select(k => new KeyValuePair<string, object>(GetParameterName(k.Key), k.Value));
                result = ExceuteSql(sql, transaction, parm.ToArray()) > 0;
            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
        }

        public string GetParameterName(string name)
        {
            return string.Format("@{0}", name);
        }

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
