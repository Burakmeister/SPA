using SPA.DesignEntities;
using SPA.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.PKB
{
    public class Pkb : IPkb
    {
        public Program? program { get; set; } = null;

        private static Pkb instance;
        private string[] varTable;
        private string[] procTable;

        int firstEmptyProcTableIndex;
        int firstEmptyVarTableIndex;

        private int[] follows;
        private List<int>[] parents;
        private List<int>[] uses;
        private int[] modifies;

        // konstruktor zapobiegający kolejnym instancjom
        private Pkb(int statementCount)
        {
            varTable = new string[100];
            procTable = new string[50];

            modifies = new int[statementCount];
            follows = new int[statementCount];
            parents = new List<int>[statementCount];
            uses = new List<int>[statementCount];

            firstEmptyProcTableIndex = 0;
            firstEmptyVarTableIndex = 0;
        }

        public static Pkb GetInstance(int statementcount)
        {
            if (instance == null)
            {
                instance = new Pkb(statementcount);
            }
            return instance;
        }

        public void SetFollows(Statement firstStatement, Statement nextStatement)
        {
            follows[firstStatement.LineNumber] = nextStatement.LineNumber;
        }

        public List<int> GetFollowed(Statement firstStatement)
        {
            return new List<int>() { follows[firstStatement.LineNumber] };
        }

        public List<int> GetFollows(Statement nextStatement)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < follows.Length; i++)
            {
                if (follows[i] == nextStatement.LineNumber)
                {
                    result.Add(i);
                }
            }
            return result;
        }

        public bool IsFollowed(Statement firstStatement, Statement nextStatement)
        {
            if (follows[firstStatement.LineNumber]==nextStatement.LineNumber)
            {
                return true;
            }
            return false;
        }

        public void SetModifies(Statement statement, Variable variable)
        {
            modifies[statement.LineNumber] = GetVariableIndex(variable.VarName);
        }

        public List<Variable> GetModified(Statement statement)
        {
            int variableIndex = modifies[statement.LineNumber];
            string variableName = varTable[variableIndex];
            Variable var = new Variable(variableName,0);
            return new List<Variable>() { var };
        }

        public List<int> GetModifies(Variable variable)
        {
            List<int> result = new List<int>();
            int varIndex = GetVariableIndex(variable.VarName);
            for (int i = 0; i < modifies.Length; i++)
            {
                if (modifies[i] == varIndex)
                {
                    result.Add(i);
                }
            }
            return result;
        }

        public bool IsModified(Variable variable, Statement statement)
        {
            if (modifies[statement.LineNumber] == GetVariableIndex(variable.VarName))
            {
                return true;
            }
            return false;
        }

        public void SetUses(Statement statement, Variable variable)
        {
            uses[statement.LineNumber].Add(GetVariableIndex(variable.VarName));
        }

        public List<Variable> GetUsed(Statement statement)
        {
            List<int> usedVariables = uses[statement.LineNumber];
            List<Variable> variables = new List<Variable>();

            foreach (int index in usedVariables)
            {
                string variableName = varTable[index];
                Variable var = new Variable(variableName, 0);
                variables.Add(var);
            }
            return variables;
        }

        public List<int> GetUses(Variable variable)
        {
            List<int> statements = new List<int>();
            int varIndex = GetVariableIndex(variable.VarName);
            for (int i = 0; i < uses.Length; i++)
            {
                if (uses[i].Contains(varIndex))
                {
                    statements.Add(i);
                }
            }
            return statements;
        }

        public bool IsUsed(Variable variable, Statement statement)
        {
            if (uses[statement.LineNumber].Contains(GetVariableIndex(variable.VarName)))
            {
                return true;
            }
            return false;
        }

        public void SetParent(Statement parentStatement, Statement childStatement)
        {
            parents[parentStatement.LineNumber].Add(childStatement.LineNumber);
        }

        public List<int> GetChildren(Statement parentStatement)
        {
            return parents[parentStatement.LineNumber];
        }

        public int GetParent(Statement childStatement)
        {
            for (int i = 0; i < parents.Length; i++)
            {
                if (parents[i].Contains(childStatement.LineNumber))
                {
                    return i;
                }
            }

            return -1; //czy to jest dobrze?
        }

        public bool IsParent(Statement parentStatement, Statement childStatement)
        {
            if (parents[parentStatement.LineNumber].Contains(childStatement.LineNumber))
            {
                return true;
            }

            return false;
        }

        public int InsertProcedure(string procedureName)
        {
            int index = 0;
            while (true)
            {
                if (firstEmptyProcTableIndex == index)
                {
                    procTable[firstEmptyProcTableIndex] = procedureName;
                    firstEmptyProcTableIndex++;
                    break;
                }
                else
                {
                    if (procTable[index] == procedureName)
                    {
                        break;
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            return index;
        }

        public int GetProcedureIndex(string ProcedureName)
        {
            int index = 0;
            while (procTable[index] != ProcedureName && index < procTable.Length)
            {
                index++;
            }
            if (index < procTable.Length)
            {
                return index;
            }
            else
            {
                return -1; // variable name not in varTable
            }
        }

        public string GetProcedureName(int index)
        {
            if (index >= 0 && index < procTable.Length)
            {
                return procTable[index];
            }
            else
            {
                throw new Exception();
            }
        }

        public int GetProcTableSize()
        {
            return procTable.Length;
        }

        public int InsertVariable(string variableName)
        {
            int index = 0;
            while (true)
            {
                if (firstEmptyVarTableIndex == index)
                {
                    varTable[firstEmptyVarTableIndex] = variableName;
                    firstEmptyVarTableIndex++;
                    break;
                }
                else
                {
                    if (varTable[index] == variableName)
                    {
                        break;
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            return index;
        }

        public int GetVariableIndex(string VariableName)
        {
            int index = 0;
            while (varTable[index]!=VariableName && index<varTable.Length) {
                index++;
            }
            if (index<varTable.Length)
            {
                return index;
            }
            else
            {
                return -1; // variable name not in varTable
            }
        }

        public string GetVariableName(int index)
        {
            if (index>=0 && index<varTable.Length)
            {
                return varTable[index];
            }
            else
            {
                OutOfBoundsIndexException e = new OutOfBoundsIndexException();
                e.Index = index;
                throw e;
            }
        }

        public int GetVarTableSize()
        {
            return varTable.Length;
        }

    }
}
