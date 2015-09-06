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
        string DbType { get; }

        string DbConnectStr { get; set; }
        string Perfix { get; }
        T DbConnect<T>(Func<IDbConnection, T> proc);
        int ExecuteSql(string SQLString);
        T DbExcute<T>(Func<IDbCommand, T> proc, IDbTransaction transaction = null);
        int ExceuteSql(string sql, IDbTransaction transaction = null, params KeyValuePair<string, object>[] parameters);
        bool ExceuteTrans(Func<IDbTransaction, bool> proc);
        DataSet Query(string SQLString,params KeyValuePair<string,object>[] parameters);

        DataTable QueryDataTable(string sql,params KeyValuePair<string,object>[] parameters);
        IDataReader QueryDataReader(string sql, params KeyValuePair<string, object>[] parameters);

        DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string orderby, string select,
            params KeyValuePair<string, object>[] parameters);
        DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string orderby, string select, out int count, params KeyValuePair<string,object>[] parameters);

        string GetParameterName(string name);
        T QueryModel<T>(string sql, params KeyValuePair<string, object>[] parameters) where T : class, new();

        List<T> QueryModels<T>(string sql, params KeyValuePair<string, object>[] parameters) where T : class, new();

        object ExecuteScalar(string sql, params KeyValuePair<string, object>[] parameters);

        bool Insert<T>(T model, IDbTransaction transaction = null);

        bool Update<T>(T model, IDbTransaction transaction = null);

        bool Update(string fields, string where, string tableName, IDbTransaction transaction = null,
            params KeyValuePair<string, object>[] parameters);
        bool Delete<T>(T model, IDbTransaction transaction = null);
        bool Delete(string where, string tableName, IDbTransaction transaction = null, params KeyValuePair<string, object>[] parameters);
        bool IsExist<T>(T model, bool ignoreAutoIm = false);

        bool IsExist(string where, string tableName, params KeyValuePair<string, object>[] kv);
        List<IDbDataParameter> MakeParams(KeyValuePair<string, object>[] parameters);
        int GetCount(string sql, params KeyValuePair<string, object>[] parameters);
    }
}
