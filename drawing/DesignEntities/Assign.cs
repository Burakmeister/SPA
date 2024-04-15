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
        public Assign(int lineNumber, string varName, Expr? expr) : base(lineNumber)
        {
            Expr = expr;
            Var = new Variable(varName, lineNumber);
        }
    }
}
