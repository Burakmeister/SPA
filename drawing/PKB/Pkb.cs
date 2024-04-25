using SPA.DesignEntities;
using SPA.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.PKB
{
    public class Pkb : IPkb
    {
        public Program? program { get; set; } = null;

        private static Pkb instance;
        private ArrayList variables;
        private ArrayList procedures;
        private ArrayList whiles;

        int firstEmptyProcTableIndex;
        int firstEmptyVarTableIndex;

        private int[] follows;
        private List<int>[] parents;
        private List<int>[] uses;
        private int[] modifies;

        // konstruktor zapobiegający kolejnym instancjom
        private Pkb(int statementCount)
        {
            variables = new ArrayList();
            procedures = new ArrayList();
            whiles = new ArrayList();

            modifies = new int[statementCount];
            InitializeArrayWithValue(modifies, -1);
            follows = new int[statementCount];
            InitializeArrayWithValue(follows, -1);
            parents = new List<int>[statementCount];
            InitializeLists(parents);
            uses = new List<int>[statementCount];
            InitializeLists(uses);

            firstEmptyProcTableIndex = 0;
            firstEmptyVarTableIndex = 0;
        }

        private void InitializeArrayWithValue(string[] array, string value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        private void InitializeArrayWithValue(int[] array, int value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        private void InitializeLists(List<int>[] lists)
        {
            for (int i = 0; i < lists.Length; i++)
            {
                lists[i] = new List<int>();
            }
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
            if (variableIndex >= 0)
            {
                string variableName = (string)variables[variableIndex];
                Variable var = new Variable(variableName, 0);
                return new List<Variable>() { var };
            }
            else
            {
                    throw new OutOfBoundsIndexException();
            }

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
                string variableName = (string)this.variables[index];
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
            if (!procedures.Contains(procedureName))
            {
                procedures.Add(procedureName);
            }
            return procedures.IndexOf(procedureName);
        }

        public int GetProcedureIndex(string ProcedureName)
        {
            if (procedures.Contains(ProcedureName))
            {
                return procedures.IndexOf(ProcedureName);
            }
            else
            {
                return -1; // procedure name not in procedures
            }
        }

        public string GetProcedureName(int index)
        {
            if (index >= 0 && index < procedures.Count)
            {
                return (string)procedures[index];
            }
            else
            {
                OutOfBoundsIndexException e = new OutOfBoundsIndexException();
                e.Index = index;
                throw e;
            }
        }

        public int GetProcTableSize()
        {
            return procedures.Count;
        }

        public int InsertVariable(string variableName)
        {
            if (!variables.Contains(variableName))
            {
                variables.Add(variableName);
            }
            return variables.IndexOf(variableName);
        }

        public int GetVariableIndex(string VariableName)
        {
            if (variables.Contains(VariableName))
            {
                return variables.IndexOf(VariableName);
            }
            else
            {
                return -1; // variable name not in varTable
            }
        }

        public string GetVariableName(int index)
        {
            if (index>=0 && index<variables.Count)
            {
                return (string)variables[index];
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
            return variables.Count;
        }

        public void ClearPkb()
        {
            InitializeArrayWithValue(varTable, "");
            InitializeArrayWithValue(procTable, "");
            InitializeArrayWithValue(modifies, -1);            
            InitializeArrayWithValue(follows, -1);
            InitializeLists(parents);
            InitializeLists(uses);

            firstEmptyProcTableIndex = 0;
            firstEmptyVarTableIndex = 0;
        }
    }
}
