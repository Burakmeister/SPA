using SPA.DesignEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SPA.parser
{
    public class Parser
    {
        public Program? program { get; private set; }

        public void Parse(string code) {

            string strippedCode = Regex.Replace(code, @"\r\n?|\n|\t", " ");
            strippedCode = Regex.Replace(strippedCode, @"\s+"," ");
            string[] strings = strippedCode.Split(' ');
            program = new Program();
            for(int i=0; i<strings.Length; i++)
            {
                if (strings[i] == "procedure")
                {
                    string procName = strings[++i];

                }
            }
        }
    }
}
