using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    internal class UsesS : Relation
    {
        public StmtRef StmtRef { get; set; }
        public EntRef EntRef { get; set; }

        public override string FirstElement()
        {
            return StmtRef.Value;
        }

        public override string SecondElement()
        {
            return EntRef.Value;
        }
    }
}
