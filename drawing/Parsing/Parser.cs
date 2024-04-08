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
        public Program? program = null;

        public Assign createAssign()
        {
            return new Assign(0, new Variable("xd", 0));
        }


        public Program createProgram()
        {
            return new Program();
        }

        public While createWhile(ArrayList stringsList)
        {
            if (stringsList[^1] as string == "}" && stringsList[2] as string == "{" && isNameAccepted(stringsList[1] as string))
            {
                Variable var = new Variable((stringsList[1] as string)!, 0);
                While newWhile = new While(0, var);
                StatementList statementList = new StatementList();
                newWhile.StatementList = statementList;
                Statement? statement;
                Statement? prevStatement = null;
                for (int i = 3; i < stringsList.Count - 1; i++)
                {
                    if (stringsList[i] as string == "while")
                    {
                        int whileStart = i;
                        int whileLength = findClosingBracket(stringsList.GetRange(whileStart, stringsList.Count - whileStart));
                        i += whileLength;
                        statement = createWhile(stringsList.GetRange(whileStart, whileLength));
                    }
                    else
                    {
                        statement = createAssign();
                        while ((stringsList[i] as string)[1] != ';')
                        {
                            i++;
                        }
                        i++;
                    }

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

        private bool isNameAccepted(string? name)
        {
            if(name==null) return false;
            if(name.Length > 0 && ((name[0]>64 && name[0] < 91) || (name[0] > 96 && name[0] < 123)))
            {
                return true;
            }
            return false;
        }

        public void Parse(string code) {
            string strippedCode = Regex.Replace(code, @"\r\n?|\n|\t", " ");
            strippedCode = Regex.Replace(strippedCode, @"\s+"," ");
            string[] strings = strippedCode.Split(' ');
            Procedure? procedure;
            Procedure? prevProcedure = null;
            program = createProgram();
            ArrayList stringsList = new(strings);

            for(int i = 0; i < stringsList.Count; i++)
            {
                if (stringsList[i] as string == "procedure")
                {
                    int procedureStart = i;
                    int procedureLength = findClosingBracket(stringsList.GetRange(procedureStart, stringsList.Count - procedureStart));
                    i += procedureLength;
                    procedure = createProcedure(stringsList.GetRange(procedureStart, procedureLength));
                    if (program.FirstProcedure == null)
                    {
                        program.FirstProcedure = procedure;
                    }
                    else
                    {
                        prevProcedure!.NextProcedure = procedure;
                    }
                    prevProcedure = procedure;
                    i++;
                }
            }
        }

        public Procedure createProcedure(ArrayList stringsList)
        {
            if (stringsList[^1] as string == "}" && stringsList[2] as string == "{" && isNameAccepted(stringsList[1] as string))
            {
                Procedure newProcedure = new Procedure((stringsList[1] as string)!);
                StatementList statementList = new StatementList();
                Statement? statement;
                Statement? prevStatement = null;
                newProcedure.StatementList = statementList;
                for(int i=3; i<stringsList.Count-1; i++)
                {
                    if (stringsList[i] as string == "while")
                    {
                        int whileStart = i;
                        int whileLength = findClosingBracket(stringsList.GetRange(whileStart, stringsList.Count - whileStart));
                        i += whileLength;
                        while (stringsList[i] as string!= "}")
                        {
                            i++;
                            whileLength++;
                        }
                        statement = createWhile(stringsList.GetRange(whileStart, whileLength));
                    }
                    else
                    {
                        statement = createAssign();
                        while ((stringsList[i] as string)!.Length==1)
                        {
                            i++;
                        }
                        i++;
                    }

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
        private int findClosingBracket(ArrayList stringsList)
        {
            int i = 0;
            int bracketCounter = 0;
            while (stringsList[i] as string != "{")
            {
                i++;
            }
            i++;
            for(; i<stringsList.Count-1; i++)
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
    }
}
