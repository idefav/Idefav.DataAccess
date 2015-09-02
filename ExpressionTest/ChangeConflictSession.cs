using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal sealed class ChangeConflictSession
    {
        private DataContext context;
        private DataContext refreshContext;

        internal DataContext Context
        {
            get
            {
                return this.context;
            }
        }

        internal DataContext RefreshContext
        {
            get
            {
                if (this.refreshContext == null)
                    this.refreshContext = this.context.CreateRefreshContext();
                return this.refreshContext;
            }
        }

        internal ChangeConflictSession(DataContext context)
        {
            this.context = context;
        }
    }
}
