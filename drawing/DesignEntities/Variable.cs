using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace SPA.DesignEntities
{
    public class Variable : Factor
    {
        public string VarName { get; private set; }

        public Variable(string name, int lineNumber) : base(lineNumber) { 
            VarName = name;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (!(obj is Variable)) return false;
            Variable v = (Variable)obj;
            if (v.VarName == VarName) return true;
            return false;
        }
    }
}
