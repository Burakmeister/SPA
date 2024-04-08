using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class Assign : Statement
    {
        public Variable Var { get; private set; }
        private Expr? Expr { get; set; }
        public Assign(int lineNumber, Variable var) : base(lineNumber)
        {
            Var = var;
        }
    }
}
