using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    internal class Query
    {
        public List<Declaration> Declarations { get; set; }
        public string Synonym { get; set; }
        public SuchThat SuchThatClause { get; set; }
        public With WithClause { get; set; }


        public Query()
        {
            
            
        }

        internal object Substring(object position)
        {
            throw new NotImplementedException();
        }
    }
}
