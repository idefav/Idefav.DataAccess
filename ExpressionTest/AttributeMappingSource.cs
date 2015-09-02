using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 使用上下文中的属性创建映射模型的映射源。
    /// </summary>
    public sealed class AttributeMappingSource : MappingSource
    {
        protected override MetaModel CreateModel(Type dataContextType)
        {
            if (dataContextType == (Type)null)
                throw Error.ArgumentNull("dataContextType");
            return (MetaModel)new AttributedMetaModel((MappingSource)this, dataContextType);
        }
    }
}
