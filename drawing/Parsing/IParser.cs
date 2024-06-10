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
        Assign CreateAssign(ArrayList stringsList);
        Program CreateProgram();
        Program? GetProgram();
        While CreateWhile(ArrayList stringsList, Procedure procedure);
        int Parse(string code);
        Procedure CreateProcedure(ArrayList stringsList);
        StatementList CreateStatementList(ArrayList stringsList, Procedure procedure);
        If CreateIf(ArrayList stringsList, Variable variable, Procedure procedure);
    }
}
