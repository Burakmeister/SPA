using SPA.DesignEntities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.Parsing
{
    public interface IParser
    {
        Assign createAssign();
        Program createProgram();
        While createWhile(ArrayList stringsList);
        void Parse(string code);
        Procedure createProcedure(ArrayList stringsList);
    }
}
