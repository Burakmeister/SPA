using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class Constant : Factor
    {
        public int Value { get; private set; }
        public Constant(int value, int lineNumber) : base(lineNumber)
        {
            Value = value;
        }
    }
}
