using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class Constant : Factor
    {
        private int Value { get; }
        public Constant(int value, int lineNumber) : base(lineNumber)
        {
            Value = value;
        }
    }
}
