using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idefav.Utility
{
    public class ClassTableInfo
    {
        public ClassTableInfo(string parameterPerfix)
        {
            ParameterPerfix = parameterPerfix;
            AutoIncreFields=new List<KeyValuePair<string, object>>();
            Fields=new List<KeyValuePair<string, object>>();
            PrimaryKeys=new List<KeyValuePair<string, object>>();
        }

        public string ParameterPerfix { get; set; }

        public object[] ClassAttributes { get; set; }

        public List<KeyValuePair<string,object>> AutoIncreFields { get; set; }

        public string TableName { get; set; }

        public List<KeyValuePair<string, object>> Fields { get; set; }
        public List<KeyValuePair<string, object>> PrimaryKeys { get; set; }
        public string SqlSelect
        {
            get
            {
                string sql = "";
                sql += " select * from " + TableName;
                return sql;
            }
        }

        public string GetParameterPerfix(string key)
        {
            return string.Format("{0}{1}", ParameterPerfix, key);
        }

        public string SqlInsert
        {
            get
            {
                string sql = "";
                sql += string.Format("insert {0} ", TableName);
                string fieldstr = string.Join(",", Fields.Select(k => k.Key));
                string valuestr = string.Join(",", Fields.Select(k => GetParameterPerfix(k.Key)));
                sql += string.Format(" ({0}) values ({1}) ", fieldstr, valuestr);
                var parm = Fields.Select(k => new KeyValuePair<string, object>(GetParameterPerfix(k.Key), k.Value));
                return "";
            }
        }

        public string SqlDelete
        {
            get
            {
                string sql = "";
                if (PrimaryKeys.Count > 0)
                {

                    sql += string.Format("delete from {0}", TableName);
                    sql += " where ";
                    string fieldstr = string.Join(" AND ",
                        PrimaryKeys.Select(k => k.Key + "=" + GetParameterPerfix(k.Key)));
                    sql += fieldstr;

                }
                return sql;

            }
        }

        public string SqlUpdate
        {
            get
            {
                string sql = "";
                sql += string.Format("update {0}", TableName);
                sql += " set ";
                
                return "";
            }
        }
    }
}
