using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class While : Statement
    {
        public Variable Var{ get; private set; }
        public StatementList? StatementList { get; set; }
        public While(int lineNumber, Variable var) : base(lineNumber)
        {
            this.Var = var;
        }
    }
}
