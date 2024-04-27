using Microsoft.VisualBasic;
using SPA.DesignEntities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;

namespace SPA.Parsing
{
    public class Parser : IParser
    {
        public Program? program { get; set; } = null;
        private int lineNumber;

        public Program CreateProgram()
        {
            return new Program();
        }

        public Program? GetProgram()
        {
            return program;
        }

        public While CreateWhile(ArrayList stringsList)
        {
            if (stringsList[^1] as string == "}" && stringsList[2] as string == "{" && IsNameAccepted(stringsList[1] as string))
            {
                Variable var = new Variable((stringsList[1] as string)!, lineNumber);
                While newWhile = new While(lineNumber, var);
                lineNumber++;
                StatementList statementList = new StatementList();
                newWhile.StatementList = statementList;
                Statement? statement = null ;
                Statement? prevStatement = null;
                for (int i = 3; i < stringsList.Count - 1;)
                {
                    if (stringsList[i] as string == "while")
                    {
                        int whileStart = i;
                        int whileLength = FindClosingBracket(stringsList.GetRange(whileStart, stringsList.Count - whileStart)) + 1;
                        statement = CreateWhile(stringsList.GetRange(whileStart, whileLength));
                        i += whileLength;
                    }
                    else
                    {
                        int assignStart = i;
                        int assignLength = FindSemicolon(stringsList.GetRange(assignStart, stringsList.Count - assignStart)) + 1;
                        statement = CreateAssign(stringsList.GetRange(assignStart, assignLength));
                        i+= assignLength;
                    }
                    lineNumber++;

                    if (statementList.FirstStatement == null)
                    {
                        statementList.FirstStatement = statement;
                    }
                    else
                    {
                        prevStatement!.NextStatement = statement;
                    }
                    prevStatement = statement;
                }
                return newWhile;
            }
            else
            {
                throw new Exception("Nieprawidłowo zdefiniowana pętla while!");
            }
        }

        private bool IsNameAccepted(string? name)
        {
            if(name==null) return false;
            if(name.Length > 0 && ((name[0]>64 && name[0] < 91) || (name[0] > 96 && name[0] < 123)))
            {
                return true;
            }
            return false;
        }

        public Assign CreateAssign(ArrayList stringsList)
        {
            if ((stringsList[^1] as string)!.EndsWith(';') && stringsList[1] as string == "=" && IsNameAccepted(stringsList[0] as string))
            {
                string varName = (stringsList[0] as string)!;
                Expr? expr = CreateExpr(stringsList.GetRange(2, stringsList.Count-2));
                return new Assign(lineNumber, varName, expr);
            }
            throw new Exception("Nieprawidłowo zdefiniowane przypisanie!");
        }

        public Expr? CreateExpr(ArrayList stringsList)
        {
            ArrayList exprType = new ArrayList();
            ArrayList vars = new ArrayList();

            int semicolonPos = FindSemicolon(stringsList);

            for (int i=0; i<stringsList.Count; i++)
            {
                if(i%2 == 0)
                {
                    if (i == semicolonPos)
                    {
                        if(IsNameAccepted((stringsList[i] as string)![..^1]))
                        {
                            vars.Add(new Variable((stringsList[i] as string)![..^1], lineNumber));
                        }
                        else
                        {
                            try
                            {
                                int constant = int.Parse((stringsList[i] as string)![..^1]);
                                vars.Add(new Constant(constant, lineNumber));
                            }
                            catch(Exception e)
                            {
                                throw new Exception("Nieprawidłowa nazwa zmiennej! " + "[" + lineNumber + "]");
                            }
                        }
                    }
                    else if (IsNameAccepted(stringsList[i] as string))
                    {
                        vars.Add(new Variable((stringsList[i] as string)!, lineNumber));
                    }
                    else
                    {
                        try
                        {
                            int constant = int.Parse((stringsList[i] as string)!);
                            vars.Add(new Constant(constant, lineNumber));
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Nieprawidłowa nazwa zmiennej! " + "[" + lineNumber + "]");
                        }
                    }
                }
                else
                {
                    if (stringsList[i] as string == "+")
                    {
                        exprType.Add(new ExprPlus(lineNumber));
                    }
                    else
                    {
                        throw new Exception("Nieprawidłowe równanie! " + "[" + lineNumber + "]");
                    }
                }
            }

            if (exprType.Count > 0)
            {
                for (int i = 0; i < exprType.Count-1; i++)
                {
                    (exprType[i] as ExprPlus)!.RightExpr = exprType[i+1] as ExprPlus;
                }
                (exprType[^1] as ExprPlus)!.RightExpr = vars[^1] as Factor;

                for (int i = 0; i < vars.Count - 1; i++)
                {
                    (exprType[i] as ExprPlus)!.LeftExpr = vars[i] as Factor;
                }
            }else if (exprType.Count == 0 && vars.Count==1) {
                return vars[0] as Expr;
            }
            return exprType[0] as Expr;
        }

