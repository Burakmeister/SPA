using SPA.DesignEntities;
using System;
using System.Collections;
using System.Security.Cryptography;

namespace SPA.PKB
{
    public class RelationFinder
    {

        private ArrayList procedures; 
        private IPkb? pkb;

        public RelationFinder(ArrayList procedures, IPkb pkb) {
            this.procedures = procedures;
            this.pkb = pkb;
        }
       
        public void FillPKB()
        {
            InsertProcedures();
            InsertAbstractionsAndRelations();
            InsertContainerUsesAndModifies();
        }

        private void InsertContainerUsesAndModifies()
        {
            foreach (int wh in pkb.GetWhiles())
            {
                foreach (int child in pkb.GetChildren(wh))
                {
                    pkb.GetUsed(child).ForEach(var =>
                        pkb.SetUses(wh, var));
                    pkb.GetModified(child).ForEach(var =>
                        pkb.SetModifies(wh, var));
                }
            }
        }

        private void InsertAbstractionsAndRelations()
        {
            for (int i = 0; i < procedures.Count; i++)
            {
                FindProcedureAbstractionsAndRelations((Procedure)procedures[i]!);
            }
        }
        private void FindProcedureAbstractionsAndRelations(Procedure procedure)
        {
            if (procedure.StatementList!.FirstStatement != null)
            {
                FindStatementWhiles(procedure.StatementList.FirstStatement);
                FindStatementAssigns(procedure.StatementList.FirstStatement);
                FindStatementConstants(procedure.StatementList.FirstStatement);
                FindStatementVariables(procedure.StatementList.FirstStatement);
                FindStatementModifies(procedure.StatementList.FirstStatement);
                FindStatementFollowed(procedure.StatementList.FirstStatement);
                FindStatementChildren(procedure.StatementList.FirstStatement);
                FindStatementUses(procedure.StatementList.FirstStatement);
            }
        }

        private void InsertProcedures() {
            for (int i = 0; i < procedures.Count; i++)
            {
                Procedure proc = (Procedure)procedures[i];
               pkb.InsertProcedure(proc.ProcName);
            }
        }

        private void FindStatementWhiles(Statement statement)
        {
            if (statement != null)
            {
                if (statement is Assign)
                {
                    FindStatementWhiles(statement.NextStatement);
                }
                else if (statement is While)
                {
                    While stmtWhile = statement as While;
                    pkb.InsertWhile(stmtWhile.LineNumber);
                    FindStatementWhiles(stmtWhile.NextStatement);
                    FindStatementWhiles(stmtWhile.StatementList.FirstStatement);
                }
            } 
        }

        private void FindStatementAssigns(Statement statement)
        {
            if (statement != null)
            {
                if (statement is Assign)
                {
                    pkb.InsertAssign(statement.LineNumber);
                    FindStatementAssigns(statement.NextStatement);
                }
                else if (statement is While)
                {
                    While stmtWhile = statement as While;
                    FindStatementAssigns(stmtWhile.NextStatement);
                    FindStatementAssigns(stmtWhile.StatementList.FirstStatement);
                }
            }
           
        }

        private void FindStatementConstants(Statement statement)
        {
            if (statement != null)
            {
                if (statement is Assign)
                {
                    Assign assign = (statement as Assign)!;
                    FindExprConstants(assign.Expr);
                    FindStatementConstants(statement.NextStatement);
                }
                else if (statement is While)
                {
                    While stmtWhile = statement as While;
                    FindStatementConstants(stmtWhile.NextStatement);
                    FindStatementConstants(stmtWhile.StatementList.FirstStatement);
                }
            }
           
        }

        private void FindExprConstants(Expr expr)
        {
            if (expr is Factor)
            {
                Factor factor = (Factor)expr;
                if (factor is Constant)
                {
                    Constant constant = (Constant)factor;
                    pkb.InsertConstant(constant.Value);
                }
            }
            else if (expr is ExprMinus)
            {
                ExprMinus exprPlus = (ExprMinus)expr;
                FindExprConstants(exprPlus.LeftExpr);
                FindExprConstants(exprPlus.RightExpr);
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
                    }
                    FindStatementVariables(assign.NextStatement);
                }
                else if (statement is While)
                {
                    While stmtWhile = statement as While;
                    if (!CheckIfVarIsInVarTable(stmtWhile.Var.VarName))
                    {
                        pkb.InsertVariable(stmtWhile.Var.VarName);
                    }
                    FindStatementVariables(stmtWhile.NextStatement);
                    FindStatementVariables(stmtWhile.StatementList.FirstStatement);
                }
            }
        }

        private bool CheckIfVarIsInVarTable(string varName)
        {
            if (pkb.GetVariableIndex(varName) != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void FindStatementModifies(Statement statement)
        {
            if (statement != null)
            {
                if (statement is Assign)
                {
                    Assign assign = statement as Assign;
                    pkb.SetModifies(assign.LineNumber, assign.Var.VarName);
                    FindStatementModifies(assign.NextStatement);
                }
                else if (statement is While)
                {
                    While stmtWhile = statement as While;
                    FindStatementModifies(stmtWhile.NextStatement);
                    FindStatementModifies(stmtWhile.StatementList.FirstStatement);

                }
            }
        }

        private void FindStatementFollowed(Statement statement)
        {
            if (statement.NextStatement != null)
            {
                pkb.SetFollows(statement, statement.NextStatement);
                FindStatementFollowed(statement.NextStatement);
            } else if (statement is While)
            {
                While stmWhile = statement as While;
                FindStatementFollowed(stmWhile.StatementList.FirstStatement);
            }
        }

        private void FindStatementChildren(Statement statement)
        {
            if (statement is While)
            {
                While stmWhile = statement as While;
                Statement stmt = stmWhile.StatementList.FirstStatement;
                while (stmt != null)
                {
                    pkb.SetParent(stmWhile, stmt);
                    FindStatementChildren(stmt);
                    stmt = stmt.NextStatement;
                }
            } else if (statement != null)
            {
                FindStatementChildren(statement.NextStatement);
            }
        }

        private void FindStatementUses(Statement statement)
        {
            if (statement != null && statement is Assign)
            {
                Assign assign = (statement as Assign)!;
                CheckExpr(assign, assign.Expr);
                FindStatementUses(statement!.NextStatement!);

            } else if (statement is While)
            {
                While stmtWhile = (statement as While)!;
                pkb!.SetUses(statement.LineNumber, stmtWhile.Var.VarName);
                FindStatementUses(stmtWhile!.StatementList!.FirstStatement!);
            }
            else if (statement != null)
            {
                FindStatementUses(statement.NextStatement!);
            }
        }

        private void CheckExpr(Statement statement, Expr expr)
        {
            if (expr is Factor)
            {
                Factor factor = (Factor)expr;
                if (factor is Variable)
                {
                    Variable variable = (Variable)factor;
                    pkb!.SetUses(statement.LineNumber, variable.VarName);
                }
            } else if (expr is ExprMinus)
            {
                ExprMinus exprPlus = (ExprMinus)expr;
                CheckExpr(statement, exprPlus.LeftExpr);
                CheckExpr(statement, exprPlus.RightExpr);
            }
        }
    }
}
