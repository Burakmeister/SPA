using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class Variable : Factor
    {
        public string VarName { get; }

        public Variable(string name, int lineNumber) : base(lineNumber)
        {
            {
                VarName = name;
            }
        }
    }
}
