using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class While : Statement
    {
        private Variable Var{ get; }
        private StatementList StatementList { get; }
        public While(int lineNumber, Variable var, StatementList statementList) : base(lineNumber)
        {
            this.Var = var;
            this.StatementList = statementList;
        }
    }
}
