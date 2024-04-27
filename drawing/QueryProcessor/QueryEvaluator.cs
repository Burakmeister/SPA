using SPA.DesignEntities;
using SPA.PKB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    internal class QueryEvaluator
    {
        private IPkb Pkb;
        private Relation? Relation;
        private string synonym;
        private With? With;

        public QueryEvaluator(Query query, IPkb pkb) 
        {
            Pkb = pkb;
            if(query.SuchThatClause!=null)
                Relation = query.SuchThatClause.Relation;
            With = query.WithClause;
            synonym = query.Synonym;
            Dictionary<string, string> declarationsMap = getDeclarations(query);
            query.Result = executeQuery(declarationsMap);
        }

        // robię słownik deklaracji z ich jako klucze nazwami i jako wartości typami
        private Dictionary<string, string> getDeclarations(Query query)
        {
            Dictionary<string, string> declarationsMap = new Dictionary<string, string>();
            List<Declaration> declarations = query.Declarations;
            foreach (Declaration declaration in declarations)
            {
                foreach(string name in declaration.Synonyms)
                {
                    declarationsMap.Add(name, declaration.DesignEntity);
                }
            }
            return declarationsMap;
        }

        private List<string> executeQuery(Dictionary<string, string> declarationsMap)
        {
            if(Relation==null && With == null)
            {
                string? varType;
                varType = declarationsMap[synonym];
                if(varType == "stmt")
                {
                    int programLength = Pkb.GetProgramLength();
                    return new List<string>(new string[] {"0", "-", programLength.ToString()});
                }
                else if(varType == "assign")
                {
                    return Pkb.GetAssigns().ConvertAll<string>(x => x.ToString());
                }
                else if (varType == "while")
                {
                    return Pkb.GetWhiles().ConvertAll<string>(x => x.ToString());
                }
                else if (varType == "variable")
                {
                    return Pkb.GetVariables();
                }
                else if (varType == "constant")
                {
                    return Pkb.GetConstants().ConvertAll<string>(x => x.ToString());
                }
                else if (varType == "prog_line")
                {
                    int programLength = Pkb.GetProgramLength();
                    return new List<string>(new string[] { "0", "-", programLength.ToString() });
                }
                else
                {
                    throw new Exception("Niepoprawny typ zmiennej!");
                }
            }
            else if(With == null)
            {
                if(Relation is Follows follows)
                {
                    string stmtRef1 = follows.StmtRef.Value;
                    string stmtRef2 = follows.StmtRef2.Value;
                    if (stmtRef1 == "_" && stmtRef2 == "_")
                    {
                        throw new Exception("Relacja z podłogą nieobsługiwana!");
                    }
                    else if(stmtRef1 == "_")
                    {
                        throw new Exception("Relacja z podłogą nieobsługiwana!");
                    }
                    else if (stmtRef2 == "_")
                    {
                        throw new Exception("Relacja z podłogą nieobsługiwana!");
                    }
                    else
                    {
                        int firstRef;
                        int secondRef;

                        if(int.TryParse(stmtRef1, out firstRef))
                        {
                            if(int.TryParse(stmtRef2, out secondRef))
                            {
                                // tabela statementów jest niedostępna z PKB :(
                                // nie widze tez mozliwosci sprawdzenia followed*
                                //return new List<string>().Add(Pkb.IsFollowed(firstRef, secondRef).ToString);
                            }


                        }
                        else if(int.TryParse(stmtRef2, out secondRef))
                        {

                        }
                    }
                }
                else if(Relation is FollowsT followsT)
                {
                    string stmtRef1 = followsT.StmtRef.Value;
                    string stmtRef2 = followsT.StmtRef2.Value;
                }
                else if (Relation is Parent parent)
                {
                    string stmtRef1 = parent.StmtRef.Value;
                    string stmtRef2 = parent.StmtRef2.Value;
                }
                else if (Relation is ParentT parentT)
                {
                    string stmtRef1 = parentT.StmtRef.Value;
                    string stmtRef2 = parentT.StmtRef2.Value;
                }
                else if (Relation is ModifiesS modifiesS)
                {
                    string stmtRef = modifiesS.StmtRef.Value;
                    string entRef = modifiesS.EntRef.Value;
                }
                else if (Relation is UsesS usesS)
                {
                    string stmtRef = usesS.StmtRef.Value;
                    string entRef = usesS.EntRef.Value;
                }
                else
                {
                    throw new Exception("Nieprawidłowy typ relacji!");
                }
                throw new Exception("such that not implemented yet");
            }
            else if (Relation == null)
            {
                throw new Exception("With not implemented yet!");
            }
            throw new Exception("Coś poszło bardzo nie tak!");
        }
    }
}
