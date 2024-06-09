using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class If : Statement
    {
        public Variable Var { get; private set; }
        public StatementList? Then { get; set; }
        public StatementList? Else { get; set; }
        public If(int lineNumber, Variable var) : base(lineNumber)
        {
            this.Var = var;
        }
    }
}
