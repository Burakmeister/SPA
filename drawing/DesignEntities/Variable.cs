using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.DesignEntities
{
    public class Variable : Factor
    {
        string Name;

        public Variable(string name)
        {
            Name = name;
        }
    }
}
