using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idefav.Utility
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TableFieldAttribute : Attribute
    {
        public string FieldName { get; set; }

        public string FieldDescription { get; set; }

        public TableFieldAttribute() { }

        public TableFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

        public TableFieldAttribute(string fieldName, string fieldDescription)
        {
            FieldName = fieldName;
            FieldDescription = fieldDescription;
        }
    }
}
