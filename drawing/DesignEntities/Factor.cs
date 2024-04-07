using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public abstract class Factor : Expr
    {
        protected Factor(int lineNumber) : base(lineNumber)
        {
        }
    }
}
