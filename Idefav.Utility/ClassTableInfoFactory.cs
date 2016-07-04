using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idefav.Utility
{
    public class ClassTableInfoFactory
    {
        public static ClassTableInfo CreateClassTableInfo<T>(T model, string perfix)
        {
            ClassTableInfo cti = new ClassTableInfo(perfix);

            cti.ClassAttributes = typeof (T).GetCustomAttributes(true);
            var tablenameAttribute =
                cti.ClassAttributes.SingleOrDefault(c => c is TableNameAttribute) as TableNameAttribute;
            cti.TableName =
                tablenameAttribute != null ? tablenameAttribute.Name : typeof (T).Name;


            foreach (var propertyInfo in typeof (T).GetProperties())
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
                    if (cti.PrimaryKeys.Count(c => c.Key == propertyInfo.Name) <= 0)
                    {
                        cti.PrimaryKeys.Add(new KeyValuePair<string, object>(propertyInfo.Name,
                            propertyInfo.GetValue(model, null)));
                    }
                }
                // 自动增长
                if (isAutoIncre)
                {
                    cti.AutoIncreFields.Add(new KeyValuePair<string, object>(propertyInfo.Name,
                        propertyInfo.GetValue(model, null)));
                    continue;
                }

                if (tableField != null)
                {
                    string tableFieldName = tableField != null && !string.IsNullOrEmpty(tableField.FieldName)
                        ? tableField.FieldName
                        : propertyInfo.Name;
                    object tableFieldValue = propertyInfo.GetValue(model, null);

                    KeyValuePair<string, object> kv = new KeyValuePair<string, object>(tableFieldName, tableFieldValue);
                    cti.Fields.Add(kv);

                }
                else
                {
                    continue;
                    
                }

                
            }
            return cti;
        }
    }
}
