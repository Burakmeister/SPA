using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class Assign : Statement
    {
        private Variable Var { get; }
        private Expr Expr { get; }
        public Assign(int lineNumber, Variable var, Expr expr) : base(lineNumber)
        {
            Var = var;
            Expr = expr;
        }
    }
}
