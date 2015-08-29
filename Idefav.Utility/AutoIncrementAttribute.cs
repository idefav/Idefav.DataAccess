using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idefav.Utility
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AutoIncrementAttribute : Attribute
    {
        public string Description
        {
            get { return "数据库自动增长字段"; }
        }
    }
}
