using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public abstract class Statement : TNode
    {
        public int LineNumber { get; set; }
        protected Statement? NextStatement { get; set; }

        protected Statement(int lineNumber)
        {
            this.LineNumber = lineNumber;
        }
    }
}
