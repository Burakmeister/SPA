using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public abstract class Statement
    {
        int lineNumber;

        Statement nextStatement;

        protected Statement(int lineNumber, Statement nextStatement)
        {
            this.lineNumber = lineNumber;
            this.nextStatement = nextStatement;
        }

        public int LineNumber { get; set; }

    }
}
