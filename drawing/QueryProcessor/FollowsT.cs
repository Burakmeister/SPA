using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    internal class FollowsT : Relation
    {
        public StmtRef leftStmtRef { get; set; }
        public StmtRef rightStmtRef { get; set; }

        public override string FirstElement()
        {
            return StmtRef.Value;
        }

        public override string SecondElement()
        {
            return StmtRef2.Value;
        }
    }
}
