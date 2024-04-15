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
        Assign CreateAssign(int lineNumber, ArrayList stringsList);
        Program CreateProgram();
        Program? GetProgram();
        While CreateWhile(int lineNumber, ArrayList stringsList);
        int Parse(string code);
        Procedure CreateProcedure(int lineNumber, ArrayList stringsList);
    }
}
