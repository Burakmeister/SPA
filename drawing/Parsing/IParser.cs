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
        void Parse(string input);
        Procedure createProcedure(ArrayList stringList);
        Program createProgram();
        While createWhile(Variable var, string[] code);
        ExprPlus createExprPlus(Expr left, Expr right);
        Assign createAssign(Variable var, Expr expr);
    }
}
