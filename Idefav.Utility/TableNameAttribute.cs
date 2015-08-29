using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idefav.Utility
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        public TableNameAttribute(string name)
        {
            Name = name;
        }

        public TableNameAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
