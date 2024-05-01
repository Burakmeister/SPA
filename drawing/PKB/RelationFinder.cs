using SPA.DesignEntities;
using System;
using System.Collections;

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
            InsertVariables();
            InsertWhiles();
            InsertAssigns();
            InsertConstants();
            //InsertModifiesRel(); nie działa już
            InsertFollowsRel();
            //InsertParentRel(); tez juz nie dziala :((
            //InsertUsesRel(); przykre
            InsertProcedures();
        }

        private void InsertProcedures() {
            for (int i = 0; i < procedures.Count; i++)
            {
                Procedure proc = (Procedure)procedures[i];
               pkb.InsertProcedure(proc.ProcName);
            }
        }

        private void InsertWhiles()
        {
            for (int i = 0; i < procedures.Count; i++)
            {
                FindProcedureWhiles((Procedure)procedures[i]!);
            }
        }

        private void FindProcedureWhiles(Procedure procedure)
        {
            if (procedure.StatementList!.FirstStatement != null)
            {
                FindStatementWhiles(procedure.StatementList.FirstStatement);
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

        private void InsertAssigns()
        {
            for (int i = 0; i < procedures.Count; i++)
            {
                FindProcedureAssigns((Procedure)procedures[i]!);
            }
        }

        private void FindProcedureAssigns(Procedure procedure)
        {
            if (procedure.StatementList!.FirstStatement != null)
            {
                FindStatementAssigns(procedure.StatementList.FirstStatement);
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

        private void InsertConstants()
        {
            for (int i = 0; i < procedures.Count; i++)
            {
                FindProcedureConstants((Procedure)procedures[i]!);
            }
        }

        private void FindProcedureConstants(Procedure procedure)
        {
            if (procedure.StatementList!.FirstStatement != null)
            {
                FindStatementConstants(procedure.StatementList.FirstStatement);
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
            else if (expr is ExprPlus)
            {
                ExprPlus exprPlus = (ExprPlus)expr;
                FindExprConstants(exprPlus.LeftExpr);
                FindExprConstants(exprPlus.RightExpr);
            }
        }

        private void InsertVariables()
        {
            for (int i = 0; i < procedures.Count; i++)
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
                }
                else if (statement is While)
                {
                    While stmtWhile = statement as While;
                    FindStatementModifies(stmtWhile.NextStatement);
                    FindStatementModifies(stmtWhile.StatementList.FirstStatement);

                }
            }
        }

        private void InsertFollowsRel()
        {
            for (int i = 0; i < procedures.Count; i++)
            {
                FindFollowsRelInProcedure((Procedure)procedures[i]!);
            }
        }

        private void FindFollowsRelInProcedure(Procedure procedure)
        {
            if (procedure.StatementList.FirstStatement != null)
            {
                FindStatementFollowed(procedure.StatementList.FirstStatement);
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

        private void InsertParentRel()
        {
            for (int i = 0; i < procedures.Count; i++)
            {
                FindParentRelInProcedure((Procedure)procedures[i]!);
            }
        }

        private void FindParentRelInProcedure(Procedure procedure)
        {
            if (procedure.StatementList.FirstStatement != null)
            {
                FindStatementChildren(procedure.StatementList.FirstStatement);
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

        private void InsertUsesRel()
        {
            for (int i = 0; i < procedures.Count; i++)
            {
                FindUsesRelInProcedure((Procedure)procedures[i]!);
            }
        }

        private void FindUsesRelInProcedure(Procedure procedure)
        {
            if (procedure.StatementList!.FirstStatement != null)
            {
                FindStatementUses(procedure.StatementList.FirstStatement);
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
                    pkb!.SetUses(statement, variable);
                }
            } else if (expr is ExprPlus)
            {
                ExprPlus exprPlus = (ExprPlus)expr;
                CheckExpr(statement, exprPlus.LeftExpr);
                CheckExpr(statement, exprPlus.RightExpr);
            }
        }
    }
}
