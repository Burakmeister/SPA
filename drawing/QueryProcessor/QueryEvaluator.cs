using SPA.DesignEntities;
using SPA.PKB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    // pseudoenum  (enum w c# nie przyjmuje zmiennych)
    public sealed class VarType
    {
        public static readonly VarType ASSIGN = new VarType("assign");
        public static readonly VarType STMT = new VarType("stmt");
        public static readonly VarType PROG_LINE = new VarType("prog_line");
        public static readonly VarType WHILE = new VarType("while");
        public static readonly VarType VARIABLE = new VarType("variable");
        public static readonly VarType CONSTANT = new VarType("constant");

        private string _varType;

        private VarType(string varType)
        {
            this._varType = varType;
        }

        public static VarType GetVarType(string varType)
        {
            if (varType == "stmt")
            {
                return STMT;
            }
            else if (varType == "assign")
            {
                return ASSIGN;
            }
            else if (varType == "while")
            {
                return WHILE;
            }
            else if (varType == "variable")
            {
                return VARIABLE;
            }
            else if (varType == "constant")
            {
                return CONSTANT;
            }
            else if (varType == "prog_line")
            {
                return PROG_LINE;
            }
            else
            {
                throw new Exception("Niepoprawny typ zmiennej!");
            }
        }
    }

    // właściwa klasa
    public class QueryEvaluator
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
            query.Result = ExecuteQuery(declarationsMap);
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

        // sprawdzam czy to przypadek z relacją, z with itd.
        private List<string> ExecuteQuery(Dictionary<string, string> declarationsMap)
        {
            string? varTypeStr;
            varTypeStr = declarationsMap[synonym];
            VarType varType = VarType.GetVarType(varTypeStr);
            if (Relation == null && With == null)
            {
                return Select(varType);
            }
            else if (With == null)
            {
                return SelectRelation(varType, Relation!);
            }
            else if (Relation == null)
            {
                return SelectWith(varType);
            }
            else
            {
                return SelectRelationWith(varType);
            }
            throw new Exception("Coś poszło bardzo nie tak (QueryEvaluator.ExecuteQuery())!");
        }

        // najprostszy przypadek bez relaji i bez with
        private List<string> Select(VarType varType)
        { 
            int programLength = Pkb.GetProgramLength();
            switch (varType)
            {
                case var type when type == VarType.STMT:
                    return new List<string>(new string[] { "1", "-", programLength.ToString() });

                case var type when type == VarType.ASSIGN:
                    return Pkb.GetAssigns().ConvertAll<string>(x => x.ToString());

                case var type when type == VarType.VARIABLE:
                    return Pkb.GetVariables();

                case var type when type == VarType.PROG_LINE:
                    return new List<string>(new string[] { "1", "-", programLength.ToString() });

                case var type when type == VarType.CONSTANT:
                    return Pkb.GetConstants().ConvertAll<string>(x => x.ToString());

                case var type when type == VarType.WHILE:
                    return Pkb.GetWhiles().ConvertAll<string>(x => x.ToString());
                
                default:
                    throw new Exception("Niepoprawny typ zmiennej!");
            }
        }

        // przypadek z relacją bez with
        private List<string> SelectRelation(VarType varType, Relation relation)
        {
            switch (relation)
            {
                case var type when type is Follows:
                    return FollowsRelation((type as Follows)!);
                case var type when type is FollowsT:
                    return FollowsTRelation((type as FollowsT)!);
                case var type when type is Parent:
                    return ParentRelation((type as Parent)!);
                case var type when type is ParentT:
                    return ParentTRelation((type as ParentT)!);
                case var type when type is ModifiesS:
                    return ModifiesSRelation((type as ModifiesS)!);
                case var type when type is UsesS:
                    return UsesSRelation((type as UsesS)!);
                default:
                    throw new Exception("Nieprawidłowa relacja!");
            }
        }

        // relacja follow
        private List<string> FollowsRelation(Follows follows)
        {
            string stmtRef1 = follows.StmtRef.Value;
            string stmtRef2 = follows.StmtRef2.Value;
            if (stmtRef1 == "_" && stmtRef2 == "_")
            {
                throw new Exception("Relacja z podłogą nieobsługiwana!");
            }
            else if (stmtRef1 == "_")
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
                int.TryParse(stmtRef1, out firstRef);
                int.TryParse(stmtRef2, out secondRef);

                if(firstRef == 0 && secondRef == 0)
                {
                    //bool isFollowed = Pkb.IsFollowed(s)
                    //return Pkb.GetFollows().ConvertAll<string>(x => x.ToString());
                }
                else if (firstRef == 0)
                {
                    return Pkb.GetFollows(secondRef).ConvertAll<string>(x => x.ToString());
                }
                else if (secondRef == 0)
                {
                    return Pkb.GetFollowed(firstRef).ConvertAll<string>(x => x.ToString());
                }
                else
                {
                    bool isFollowed = Pkb.IsFollowed(firstRef, secondRef);
                    return new List<string>(new string[]{isFollowed.ToString()});
                }
            }
            throw new Exception("");
        }

        private List<string> UsesSRelation(UsesS usesS)
        {
            throw new NotImplementedException();
        }

        private List<string> ModifiesSRelation(ModifiesS modifiesS)
        {
            throw new NotImplementedException();
        }

        private List<string> ParentTRelation(ParentT parentT)
        {
            throw new NotImplementedException();
        }

        private List<string> ParentRelation(Parent parent)
        {
            throw new NotImplementedException();
        }

        private List<string> FollowsTRelation(FollowsT followsT)
        {
            throw new NotImplementedException();
        }

        // przypadek z samym With
        private List<string> SelectWith(VarType varType)
        {
            throw new Exception("With not implemented yet!");
        }

        // przypadek z relacją i z with
        private List<string> SelectRelationWith(VarType varType)
        {
            throw new Exception("Relation and With not implemented yet!");
        }



        



    }
}
