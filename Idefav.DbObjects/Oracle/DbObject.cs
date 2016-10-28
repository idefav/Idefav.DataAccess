using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Idefav.IDAL;
using Idefav.Utility;
using Oracle.DataAccess.Client;

namespace Idefav.DbObjects.Oracle
{
    public class DbObject 
    {
        public string DbType => DBType.Oracle.ToString();

        public DbObject() { }

        public DbObject(string connStr)
        {
            DbConnectStr = connStr;
        }
        public string DbConnectStr { get; set; }

        public string Perfix => ":";

        public T DbConnect<T>(Func<IDbConnection, T> proc)
        {
            using (OracleConnection connection = new OracleConnection(DbConnectStr))
            {
                connection.Open();
                return proc(connection);
            }
        }

        public int ExecuteSql(string SQLString)
        {
            return ExceuteSql(SQLString, null);
        }

        public T DbExcute<T>(Func<IDbCommand, T> proc, IDbTransaction transaction = null)
        {
            if (transaction != null)
            {
                using (OracleCommand cmd = (OracleCommand)transaction.Connection.CreateCommand())
                {
                    cmd.Transaction = (OracleTransaction)transaction;
                    return proc(cmd);
                }
            }
            else
            {
                return DbConnect(conn =>
                {
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn as OracleConnection;
                        return proc(cmd);
                    }
                });
            }
        }

        public int ExceuteSql(string sql, IDbTransaction transaction = null, params KeyValuePair<string, object>[] parameters)
        {
            return DbExcute(cmd =>
            {
                OracleCommand sqlcmd = cmd as OracleCommand;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = sql;

                if (parameters != null)
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                return sqlcmd.ExecuteNonQuery();
            }, transaction);
        }

        public int ExceuteSql(string sql, IDbTransaction transaction = null, object parameters = null)
        {
            return ExceuteSql(sql, transaction, Common.ObjectToDictionary(parameters).ToArray());
        }

