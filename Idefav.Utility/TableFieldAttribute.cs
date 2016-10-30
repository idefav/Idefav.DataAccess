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

        private bool _include = true;

        public bool Include
        {
            get { return _include; }
            set { _include = value; }
        }

        public TableFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

        public TableFieldAttribute(string fieldname, bool include)
        {
            FieldName = fieldname;
            Include = include;
        }

        public TableFieldAttribute(string fieldName, string fieldDescription)
        {
            FieldName = fieldName;
            FieldDescription = fieldDescription;
        }

        public TableFieldAttribute(string fieldname, string fielddescriptioin,bool include)
        {
            FieldName = fieldname;
            FieldDescription = fielddescriptioin;
            Include = include;
        }
    }
}
