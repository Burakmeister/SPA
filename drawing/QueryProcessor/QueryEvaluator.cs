using SPA.DesignEntities;
using SPA.PKB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    internal class QueryEvaluator
    {
        private IPkb Pkb;
        private Dictionary<string, string> declarationsMap;
        private Relation? Relation;
        private string synonym;
        private With? With;

        public List<string>? Return { get; set; } = null;
        public QueryEvaluator(Query query, IPkb pkb) 
        {
            Pkb = pkb;
            if(query.SuchThatClause!=null)
                Relation = query.SuchThatClause.Relation;
            With = query.WithClause;
            synonym = query.Synonym;
            declarationsMap = getDeclarations(query);
            executeQuery();
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

        private List<string> executeQuery()
        {
            if(Relation==null && With == null){
                string? varType;
                if (declarationsMap.TryGetValue(synonym, out varType))
                {
                    throw new Exception("Próbujesz użyć niezadeklarowanej zmiennej!");
                }
                if(varType == "stmt")
                {
                    // potrzebna by była lista statementow, ale to troche bez sensu
                    // bo to wszystkie linie w sumie
                    
                    // można tu zwracać np. liczbę linii w kodzie ogólnie coś w stylu 1-(liczba linii)
                    // więc można dodać w pkb możliwość pobrania tego
                    // brakuje też możliwości pobrania całego programu z pkb
                    throw new Exception("not implemented yet!");
                }
                else if(varType == "assign")
                {
                    // tu potrzebna jest lista wszystkich assignow
                    throw new Exception("not implemented yet!");
                }
                else if (varType == "while")
                {
                    // potrzebna jest lista while'ow
                    throw new Exception("not implemented yet!");
                }
                else if (varType == "variable")
                {
                    // return Pkb.Var
                    // chyba tu ktoś nie przymyślał
                    // nie moge pobrac z PKB tej tablicy
                    throw new Exception("not implemented yet!");
                }
                else if (varType == "constant")
                {
                    // lista constantow
                    throw new Exception("not implemented yet!");
                }
                else if (varType == "prog_line")
                {
                    // a tu to beka w sumie, nie mam pojecia jak to obsluzyc
                    // sytuacja taka sama jak z stmt 
                    throw new Exception("not implemented yet!");
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
