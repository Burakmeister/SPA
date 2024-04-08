using SPA.DesignEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.parser
{
    public interface ITreeBuilder
    {
        void CreateAssign();

        void CreateCall();

        void CreateConstant();

        void CreateExpr();

        void CreateExprPlus();

        void CreateFactor();

        void CreateProcedure(string[] statement);

        void CreateProgram();

        void CreateStatement();

        void CreateStatementList();

        void CreateVariable();

        void CreateWhile(Variable var, string[] statements);

    }
}
