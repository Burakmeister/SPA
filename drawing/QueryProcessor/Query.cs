using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    public class Query
    {
        public List<string> Result { get; set; } = null;
        public List<Declaration> Declarations { get; set; }
        public string Synonym { get; set; }
        public SuchThat? SuchThatClause { get; set; }
        public With? WithClause { get; set; }
    }
}
