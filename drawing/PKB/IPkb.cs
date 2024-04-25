using SPA.DesignEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.PKB
{
    public interface IPkb
    {
        void SetFollows(Statement firstStatement, Statement nextStatement);

        List<int> GetFollowed(Statement firstStatement);

        List<int> GetFollows(Statement nextStatement);

        bool IsFollowed(Statement firstStatement, Statement nextStatement);

        void SetUses(Statement statement, Variable variable);

        List<Variable> GetUsed(Statement statement);

        List<int> GetUses(Variable variable);

        bool IsUsed(Variable variable,Statement statement);

        void SetModifies(Statement statement, Variable variable);

        List<Variable> GetModified(Statement statement);

        List<int> GetModifies(Variable variable);

        bool IsModified(Variable variable, Statement statement);

        void SetParent(Statement parentStatement, Statement childStatement);

        List<int> GetChildren(Statement parentStatement);

        int GetParent(Statement childStatement);

        bool IsParent(Statement parentStatement, Statement childStatement);

        int InsertVariable(String variableName);

        String GetVariableName(int index);

        int GetVariableIndex(String VariableName);

        int GetVarTableSize();

        int InsertProcedure(String procedureName);

        int GetProcTableSize();

        String GetProcedureName(int index);

        int GetProcedureIndex(String ProcedureName);

        void ClearPkb();
    }
}
