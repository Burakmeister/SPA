using SPA.DesignEntities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            InsertVariablesIntoVarTable();
            InsertModifiesRel();
            InsertFollowsRel();
            InsertParentRel();
        }

        private void InsertVariablesIntoVarTable()
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
    }
}
