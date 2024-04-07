using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class Procedure : TNode
    {
        private string ProcName { get;}
        private StatementList StatementList { get;}
        private Procedure? NextProcedure { get; set; }

        public Procedure(string procName, StatementList statementList)
        {
            ProcName = procName;
            StatementList = statementList;
        }
    }
}
