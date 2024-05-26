using SPA.DesignEntities;
using SPA.Exceptions;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace SPA.PKB
{
    public class Pkb : IPkb
    {
        public Program? program { get; set; } = null;

        private static Pkb instance;
        private List<string> variables;
        private List<string> procedures;
        private List<int> whiles;
        private List<int> assigns;
        private List<int> constants;
        private List<int> statements;

        private int[] follows;
        private List<int>[] parents;
        private List<int>[] uses;
        private List<int>[] modifies;

        public int programLength;

        // konstruktor zapobiegający kolejnym instancjom
        private Pkb(int statementCount)
        {
            programLength = statementCount; 

            variables = new ();
            procedures = new ();
            whiles = new ();
            assigns = new ();
            constants = new ();

            modifies = new List<int>[statementCount];
            InitializeLists(modifies);
            follows = new int[statementCount];
            InitializeArrayWithValue(follows, -1);
            parents = new List<int>[statementCount];
            InitializeLists(parents);
            uses = new List<int>[statementCount];
            InitializeLists(uses);

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
            else
            {
                instance.ClearPkb(statementcount);
            }          
            return instance;
        }

        public void SetFollows(Statement firstStatement, Statement nextStatement)
        {
            follows[firstStatement.LineNumber] = nextStatement.LineNumber;
        }

        public int GetFollowed(int statementNumber)
        {
            return follows[statementNumber] ;
        }

        public int GetFollows(int statementNumber)
        {
            int result = -1;

            for (int i = 0; i < follows.Length; i++)
            {
                if (follows[i] == statementNumber)
                {
                    result = i;
                }
            }
            return result;
        }

        public List<int> GetAllFollowed()
        {
            List<int> toRet = new();
            for (int i = 1; i < programLength; i++)
            {
                if (GetFollowed(i) != -1)
                    toRet.Add(i);
            }
            return toRet;
        }

        public List<int> GetAllFollows()
        {
             List<int> toRet = new();
            for (int i = 1; i < programLength; i++)
            {
                if (GetFollowed(i) != -1)
                    toRet.Add(GetFollowed(i));
            }
            return toRet;
        }

        public bool IsFollowed(int firstStatement, int nextStatement)
        {
            if (follows[firstStatement]==nextStatement)
            {
                return true;
            }
            return false;
        }

        public void SetModifies(int statement, string variable)
        {
            if (!modifies[statement].Contains(GetVariableIndex(variable)))
                modifies[statement].Add(GetVariableIndex(variable));
        }

        public List<string> GetModified(int statementNumber)
        {
            List<int> modifiedVariables = modifies[statementNumber];
            List<string> variables = new List<string>();

            foreach (int index in modifiedVariables)
            {
                string variableName = (string)this.variables[index];
                variables.Add(variableName);
            }
            return variables;
        }

        public List<int> GetModifies(string variable)
        {
            List<int> statements = new List<int>();
            int varIndex = GetVariableIndex(variable);
            for (int i = 0; i < modifies.Length; i++)
            {
                if (modifies[i].Contains(varIndex))
                {
                    statements.Add(i);
                }
            }
            return statements;
        }

        public List<int> GetAllStatementsThatModifieVariables()
        {
            List<int> statements = new List<int>();
            for (int i = 0; i < uses.Length; i++)
            {
                if (modifies[i].Count > 0)
                {
                    statements.Add(i);
                }
            }
            return statements;
        }

        public bool IsModified(string variable, int statement)
        {
            if (modifies[statement].Contains(GetVariableIndex(variable)))
            {
                return true;
            }
            return false;
        }

        public void SetUses(int statement, string variable)
        {
            if (!uses[statement].Contains(GetVariableIndex(variable)))
                uses[statement].Add(GetVariableIndex(variable));
        }

        public List<string> GetUsed(int statement)
        {
            List<int> usedVariables = uses[statement];
            List<string> variables = new List<string>();

            foreach (int index in usedVariables)
            {
                string variableName = (string)this.variables[index];
                variables.Add(variableName);
            }
            return variables;
        }

        public List<int> GetUses(string variable)
        {
            List<int> statements = new List<int>();
            int varIndex = GetVariableIndex(variable);
            for (int i = 0; i < uses.Length; i++)
            {
                if (uses[i].Contains(varIndex))
                {
                    statements.Add(i);
                }
            }
            return statements;
        }

        public List<int> GetAllStatementsThatUseVariables()
        {
            List<int> statements = new List<int>();
            for (int i = 0; i < uses.Length; i++)
            {
                if (uses[i].Count > 0)
                {
                    statements.Add(i);
                }
            }
            return statements;
        }

        public bool IsUsed(string variable, int statement)
        {
            if (uses[statement].Contains(GetVariableIndex(variable)))
            {
                return true;
            }
            return false;
        }

        public void SetParent(Statement parentStatement, Statement childStatement)
        {
            parents[parentStatement.LineNumber].Add(childStatement.LineNumber);
        }

        public List<int> GetChildren(int parentStatement)
        {
            return parents[parentStatement];
        }

        public int GetParent(int childStatement)
        {
            for (int i = 0; i < parents.Length; i++)
            {
                if (parents[i].Contains(childStatement))
                {
                    return i;
                }
            }

            return -1; //czy to jest dobrze?
        }

        public List<int> GetAllParents()
        {
            List<int> toRet = new();
            for (int i = 1; i < programLength; i++)
            {
                if (GetChildren(i).Count > 0)
                    toRet.Add(i);
            }
            return toRet;
        }
        public List<int> GetAllChildren()
        {
            List<int> toRet = new();
            for (int i = 1; i < programLength; i++)
            {
                if (GetChildren(i).Count > 0)
                    toRet.AddRange(GetChildren(i));
            }
            return toRet;
        }

        public bool IsParent(int parentStatement, int childStatement)
        {
            if (parents[parentStatement].Contains(childStatement))
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

        public int GetProceduresSize()
        {
            return procedures.Count;
        }

        public List<string> GetProcedures()
        {
            return procedures;
        }

        public int InsertVariable(string variableName)
        {
            if (!variables.Contains(variableName))
                variables.Add(variableName);
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

        public int GetVariablesSize()
        {
            return variables.Count;
        }

        public List<string> GetVariables()
        {
            return variables;
        }

        public int InsertAssign(int statementNumber)
        {
           if(!assigns.Contains(statementNumber))
                assigns.Add(statementNumber);
           return assigns.IndexOf(statementNumber);
        }

        public List<int> GetAssigns()
        {
            return assigns;
        }

        public int InsertWhile(int statementNumber)
        {
            if (!whiles.Contains(statementNumber))
                whiles.Add(statementNumber);
            return whiles.IndexOf(statementNumber);
        }

        public List<int> GetWhiles()
        {
            return whiles;
        }

        public int InsertConstant(int constant)
        {
            if(!constants.Contains(constant))
                constants.Add(constant);
            return constants.IndexOf(constant);
        }

        public List<int> GetConstants()
        {
            return constants;
        }

        public void ClearPkb(int statementCount)
        {
            programLength = statementCount;

            variables.Clear();
            procedures.Clear();
            whiles.Clear();
            assigns.Clear();
            constants.Clear();

            modifies = new List<int>[statementCount];
            InitializeLists(modifies);
            follows = new int[statementCount];
            InitializeArrayWithValue(follows, -1);
            parents = new List<int>[statementCount];
            InitializeLists(parents);
            uses = new List<int>[statementCount];
            InitializeLists(uses);
        }

        public int GetProgramLength()
        {
            return programLength;
        }

        public List<int> GetAllStatements()
        {
            throw new System.NotImplementedException();
        }

    }
}
