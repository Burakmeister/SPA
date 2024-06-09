using SPA.PKB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    internal class Follows : Relation
    {
        public StmtRef leftStmtRef { get; set; }
        public StmtRef rightStmtRef { get; set; }

        public override string FirstElement()
        {
            return leftStmtRef.Value;
        }

        public override string SecondElement()
        {
            return rightStmtRef.Value;
        }

    }
}
