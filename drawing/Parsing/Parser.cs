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
        private Program? Program { get; set; } = null;
        private int lineNumber;
        private Dictionary<Call, string> calledProcedures = new();
        public List<Variable> VarList { get; set; } = new();

        public Program CreateProgram()
        {
            return new Program();
        }

        public Program? GetProgram()
        {
            return Program;
        }

        public While CreateWhile(ArrayList stringsList, Procedure procedure)
        {
            if (stringsList[^1] as string == "}" && stringsList[2] as string == "{" && IsNameAccepted(stringsList[1] as string))
            {
                Variable var = new Variable((stringsList[1] as string)!, lineNumber);
                While newWhile = new While(lineNumber, var);
                newWhile.StatementList = CreateStatementList(stringsList.GetRange(2, stringsList.Count-2), procedure);
                return newWhile;
            }
            else
            {
                throw new Exception("Nieprawidłowo zdefiniowana pętla while!");
            }
        }

        public If CreateIf(ArrayList stringsList, Variable variable, Procedure procedure)
        {
            If newIf = new If(lineNumber, variable);
            if (stringsList[1] as string == "then" && stringsList[2] as string == "{" && stringsList[^1] as string == "}")
            {
                StatementList statementList = CreateStatementList(stringsList.GetRange(2, stringsList.Count-3), procedure);
                newIf.Then = statementList;
            }
            else
            {
                throw new Exception("Nieprawidłowo zdefiniowany if!");
            }
            return newIf;
        }

        public StatementList CreateElse(ArrayList stringsList, Procedure procedure)
        {
            StatementList statementList;
            if (stringsList[0] as string == "else" && stringsList[1] as string == "{" && stringsList[^1] as string == "}")
            {
                statementList = CreateStatementList(stringsList.GetRange(1, stringsList.Count-1), procedure);
            }
            else
            {
                throw new Exception("Nieprawidłowo zdefiniowany else!");
            }
            return statementList;
        }

        public StatementList CreateStatementList(ArrayList stringsList, Procedure procedure)
        {
            lineNumber++;
            StatementList statementList = new StatementList();
            Statement? statement = null;
            Statement? prevStatement = null;
            for (int i = 1; i < stringsList.Count - 1;)
            {
                if (stringsList[i] as string == "while")
                {
                    int whileStart = i;
                    int whileLength = FindClosingBracket(stringsList.GetRange(whileStart, stringsList.Count - whileStart)) + 1;
                    statement = CreateWhile(stringsList.GetRange(whileStart, whileLength), procedure);
                    i += whileLength;
                }
                else if (stringsList[i] as string == "call")
                {
                    statement = new Call(procedure, lineNumber);
                    i++;
                    calledProcedures.Add((statement as Call)!, (stringsList[i] as string)!.Remove((stringsList[i]! as string)!.Length - 1));
                    i++;
                }
                else if (stringsList[i] as string == "if")
                {
                    i++;
                    if (!IsNameAccepted(stringsList[1] as string))
                    {
                        throw new Exception("Nieprawidłowa nazwa zmiennej!");
                    }
                    Variable variable = new Variable((stringsList[i] as string)!, lineNumber);
                    int ifStart = i;
                    int ifLength = FindClosingBracket(stringsList.GetRange(ifStart, stringsList.Count - ifStart)) + 1;
                    statement = CreateIf(stringsList.GetRange(ifStart, ifLength), variable, procedure);
                    i += ifLength;
                    if(stringsList[i] as string == "else")
                    {
                        int elseStart = i;
                        int elseLength = FindClosingBracket(stringsList.GetRange(elseStart, stringsList.Count - elseStart)) + 1;
                        (statement as If)!.Else = CreateElse(stringsList.GetRange(elseStart, elseLength), procedure);
                        i += elseLength;
                    }

                }
                else
                {
                    int assignStart = i;
                    int assignLength = FindSemicolon(stringsList.GetRange(assignStart, stringsList.Count - assignStart)) + 1;
                    statement = CreateAssign(stringsList.GetRange(assignStart, assignLength));
                    i += assignLength;
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
            lineNumber--;
            return statementList;
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
            List<char> mathSigns = new List<char>();
            List<string> variables = new List<string>();

            string equation = "";
            foreach(string s in stringsList)
            {
                equation += s;
            }
            equation = equation.Remove(equation.Length-1);

            char[] operators = { '+', '-', '*', '='};
            string currentVariable = "";

            foreach (char c in equation)
            {
                if (Array.Exists(operators, element => element == c))
                {
                    if (!string.IsNullOrEmpty(currentVariable))
                    {
                        variables.Add(currentVariable);
                        currentVariable = "";
                    }
                    mathSigns.Add(c);
                }
                else
                {
                    currentVariable += c;
                }
            }

            if (!string.IsNullOrEmpty(currentVariable))
            {
                variables.Add(currentVariable);
            }

            string tmp = variables[0];
            if(variables.Count > 0)
            {
                variables.RemoveAt(0);
            }
            if(mathSigns.Count > 0)
            {
                mathSigns.RemoveAt(0);
            }

            return new Assign(lineNumber, tmp, CreateExpr(variables, mathSigns));
        }


        public Expr CreateExpr(List<string> variables, List<char> mathSigns)
        {
            Expr temp;
            List<Expr> variablesAndConstants = new();
            foreach (string str in variables)
            {
                variablesAndConstants.Add(int.TryParse(str, out _) ?
                    new Constant(int.Parse(str), lineNumber) :
                    new Variable(str, lineNumber));
            }
            if(variablesAndConstants.Count == 1) { 
                return variablesAndConstants[0];
            }
            else
            {
                temp = mathSigns[0] switch
                {
                    '+' => new ExprPlus(lineNumber, variablesAndConstants[0]),
                    '-' => new ExprMinus(lineNumber, variablesAndConstants[0]),
                    '*' => new ExprTimes(lineNumber, variablesAndConstants[0]),
                    _ => throw new Exception("Nieobsługiwany znak w równaniu!")
                };

                bool flag = true;

                for (int i=1; i<mathSigns.Count;)
                {
                    switch(mathSigns[i])
                    {
                        case '+':
                            {
                                if (flag)
                                {
                                    if (temp is ExprPlus)
                                    {
                                        (temp as ExprPlus)!.RightExpr = variablesAndConstants[i];
                                    }
                                    else if (temp is ExprMinus)
                                    {
                                        (temp as ExprMinus)!.RightExpr = variablesAndConstants[i];
                                    }
                                    else // ExprTimes
                                    {
                                        (temp as ExprTimes)!.RightExpr = variablesAndConstants[i];
                                    }
                                }
                                temp = new ExprPlus(lineNumber, temp);
                                flag = true;
                                i++;
                                break;
                            }
                        case '-':
                            {
                                if (flag)
                                {
                                    if (temp is ExprPlus)
                                    {
                                        (temp as ExprPlus)!.RightExpr = variablesAndConstants[i];
                                    }
                                    else if (temp is ExprMinus)
                                    {
                                        (temp as ExprMinus)!.RightExpr = variablesAndConstants[i];
                                    }
                                    else // ExprTimes
                                    {
                                        (temp as ExprTimes)!.RightExpr = variablesAndConstants[i];
                                    }
                                }
                                temp = new ExprMinus(lineNumber, temp);
                                flag = true;
                                i++;
                                break;
                            }
                        case '*':
                            {
                                Expr? tmp = null;
                                if (temp is ExprPlus)
                                {
                                    tmp = new ExprTimes(lineNumber, variablesAndConstants[i++]);
                                    (temp as ExprPlus)!.RightExpr = tmp;
                                }
                                else if (temp is ExprMinus)
                                {
                                    tmp = new ExprTimes(lineNumber, variablesAndConstants[i++]);
                                    (temp as ExprMinus)!.RightExpr = tmp;
                                }
                                else // ExprTimes
                                {
                                    tmp = new ExprTimes(lineNumber, variablesAndConstants[i++]);
                                    (temp as ExprTimes)!.RightExpr = tmp;
                                }
                                Expr? tmp1 = tmp;
                                while(i < mathSigns.Count && mathSigns[i] == '*')
                                {
                                    (tmp1 as ExprTimes)!.RightExpr = new ExprTimes(lineNumber, variablesAndConstants[i++]);
                                    tmp1 = (tmp1 as ExprTimes)!.RightExpr;
                                }
                                (tmp1 as ExprTimes)!.RightExpr = variablesAndConstants[i];
                                flag = false;
                                break;
                            }
                    }
                }
                if (temp is ExprPlus)
                {
                    if ((temp as ExprPlus)!.RightExpr == null)
                    {
                        (temp as ExprPlus)!.RightExpr = variablesAndConstants[^1];
                    }
                }
                else if (temp is ExprMinus)
                {
                    if ((temp as ExprMinus)!.RightExpr == null)
                    {
                        (temp as ExprMinus)!.RightExpr = variablesAndConstants[^1];
                    }
                }
                else // ExprTimes
                {
                    if ((temp as ExprTimes)!.RightExpr == null)
                    {
                        (temp as ExprTimes)!.RightExpr = variablesAndConstants[^1];
                    }
                }
            }
            return temp;
        }

        public int Parse(string code) {
            string strippedCode = Regex.Replace(code, @"\r\n?|\n|\t", " ");
            strippedCode = Regex.Replace(strippedCode, @"\s+"," ");
            string[] strings = strippedCode.Split(' ');
            Procedure? procedure;
            Procedure? prevProcedure = null;
            Program = CreateProgram();
            ArrayList stringsList = new(strings);

            lineNumber = 0;

            for(int i = 0; i < stringsList.Count-1;)
            {
                if (stringsList[i] as string == "procedure")
                {
                    int procedureStart = i;
                    int procedureLength = FindClosingBracket(stringsList.GetRange(procedureStart, stringsList.Count - procedureStart)) + 1;
                    procedure = CreateProcedure(stringsList.GetRange(procedureStart, procedureLength));
                    i += procedureLength;
                    if (Program!.FirstProcedure == null)
                    {
                        Program.FirstProcedure = procedure;
                    }
                    else
                    {
                        prevProcedure!.NextProcedure = procedure;
                    }
                    prevProcedure = procedure;
                }
            }

            foreach (Call call in calledProcedures.Keys.ToArray())
            {
                string procName = calledProcedures.GetValueOrDefault(call)!;
                call.CalledProcedure = FindProcedureByName(Program, procName);
            }
            return lineNumber;
        }

        public Procedure CreateProcedure(ArrayList stringsList)
        {
            if (stringsList[^1] as string == "}" && stringsList[2] as string == "{" && IsNameAccepted(stringsList[1] as string))
            {
                Procedure newProcedure = new Procedure((stringsList[1] as string)!, lineNumber+1);
                newProcedure.StatementList = CreateStatementList(stringsList.GetRange(2, stringsList.Count-2), newProcedure);
                newProcedure.LineEndNumber = lineNumber;
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

        private Procedure FindProcedureByName(Program program, string name)
        {
            if (program.FirstProcedure != null)
            {
                Procedure first = program.FirstProcedure;
                if(first.ProcName == name)
                {
                    return first;
                }
                while (first!.NextProcedure != null)
                {
                    first = first.NextProcedure;
                    if (first.ProcName == name)
                    {
                        return first;
                    }
                }

            }
            throw new Exception("Wywoływana procedura nie istnieje!");
        }
    }
}