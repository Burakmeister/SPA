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

        // robię słownik deklaracji jako klucze z ich nazwami i jako wartości typami
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
            return varType switch
            {
                var type when type == VarType.STMT => new List<string>(new string[] { "1", "-", programLength.ToString() }),
                var type when type == VarType.ASSIGN => Pkb.GetAssigns().ConvertAll<string>(x => x.ToString()),
                var type when type == VarType.VARIABLE => Pkb.GetVariables(),
                var type when type == VarType.PROG_LINE => new List<string>(new string[] { "1", "-", programLength.ToString() }),
                var type when type == VarType.CONSTANT => Pkb.GetConstants().ConvertAll<string>(x => x.ToString()),
                var type when type == VarType.WHILE => Pkb.GetWhiles().ConvertAll<string>(x => x.ToString()),
                _ => throw new Exception("Niepoprawny typ zmiennej!"),
            };
        }

        // przypadek z relacją bez with
        private List<string> SelectRelation(VarType varType, Relation relation)
        {
            return relation switch
            {
                var type when type is Follows => FollowsRelation((type as Follows)!, varType),
                var type when type is FollowsT => FollowsTRelation((type as FollowsT)!),
                var type when type is Parent => ParentRelation((type as Parent)!, varType),
                var type when type is ParentT => ParentTRelation((type as ParentT)!),
                var type when type is ModifiesS => ModifiesSRelation((type as ModifiesS)!),
                var type when type is UsesS => UsesSRelation((type as UsesS)!, varType),
                _ => throw new Exception("Nieprawidłowa relacja!"),
            };
        }

        // relacja follow (chyba działa)
        private List<string> FollowsRelation(Follows follows, VarType varType)
        {
            string stmtRef1 = follows.StmtRef.Value;
            string stmtRef2 = follows.StmtRef2.Value;

            List<int> lines = GetStatementLines(varType);

            if (stmtRef1 == "_" && stmtRef2 == "_")
            {
                List<string> retList = new();
                int programLength = Pkb.GetProgramLength();
                for (int i = 1; i < programLength; i++)
                {
                    foreach (int line in Pkb.GetFollows(i))
                    {
                        retList.Add("(" + i.ToString() + ", " + line.ToString() + ") ");
                    }
                }
                return retList;
            }
            else if (stmtRef1 == "_")
            {
                List<string> retList = new();
                int secondRef;
                int.TryParse(stmtRef2, out secondRef);
                if (secondRef == 0)
                {
                    List<int> followsList = GetAllFollows();
                    return followsList.Intersect(lines).ToList().ConvertAll<string>(x => x.ToString());
                }
                else
                {
                    List<int> tmpFollows = Pkb.GetFollows(secondRef);
                    if (tmpFollows.Count > 0)
                    {
                        return tmpFollows.ConvertAll<string>(x => x.ToString());
                    }
                    return new List<string>();
                }
            }
            else if (stmtRef2 == "_")
            {
                List<string> retList = new();
                int secondRef;
                int.TryParse(stmtRef2, out secondRef);
                if (secondRef == 0)
                {
                    List<int> followedList = GetAllFollowed();
                    return followedList.Intersect(lines).ToList().ConvertAll<string>(x => x.ToString());
                }
                else
                {
                    List<int> tmpFollowed = Pkb.GetFollowed(secondRef);
                    if (tmpFollowed.Count > 0)
                    {
                        return tmpFollowed.ConvertAll<string>(x => x.ToString());
                    }
                    return new List<string>();
                }
            }
            else
            {
                int firstRef;
                int secondRef;
                int.TryParse(stmtRef1, out firstRef);
                int.TryParse(stmtRef2, out secondRef);

                if(firstRef == 0 && secondRef == 0)
                {
                    List<string> retList = new();
                    int programLength = Pkb.GetProgramLength();
                    for (int i=1; i<programLength; i++)
                    {
                        foreach(int line in Pkb.GetFollows(i))
                        {
                            retList.Add("("+line.ToString()+", " + i.ToString() + ") ");
                        }
                    }
                    return retList;
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
        }

        private List<int> GetAllFollows() 
        {
            int linesNumber = Pkb.GetProgramLength();
            List<int> toRet = new();
            for(int i=1; i<linesNumber; i++)
            {
                if(Pkb.GetFollows(i).Count>0)
                    toRet.Add(Pkb.GetFollows(i)[0]);
            }
            return toRet;
        }

        private List<int> GetAllParents()
        {
            int linesNumber = Pkb.GetProgramLength();
            List<int> toRet = new();
            for (int i = 1; i < linesNumber; i++)
            {
                if (Pkb.GetParent(i) != 0)
                    toRet.Add(Pkb.GetParent(i));
            }
            return toRet;
        }

        private List<int> GetAllChildren()
        {
            int linesNumber = Pkb.GetProgramLength();
            List<int> toRet = new();
            for (int i = 1; i < linesNumber; i++)
            {
                if (Pkb.GetChildren(i).Count > 0)
                    toRet.Add(Pkb.GetChildren(i)[0]);
            }
            return toRet;
        }

        private List<int> GetAllFollowed() {
            int linesNumber = Pkb.GetProgramLength();
            List<int> toRet = new();
            for (int i = 1; i < linesNumber; i++)
            {
                if (Pkb.GetFollowed(i).Count > 0)
                    toRet.Add(Pkb.GetFollowed(i)[0]);
            }
            return toRet;
        }

        private List<string> UsesSRelation(UsesS usesS, VarType varType)
        {
            string stmtRef = usesS.StmtRef.Value;
            string entRef = usesS.EntRef.Value;

            List<int> lines = GetStatementLines(varType);

            int stmt;
            int.TryParse(stmtRef, out stmt);

            if (entRef == "_")
            {
                int programLength = Pkb.GetProgramLength();
                if (stmt == 0)
                {
                    List<string> retList = new();
                    for (int i = 1; i < programLength; i++)
                    {
                        foreach (Variable var in Pkb.GetUsed(i))
                        {
                            retList.Add("(" + i.ToString() + ", " + var.VarName + ") ");
                        }
                    }
                    return retList;
                }
                else
                {
                    return Pkb.GetUsed(stmt).ConvertAll<string>(x => x.VarName);
                }
            }
            else if (stmt == 0)
            {
                List<string> retList = new();
                return retList;
            } else if (stmtRef == "_")
            {
                throw new Exception("First argument of Uses cannot be \"_\"");
            }
            throw new Exception("Something wrong!");
        }

        private List<string> ModifiesSRelation(ModifiesS modifiesS)
        {
            throw new NotImplementedException();
        }

        private List<string> ParentTRelation(ParentT parentT)
        {
            throw new NotImplementedException();
        }

        // prawie działa
        private List<string> ParentRelation(Parent parent, VarType varType)
        {
            string stmtRef1 = parent.StmtRef.Value;
            string stmtRef2 = parent.StmtRef2.Value;

            List<int> lines = GetStatementLines(varType);

            if (stmtRef1 == "_" && stmtRef2 == "_")
            {
                List<string> retList = new();
                int programLength = Pkb.GetProgramLength();
                for (int i = 1; i < programLength; i++)
                {
                    retList.Add("(" + i.ToString() + ", " + Pkb.GetParent(i).ToString() + ") ");
                }
                return retList;
            }
            else if (stmtRef1 == "_")
            {
                List<string> retList = new();
                int secondRef;
                int.TryParse(stmtRef2, out secondRef);
                if (secondRef == 0)
                {
                    List<int> childrenList = GetAllChildren();
                    return childrenList.Intersect(lines).ToList().ConvertAll<string>(x => x.ToString());
                }
                else
                {
                    List<int> childrenList = Pkb.GetChildren(secondRef);
                    if (childrenList.Count > 0)
                    {
                        return childrenList.ConvertAll<string>(x => x.ToString());
                    }
                    return new List<string>();
                }
            }
            else if (stmtRef2 == "_")
            {
                List<string> retList = new();
                int secondRef;
                int.TryParse(stmtRef2, out secondRef);
                if (secondRef == 0)
                {
                    List<int> parentsList = GetAllParents();
                    return parentsList.Intersect(lines).ToList().ConvertAll<string>(x => x.ToString());
                }
                else
                {
                    int tmpFollowed = Pkb.GetParent(secondRef);
                    if (tmpFollowed != 0)
                    {
                        return new List<string> (new string[] { tmpFollowed.ToString() });
                    }
                    return new List<string>();
                }
            }
            else
            {
                int firstRef;
                int secondRef;
                int.TryParse(stmtRef1, out firstRef);
                int.TryParse(stmtRef2, out secondRef);

                if (firstRef == 0 && secondRef == 0)
                {
                    List<string> retList = new();
                    int programLength = Pkb.GetProgramLength();
                    for (int i = 1; i < programLength; i++)
                    {
                        foreach (int line in Pkb.GetChildren(i))
                        {
                            retList.Add("(" + line.ToString() + ", " + i.ToString() + ") ");
                        }
                    }
                    return retList;
                }
                else if (firstRef == 0)
                {
                    return Pkb.GetChildren(secondRef).ConvertAll<string>(x => x.ToString());
                }
                else if (secondRef == 0)
                {
                    return Pkb.GetChildren(firstRef).ConvertAll<string>(x => x.ToString());
                }
                else
                {
                    bool isFollowed = Pkb.IsParent(firstRef, secondRef);
                    return new List<string>(new string[] { isFollowed.ToString() });
                }
            }
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

        private List<int> GetStatementLines(VarType varType)
        {
            int programLength = Pkb.GetProgramLength();
            return varType switch
            {
                var type when type == VarType.STMT => new List<int>(Enumerable.Range(1, programLength).ToArray()),
                var type when type == VarType.ASSIGN => Pkb.GetAssigns(),
                var type when type == VarType.PROG_LINE => new List<int>(Enumerable.Range(1, programLength).ToArray()),
                var type when type == VarType.WHILE => Pkb.GetWhiles(),
                _ => throw new Exception("Błąd składniowy!"),
            };
        }
    }
}
