using SPA.PKB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    public abstract class Relation
    {
        // Metody, które pozwolą uzyskać elementy relacji
        // bez względu na typ
        public abstract string FirstElement();
        public abstract string SecondElement();

    }
}