        public int Parse(string code) {
            string strippedCode = Regex.Replace(code, @"\r\n?|\n|\t", " ");
            strippedCode = Regex.Replace(strippedCode, @"\s+"," ");
            string[] strings = strippedCode.Split(' ');
            Procedure? procedure;
            Procedure? prevProcedure = null;
            program = CreateProgram();
            ArrayList stringsList = new(strings);

            lineNumber = 1;

            for(int i = 0; i < stringsList.Count-1; i++)
            {
                if (stringsList[i] as string == "procedure")
                {
                    int procedureStart = i;
                    int procedureLength = FindClosingBracket(stringsList.GetRange(procedureStart, stringsList.Count - procedureStart));
                    procedure = CreateProcedure(stringsList.GetRange(procedureStart, procedureLength));
                    if (program!.FirstProcedure == null)
                    {
                        program.FirstProcedure = procedure;
                    }
                    else
                    {
                        prevProcedure!.NextProcedure = procedure;
                    }
                    prevProcedure = procedure;
                    i+=procedureLength;
                }
            }
            return lineNumber;
        }

        public Procedure CreateProcedure(ArrayList stringsList)
        {
            if (stringsList[^1] as string == "}" && stringsList[2] as string == "{" && IsNameAccepted(stringsList[1] as string))
            {
                Procedure newProcedure = new Procedure((stringsList[1] as string)!);
                StatementList statementList = new StatementList();
                Statement? statement;
                Statement? prevStatement = null;
                newProcedure.StatementList = statementList;
                for(int i=3; i<stringsList.Count-1;)
                {
                    if (stringsList[i] as string == "while")
                    {
                        int whileStart = i;
                        int whileLength = FindClosingBracket(stringsList.GetRange(whileStart, stringsList.Count - whileStart)) + 1;
                        statement = CreateWhile(stringsList.GetRange(whileStart, whileLength));
                        i += whileLength;
                    }
                    else
                    {
                        int assignStart = i;
                        int assignLength = FindSemicolon(stringsList.GetRange(assignStart, stringsList.Count - assignStart)) + 1;
                        statement = CreateAssign(stringsList.GetRange(assignStart, assignLength));
                        i+= assignLength;
                    }
                    lineNumber++;
                    if(statementList.FirstStatement == null)
                    {
                        statementList.FirstStatement = statement;
                    }
                    else 
                    { 
                        prevStatement!.NextStatement = statement;
                    }
                    prevStatement = statement;
                }
                return newProcedure;
            }
            else
            {
                throw new Exception("Nieprawidłowo zdefiniowana procedura!");
            }
        }

        private int FindClosingBracket(ArrayList stringsList)
        {
            int i = 0;
            int bracketCounter = 0;
            while (stringsList[i] as string != "{")
            {
                i++;
            }
            i++;
            for(; i<stringsList.Count; i++)
            {
                if(stringsList[i] as string == "{") bracketCounter++;
                if(stringsList[i] as string == "}" && bracketCounter == 0)
                {
                    return i;
                }
                else if(stringsList[i] as string == "}")
                {
                    bracketCounter--;
                }
            }
            throw new Exception("Brakuje klamry zamykającej!");
        }

        private int FindSemicolon(ArrayList stringsList)
        {
            for (int i=0; i < stringsList.Count; i++)
            {
                if (stringsList[i] as string == ";" || (stringsList[i] as string)!.EndsWith(';'))
                {
                    return i;
                }
            }
            throw new Exception("Brakuje średnika!");
        }
    }
}
