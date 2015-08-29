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

        int ExecuteSql(string SQLString);

        int ExceuteSql(string sql, IDbTransaction transaction = null, params KeyValuePair<string, object>[] parameters);

        DataSet Query(string SQLString,params KeyValuePair<string,object>[] parameters);

        DataTable QueryDataTable(string sql,params KeyValuePair<string,object>[] parameters);

        DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string orderby, string select,params KeyValuePair<string,object>[] parameters);

        string GetParameterName(string name);
        T QueryModel<T>(string sql, params KeyValuePair<string, object>[] parameters) where T : class, new();

        List<T> QueryModels<T>(string sql, params KeyValuePair<string, object>[] parameters) where T : class, new();

        object ExecuteScalar(string sql, params KeyValuePair<string, object>[] parameters);

        bool Insert<T>(T model, IDbTransaction transaction = null);
    }
}
