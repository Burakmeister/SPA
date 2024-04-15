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
        Assign CreateAssign(string varName);
        Program CreateProgram();
        Program? GetProgram();
        While CreateWhile(ArrayList stringsList);
        int Parse(string code);
        Procedure CreateProcedure(ArrayList stringsList);
    }
}
