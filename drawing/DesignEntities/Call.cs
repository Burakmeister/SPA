using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class Call : Statement
    {
        public Procedure? CallingProcedure { get; set; }
        public Procedure? CalledProcedure { get; set; }

        public Call(Procedure callingProcedure, int lineNumber) : base(lineNumber)
        {
            CallingProcedure = callingProcedure;
        }
    }
}
