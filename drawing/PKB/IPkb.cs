﻿using SPA.DesignEntities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.PKB
{
    public interface IPkb
    {
        void SetFollows(Statement firstStatement, Statement nextStatement);

        int GetFollowed(int statementNumber);

        int GetFollows(int statementNumber);

        List<int> GetAllFollowed();

        List<int> GetAllFollows();

        bool IsFollowed(int statementNumber, int nextStatement);

        void SetUses(int statement, string variable);

        List<string> GetUsed(int statement);

        List<int> GetUses(string variable);

        List<int> GetAllStatementsThatUseVariables();

        bool IsUsed(string variable,int statement);

        void SetModifies(int statement, string variable);

        List<string> GetModified(int statementNumber);

        List<int> GetModifies(string variable);

        List<int> GetAllStatementsThatModifieVariables();

        bool IsModified(string variable, int statement);

        void SetParent(Statement parentStatement, Statement childStatement);

        List<int> GetChildren(int parentStatement);

        int GetParent(int childStatement);

        List<int> GetAllChildren();

        List<int> GetAllParents();

        bool IsParent(int parentStatement, int childStatement);

        int InsertVariable(String variableName);

        String GetVariableName(int index);

        int GetVariableIndex(String VariableName);

        int GetVariablesSize();

        List<string> GetVariables();

        int InsertProcedure(String procedureName);

        int GetProceduresSize();

        String GetProcedureName(int index);

        int GetProcedureIndex(String ProcedureName);

        List<string> GetProcedures();

        int InsertAssign(int statementNumber);

        List<int> GetAssigns();

        int InsertWhile(int statementNumber);

        List<int> GetWhiles();

        int InsertConstant(int constant);

        List<int> GetConstants();

        bool isConstInConstants(int c);

        int GetProgramLength();

        List<int> GetAllStatements();

        void ClearPkb(int statementCount);
    }
}