        public bool ExceuteTrans(Func<IDbTransaction, bool> proc)
        {
            return DbConnect(conn =>
            {

                OracleTransaction transaction = (conn as OracleConnection).BeginTransaction();
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

        public DataSet Query(string SQLString, params KeyValuePair<string, object>[] parameters)
        {
            return DbExcute(cmd =>
            {
                OracleCommand sqlcmd = cmd as OracleCommand;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = SQLString;
                if (parameters != null)
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                OracleDataAdapter adp = new OracleDataAdapter(sqlcmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds;
            });
        }

        public DataSet Query(string SQLString, object parameters = null)
        {
            return Query(SQLString, Common.ObjectToDictionary(parameters).ToArray());
        }

        public DataTable QueryDataTable(string sql, params KeyValuePair<string, object>[] parameters)
        {
            return DbExcute(cmd =>
            {
                OracleCommand sqlcmd = cmd as OracleCommand;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = sql;
                if (parameters != null)
                    sqlcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                OracleDataAdapter adp = new OracleDataAdapter(sqlcmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds.Tables[0];
            });
        }

        public DataTable QueryDataTable(string sql, object parameters = null)
        {
            return QueryDataTable(sql, Common.ObjectToDictionary(parameters).ToArray());
        }

        public IDataReader QueryDataReader(string sql, params KeyValuePair<string, object>[] parameters)
        {
            OracleConnection connection = new OracleConnection(DbConnectStr);
            connection.Open();
            OracleCommand cmd = new OracleCommand();
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

        public IDataReader QueryDataReader(string sql, object parameters = null)
        {
            return QueryDataReader(sql, Common.ObjectToDictionary(parameters).ToArray());
        }

        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string @orderby, string @select,
            params KeyValuePair<string, object>[] parameters)
        {
            //int startIndex = (pageNo - 1) * pageSize + 1; //开始
            //int endIndex = pageNo * pageSize; //结束
            //StringBuilder sql = new StringBuilder();
            //sql.Append(string.Format(" SELECT * FROM "));
            //sql.Append(" (SELECT Z.*, ROWNUM RN FROM ({0}) Z ");
            //sql.Append(string.Format(" WHERE ROWNUM <= {0} ORDER BY {1}) ", endIndex,orderby));
            //sql.Append(string.Format(" WHERE RN >= {0} ORDER by RN DESC ", startIndex));
            //return DbExcute(cmd =>
            //{
            //    OracleCommand orcmd = cmd as OracleCommand;
            //    cmd.CommandType = CommandType.Text;
            //    cmd.CommandText = string.Format(sql.ToString(), sqlstr);
            //    if (parameters != null)
            //    {
            //        orcmd.Parameters.Clear();
            //        orcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
            //    }
            //    OracleDataAdapter adp = new OracleDataAdapter(orcmd);
            //    DataSet ds = new DataSet();
            //    adp.Fill(ds);
            //    return ds.Tables[0];
            //});
            return QueryPageTable(sqlstr, pageNo, pageSize, orderby, OrderDirection.ASC, select, parameters);
        }

        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string @orderby, string @select,
            object parameters = null)
        {
            return QueryPageTable(sqlstr, pageNo, pageSize, orderby, select,
                Common.ObjectToDictionary(parameters).ToArray());
        }

        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string @orderby, OrderDirection direction,
            string @select, params KeyValuePair<string, object>[] parameters)
        {
            int startIndex = (pageNo - 1) * pageSize + 1; //开始
            int endIndex = pageNo * pageSize; //结束
            StringBuilder sql = new StringBuilder();
            sql.Append(string.Format(" SELECT * FROM "));
            sql.Append(" (SELECT Z.*, ROWNUM RN FROM ({0}) Z ");
            sql.Append(string.Format(" WHERE ROWNUM <= {0} ORDER BY {1} {2}) ", endIndex,orderby,direction));
            sql.Append(string.Format(" WHERE RN >= {0} ORDER by RN DESC ", startIndex));
           
            return DbExcute(cmd =>
            {
                OracleCommand orcmd = cmd as OracleCommand;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = string.Format(sql.ToString(), sqlstr);
                if (parameters != null)
                {
                    orcmd.Parameters.Clear();
                    orcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
                }
                OracleDataAdapter adp = new OracleDataAdapter(orcmd);
                DataSet ds = new DataSet();
                adp.Fill(ds);
                return ds.Tables[0];
            });
        }

        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string @orderby, OrderDirection direction,
            string @select, object parameters = null)
        {
            return QueryPageTable(sqlstr, pageNo, pageSize, orderby, direction, select,
                Common.ObjectToDictionary(parameters).ToArray());
        }

        public DataTable QueryPageTableOffset(string sqlstr, int offset, int pageNo, int pageSize, out int count, string @orderby,
            OrderDirection direction = OrderDirection.DESC, string @select = "*", params KeyValuePair<string, object>[] parameters)
        {
            throw new NotImplementedException();
        }

        public DataTable QueryPageTableOffset(string sqlstr, int offset, int pageNo, int pageSize, out int count, string @orderby,
            OrderDirection direction = OrderDirection.DESC, string @select = "*", object parameters = null)
        {
            throw new NotImplementedException();
        }

        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, out int count, string @orderby, string @select,
            params KeyValuePair<string, object>[] parameters)
        {
            return QueryPageTable(sqlstr, pageNo, pageSize, out count, orderby, OrderDirection.ASC, select,
                parameters);
        }

        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, out int count, string @orderby, string @select,
            object parameters = null)
        {
            return QueryPageTable(sqlstr, pageNo, pageSize, out count, orderby, select,
                Common.ObjectToDictionary(parameters).ToArray());
        }

        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, out int count, string @orderby,
            OrderDirection direction, string @select, params KeyValuePair<string, object>[] parameters)
        {
            count = GetCount(sqlstr, parameters);
            return QueryPageTable(sqlstr, pageNo, pageSize, orderby, direction, select, parameters);
        }

        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, out int count, string @orderby,
            OrderDirection direction, string @select, object parameters = null)
        {
            return QueryPageTable(sqlstr, pageNo, pageSize, out count, orderby, direction, select,
                Common.ObjectToDictionary(parameters).ToArray());
        }

        public DataTable QueryPageTableOffset(string sqlstr, int offset, int pageNo, int pageSize, string @orderby,
            OrderDirection direction = OrderDirection.DESC, string @select = "*", params KeyValuePair<string, object>[] parameters)
        {
            throw new NotImplementedException();
        }

        public DataTable QueryPageTableOffset(string sqlstr, int offset, int pageNo, int pageSize, string @orderby,
            OrderDirection direction = OrderDirection.DESC, string @select = "*", object parameters = null)
        {
            throw new NotImplementedException();
        }

        public DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string @orderby, string @select, out int count,
            params KeyValuePair<string, object>[] parameters)
        {
            //int startIndex = (pageNo - 1) * pageSize + 1; //开始
            //int endIndex = pageNo * pageSize; //结束
            //StringBuilder sql = new StringBuilder();
            //sql.Append(string.Format(" SELECT * FROM "));
            //sql.Append(" (SELECT Z.*, ROWNUM RN FROM ({0}) Z ");
            //sql.Append(string.Format(" WHERE ROWNUM <= {0}) ", endIndex));
            //sql.Append(string.Format(" WHERE RN >= {0} ORDER by RN DESC ", startIndex));
            //count = GetCount(sqlstr, parameters);
            //return DbExcute(cmd =>
            //{
            //    OracleCommand orcmd = cmd as OracleCommand;
            //    cmd.CommandType = CommandType.Text;
            //    cmd.CommandText = string.Format(sql.ToString(), sqlstr);
            //    if (parameters != null)
            //    {
            //        orcmd.Parameters.Clear();
            //        orcmd.Parameters.AddRange(MakeParams(parameters).ToArray());
            //    }
            //    OracleDataAdapter adp = new OracleDataAdapter(orcmd);
            //    DataSet ds = new DataSet();
            //    adp.Fill(ds);
            //    return ds.Tables[0];
            //});

            return QueryPageTable(sqlstr, pageNo, pageSize,out count, orderby,OrderDirection.ASC, select, 
                parameters);
        }

        public string GetParameterName(string name)
        {
            return string.Format("{0}{1}", Perfix, name);
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

        public T QueryModel<T>(string sql, object parameters = null) where T : class, new()
        {
            return QueryModel<T>(sql, Common.ObjectToDictionary(parameters).ToArray());
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

        public List<T> QueryModels<T>(string sql, object parameters = null) where T : class, new()
        {
            return QueryModels<T>(sql, Common.ObjectToDictionary(parameters).ToArray());
        }

        public object ExecuteScalar(string sql, params KeyValuePair<string, object>[] parameters)
        {
            return DbExcute(cmd =>
            {
                OracleCommand sqlcmd = cmd as OracleCommand;
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

        public object ExecuteScalar(string sql, object parameters = null)
        {
            return ExecuteScalar(sql, Common.ObjectToDictionary(parameters).ToArray());
        }

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

        public bool Update(string fields, string @where, string tableName, IDbTransaction transaction = null,
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

        public bool Update(string fields, string @where, string tableName, IDbTransaction transaction = null, object parameters = null)
        {
            return Update(fields, where, tableName, transaction, Common.ObjectToDictionary(parameters).ToArray());
        }

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

        public bool Delete(string @where, string tableName, IDbTransaction transaction = null, object parameters = null)
        {
            return Delete(where, tableName, transaction, Common.ObjectToDictionary(parameters).ToArray());
        }

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

        public bool IsExist(string @where, string tableName, params KeyValuePair<string, object>[] kv)
        {
            string sql = string.Format(" select count(*) from {0} ", tableName);
            sql += where;
            return (int)ExecuteScalar(sql, kv) > 0;
        }

        public bool IsExist(string @where, string tableName, object kv = null)
        {
            return IsExist(where, tableName, Common.ObjectToDictionary(kv).ToArray());
        }

        public List<IDbDataParameter> MakeParams(KeyValuePair<string, object>[] parameters)
        {
            List<IDbDataParameter> listParameters = new List<IDbDataParameter>();
            if (parameters != null)
            {
                foreach (var kv in parameters)
                {
                    OracleParameter sqlParameter = new OracleParameter(kv.Key, kv.Value);
                    listParameters.Add(sqlParameter);
                }
            }
            return listParameters;
        }

        public int GetCount(string sql, params KeyValuePair<string, object>[] parameters)
        {
            string sqlcount = @"WITH TB1 as (" + sql + ") select COUNT(*) from TB1";
            return DbExcute(cmd =>
            {
                OracleCommand sqlcmd = cmd as OracleCommand;
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

        public int GetCount(string sql, object parameters = null)
        {
            return GetCount(sql, Common.ObjectToDictionary(parameters).ToArray());
        }
    }
}
