using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    internal class ParentT : Relation
    {
        public StmtRef StmtRef { get; set; }
        public StmtRef StmtRef2 { get; set; }
    }
}
