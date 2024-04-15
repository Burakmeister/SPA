using SPA.DesignEntities;
using SPA.Parsing;
using SPA.PKB;
using System;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SPA.ViewModels
{
    public class Drawer : IDrawer
    {
        public string Code { get; set; } = "";
        private ArrayList procedures = new ArrayList();
        private int currentIndex = 0;
        private IPkb? pkb;
        private IDrawerAST DrawerAST= new DrawerAST();

        public IParser Parser { get; } = new Parser();
        public ICommand ParseCommand => new Command((param) =>
        {
            try
            {
                int numOfLines;
                numOfLines = Parser.Parse(Code);
                CompleteProceduresList();
                pkb = Pkb.GetInstance(numOfLines);
                FillPKB();
                currentIndex = 0;
                DrawerAST.DrawTree(procedures[currentIndex] as Procedure);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        });

        public ICommand DrawNextProcedureCommand => throw new NotImplementedException();

        public ICommand DrawPrevProcedureCommand => throw new NotImplementedException();

        private void CompleteProceduresList()
        {
            Program program;
            if (Parser.GetProgram() != null)
            {
                program = Parser.GetProgram()!;
                if(program.FirstProcedure != null)
                {
                    Procedure first = program.FirstProcedure;
                    procedures.Add(first);
                    while (first!.NextProcedure != null)
                    {
                        procedures.Add(first.ProcName);
                        first = first.NextProcedure;
                    }
                }
            }

        }

        private void FillPKB()
        {
            InsertVariablesIntoVarTable();
            InsertModifiesRel();
        }

        private void InsertVariablesIntoVarTable()
        {
            for(int i = 0; i<procedures.Count; i++)
            {
                FindProcedureVariables((Procedure)procedures[i]!);
            }
        }

        private void FindProcedureVariables(Procedure procedure)
        {
            if (procedure.StatementList!.FirstStatement != null)
            {
                FindStatementVariables(procedure.StatementList.FirstStatement);
            }
        }

        private void FindStatementVariables(Statement statement)
        {

            if (statement != null)
            {
                if (statement is Assign)
                {
                    Assign assign = statement as Assign;
                    if (!CheckIfVarIsInVarTable(assign.Var.VarName))
                    {
                        pkb.InsertVariable(assign.Var.VarName);
                        FindStatementVariables(assign.NextStatement);
                    }
                }
                else if (statement is While)
                {
                    While stmtWhile = statement as While;
                    if (!CheckIfVarIsInVarTable(stmtWhile.Var.VarName))
                    {
                        pkb.InsertVariable(stmtWhile.Var.VarName);
                        FindStatementVariables(stmtWhile.NextStatement);
                        FindStatementVariables(stmtWhile.StatementList.FirstStatement);
                    }
                }
            }
        }

        private bool CheckIfVarIsInVarTable(string varName)
        {
            if(pkb.GetVariableIndex(varName) != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void InsertModifiesRel()
        {
            for (int i = 0; i < procedures.Count; i++)
            {
                FindModifiesRelInProcedure((Procedure)procedures[i]);
            }
        }
        private void FindModifiesRelInProcedure(Procedure procedure)
        {
            if (procedure.StatementList.FirstStatement != null)
            {
                FindStatementModifies(procedure.StatementList.FirstStatement);
            }
        }

        private void FindStatementModifies(Statement statement)
        {
            if (statement != null)
            {
                if (statement is Assign)
                {
                    Assign assign = statement as Assign;
                    pkb.SetModifies(assign, assign.Var);
                    FindStatementModifies(assign.NextStatement);
                } else if (statement is While)
                {
                    While stmtWhile = statement as While;
                    FindStatementModifies(stmtWhile.NextStatement);
                    FindStatementModifies(stmtWhile.StatementList.FirstStatement);

                }
            }
        }
    }
}
