using SPA.PKB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    internal class Follows : Relation
    {
        public StmtRef leftStmtRef { get; set; }
        public StmtRef rightStmtRef { get; set; }

        //public override Dictionary<string, List<string>> EvaluateRelation(Dictionary<string, List<string>> results, IPkb pkb)
        //{
        //    string[] keys = results.Keys.ToArray();

        //    if (keys.Contains(leftStmtRef.Value) && rightStmtRef2.Value == "_")
        //    {
        //        for(int i = 0;i < pkb.programLength)
        //    }
        //    else
        //    {

        //    }

        //    throw new NotImplementedException();
        //}
    }
}
