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
        public Program? program { get; private set; }

        public Assign createAssign(Variable var, Expr expr)
        {
            throw new NotImplementedException();
        }

        public ExprPlus createExprPlus(Expr left, Expr right)
        {
            throw new NotImplementedException();
        }

        public Procedure createProcedure(string[] code)
        {
            throw new NotImplementedException();
        }

        public Program createProgram()
        {
            return new Program();
        }

        public While createWhile(Variable var, string[] code)
        {
            throw new NotImplementedException();
        }

        private bool isNameAccepted(string name)
        {
            if(name.Length > 0 && ((name[0]>64 && name[0] < 91) || (name[0] > 96 && name[0] < 123)))
            {
                return true;
            }
            return false;
        }

        private bool isProcedure(string[] code)
        {
            return false;
        }

        public void Parse(string code) {
            string strippedCode = Regex.Replace(code, @"\r\n?|\n|\t", " ");
            strippedCode = Regex.Replace(strippedCode, @"\s+"," ");
            string[] strings = strippedCode.Split(' ');
            if (strings[0] == "procedure")
            {
                program = createProgram();
                ArrayList list = new(strings);
                int j = 0;
                for(int i = 1; i < strings.Length; i++)
                {
                    if (strings[i] == "procedure")
                    {
                        createProcedure(list.GetRange(j, i - 1));
                        j = i;
                    }
                }
                createProcedure(list.GetRange(j, strings.Length-1));
            }
            else
            {
                throw new Exception("Kod nie zaczyna sie od procedury!");
            }
        }

        private Procedure? getLastProcedure()
        {
            Procedure procedure = program.FirstProcedure;
            while (procedure != null)
            {
                procedure = procedure.NextProcedure;
            }
            return procedure;
        }

        public Procedure createProcedure(ArrayList arrayList)
        {
            throw new NotImplementedException();
        }
    }
}
