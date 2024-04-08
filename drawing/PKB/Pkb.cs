//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//
//using SPA.DesignEntities;
//
//namespace SPA.PKB
//{
//    public class Pkb
//    {
//        private static Pkb instance;
//        private List<string> varTable;
//        private List<string> procTable;
//        private Dictionary<Statement, Statement> followsTable;
//        private Dictionary<Statement, List<Statement>> parentTable;
//        private Dictionary<Statement, List<Variable>> usesTable;
//        private Dictionary<Statement, List<Variable>> modifiesTable;
//
//        //singleton konstruktor
//        private Pkb()
//        {
//            varTable = new List<string>();
//            procTable = new List<string>();
//            followsTable = new Dictionary<Statement, Statement>();
//            parentTable = new Dictionary<Statement, List<Statement>>();
//            usesTable = new Dictionary<Statement, List<Variable>>();
//            modifiesTable = new Dictionary<Statement, List<Variable>>();
//        }
//
//        //wywołanie instancji
//        public static Pkb GetInstance()
//        {
//            if (instance == null)
//            {
//                instance = new Pkb();
//            }
//            return instance;
//        }
//
//        public void SetFollows(Statement firstStatement, Statement nextStatement)
//        {
//            followsTable[firstStatement] = nextStatement;
//        }
//
//        public List<Statement> GetFollowed(Statement firstStatement)
//        {
//            List<Statement> followedStatements = new List<Statement>();
//            foreach (var entry in followsTable)
//            {
//                if (entry.Value == firstStatement)
//                {
//                    followedStatements.Add(entry.Key);
//                }
//            }
//            return followedStatements;
//        }
//
//        public List<Statement> GetFollows(Statement nextStatement)
//        {
//            List<Statement> followingStatements = new List<Statement>();
//            foreach (var entry in followsTable)
//            {
//                if (entry.Key == nextStatement)
//                {
//                    followingStatements.Add(entry.Value);
//                }
//            }
//            return followingStatements;
//        }
//
//        public void SetUses(Statement statement, Variable variable)
//        {
//            if (!usesTable.ContainsKey(statement))
//            {
//                usesTable[statement] = new List<Variable>();
//            }
//            usesTable[statement].Add(variable);
//        }
//
//        public List<Variable> GetUsed(Statement statement)
//        {
//            if (usesTable.ContainsKey(statement))
//            {
//                return usesTable[statement];
//            }
//            return new List<Variable>();
//        }
//
//        public List<Statement> GetUses(Variable variable)
//        {
//            List<Statement> statements = new List<Statement>();
//            foreach (var entry in usesTable)
//            {
//                if (entry.Value.Contains(variable))
//                {
//                    statements.Add(entry.Key);
//                }
//            }
//            return statements;
//        }
//
//        public bool IsUsed(Variable variable, Statement statement)
//        {
//            if (usesTable.ContainsKey(statement))
//            {
//                return usesTable[statement].Contains(variable);
//            }
//            return false;
//        }
//
//        public void SetModifies(Statement statement, Variable variable)
//        {
//            if (!modifiesTable.ContainsKey(statement))
//            {
//                modifiesTable[statement] = new List<Variable>();
//            }
//            modifiesTable[statement].Add(variable);
//        }
//
//        public List<Variable> GetModified(Statement statement)
//        {
//            if (modifiesTable.ContainsKey(statement))
//            {
//                return modifiesTable[statement];
//            }
//            return new List<Variable>();
//        }
//
//        public List<Statement> GetModifies(Variable variable)
//        {
//            List<Statement> statements = new List<Statement>();
//            foreach (var entry in modifiesTable)
//            {
//                if (entry.Value.Contains(variable))
//                {
//                    statements.Add(entry.Key);
//                }
//            }
//            return statements;
//        }
//
//        public bool IsModified(Variable variable, Statement statement)
//        {
//            if (modifiesTable.ContainsKey(statement))
//            {
//                return modifiesTable[statement].Contains(variable);
//            }
//            return false;
//        }
//
//        public int InsertVariable(string variableName)
//        {
//            if (!varTable.Contains(variableName))
//            {
//                varTable.Add(variableName);
//            }
//        }
//
//        public string GetVariableName(int index)
//        {
//            if (index >= 0 && index < varTable.Count)
//            {
//                return varTable[index];
//            }
//            return null;
//        }
//
//        public int GetVariableIndex(string variableName)
//        {
//            if (varTable.Contains(variableName))
//            {
//                return varTable[variableName];
//            }
//            else
//            {
//                return -1;
//            }
//        }
//
//        public void InsertProcedure(string procedureName)
//        {
//            if (!procTable.Contains(procedureName))
//            {
//                procTable.Add(procedureName);
//            }
//        }
//
//        public int GetProcTableSize()
//        {
//            return procTable.Count;
//        }
//
//        public string GetProcedureName(int index)
//        {
//            if (index >= 0 && index < procTable.Count)
//            {
//                return procTable[index];
//            }
//            return null;
//        }
//
//        public int GetProcedureIndex(string procedureName)
//        {
//            if (procTable.Contains(procedureName))
//            {
//                return procTable[procedureName];
//            }
//            return -1;
//        }
//    } 
//}
//
//