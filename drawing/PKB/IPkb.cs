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

        List<int> GetFollowed(int statementNumber);

        List<int> GetFollows(int statementNumber);

        bool IsFollowed(int statementNumber, int nextStatement);

        void SetUses(Statement statement, Variable variable);

        List<Variable> GetUsed(int statement);

        List<int> GetUses(string variable);

        bool IsUsed(string variable,int statement);

        void SetModifies(Statement statement, Variable variable);

        List<Variable> GetModified(int statementNumber);

        List<int> GetModifies(string variable);

        bool IsModified(string variable, int statement);

        void SetParent(Statement parentStatement, Statement childStatement);

        List<int> GetChildren(int parentStatement);

        int GetParent(int childStatement);

        bool IsParent(int parentStatement, int childStatement);

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
