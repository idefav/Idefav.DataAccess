using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Idefav.Utility
{
    public class Common
    {
        /// <summary>  
        ///   
        /// 将对象属性转换为key-value对  
        /// </summary>  
        /// <param name="o"></param>  
        /// <returns></returns>  
        public static Dictionary<String, Object> ObjectToDictionary(Object o)
        {
            if (o == null) return null;

            Dictionary<String, Object> map = new Dictionary<string, object>();

            Type t = o.GetType();
            if (t == typeof(Dictionary<string, object>)) return o as Dictionary<string, object>;
            if (typeof (IEnumerable<KeyValuePair<string, object>>).IsAssignableFrom(t))
            {
                var op = o as IEnumerable<KeyValuePair<string, object>>;
                return op.ToDictionary(k => k.Key, v => v.Value);
            }
            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo p in pi)
            {
                MethodInfo mi = p.GetGetMethod();
                if (mi != null && mi.IsPublic)
                {
                    var v = mi.Invoke(o, null);
                    map.Add(p.Name, v ?? DBNull.Value);
                }
            }

            return map;

        }

        public static Dictionary<string, object> ListKvToDictionary(params KeyValuePair<string, object>[] kv)
        {
            return kv.ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
        }
    }
}
