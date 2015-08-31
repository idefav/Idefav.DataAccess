using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal abstract class DbFormatter
    {
        internal abstract string Format(SqlNode node, bool isDebug);

        internal abstract string Format(SqlNode node);
    }
}
