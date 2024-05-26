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
using System.Windows.Xps.Serialization;

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
        private Dictionary<string, string> declarationsMap;

        public QueryEvaluator(Query query, IPkb pkb) 
        {
            Pkb = pkb;
            if(query.SuchThatClause!=null)
                Relation = query.SuchThatClause.Relation;
            With = query.WithClause;
            synonym = query.Synonyms[0];
            declarationsMap = getDeclarations(query);
            query.Result = ExecuteQuery();
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
        private Dictionary<string,List<string>> ExecuteQuery()
        {
            Dictionary<string, List<string>> results = new Dictionary<string, List<string>>();

            foreach (var declaration in declarationsMap.Keys.ToArray())
            {
                results.Add(declaration, new List<string>());
            }

            if (Relation != null)
            {
                results = SelectRelation(Relation!, results);
            }

            if (With != null)
            {
                results = SelectWith(With! ,results);
            }

           return results;
        }

        // najprostszy przypadek bez relacji i bez with
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
        private Dictionary<string, List<string>> SelectRelation(Relation relation, Dictionary<string, List<string>> results)
        {
            return relation switch
            {
                var type when type is Follows => FollowsRelation((type as Follows)!, results),
                var type when type is FollowsT => FollowsTRelation((type as FollowsT)!, results),
                var type when type is Parent => ParentRelation((type as Parent)!, results),
                var type when type is ParentT => ParentTRelation((type as ParentT)!, results),
                var type when type is ModifiesS => ModifiesSRelation((type as ModifiesS)!, results),
                var type when type is UsesS => UsesSRelation((type as UsesS)!, results),
                _ => throw new Exception("Nieprawidłowa relacja!"),
            };
        }

        private Dictionary<string, List<string>> FollowsRelation(Follows follows, Dictionary<string, List<string>> results)
        {
            string leftRef = follows.leftStmtRef.Value;
            string rightRef = follows.rightStmtRef.Value;
            // tworze liste synonimow
            string[] keys = results.Keys.ToArray();

            int leftStmtNumber;
            int rightStmtNumber;

            if (leftRef == "_" && rightRef == "_")
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu
            }
            else if (keys.Contains(leftRef) && rightRef == "_")
            {
                List<int> stmtInt = Pkb.GetAllFollowed();
                stmtInt = IntersectByproductWithEntityList(leftRef,stmtInt);
                if (stmtInt.Count > 0)
                {
                    List<string> stmtString = stmtInt.ConvertAll<string>(x => x.ToString());
                    results[leftRef].AddRange(stmtString);
                }               
            }
            else if (leftRef == "_" && keys.Contains(rightRef))
            {
                List<int> stmtInt = Pkb.GetAllFollows();
                stmtInt = IntersectByproductWithEntityList(rightRef, stmtInt);
                if (stmtInt.Count > 0)
                {
                    List<string> stmtString = stmtInt.ConvertAll<string>(x => x.ToString());
                    results[rightRef].AddRange(stmtString);
                }
            }
            else if (keys.Contains(leftRef) && keys.Contains(rightRef))
            {
                List<int> stmtInt = Pkb.GetAllFollowed();
                stmtInt = IntersectByproductWithEntityList(leftRef, stmtInt);
                if (stmtInt.Count > 0)
                {
                    foreach (int stmtNumber in stmtInt)
                    {
                        List<int> followingStmt = new List<int> { Pkb.GetFollowed(stmtNumber) };
                        followingStmt = IntersectByproductWithEntityList(rightRef, followingStmt);
                        if (followingStmt.Count > 0)
                        {
                            results[leftRef].Add(stmtNumber.ToString());
                            results[rightRef].Add(followingStmt.First().ToString());
                        }
                    }
                }
            }
            else if (keys.Contains(leftRef) && int.TryParse(rightRef, out rightStmtNumber))
            {
                List<int> stmtInt = new List<int> { Pkb.GetFollows(rightStmtNumber) };
                stmtInt = IntersectByproductWithEntityList(leftRef, stmtInt);
                if (stmtInt.Count > 0)
                {
                    results[leftRef].Add(stmtInt.First().ToString());
                }
            }
            else if (int.TryParse(leftRef, out leftStmtNumber) && keys.Contains(rightRef))
            {
                if (Pkb.GetFollowed(leftStmtNumber) != -1)
                {
                    List<int> stmtInt = new List<int> { Pkb.GetFollowed(leftStmtNumber) };
                    stmtInt = IntersectByproductWithEntityList(rightRef, stmtInt);
                    if (stmtInt.Count > 0)
                    {                        
                        results[rightRef].Add(stmtInt.First().ToString());
                    }
                }
            }
            else if (int.TryParse(leftRef, out leftStmtNumber) && int.TryParse(rightRef, out rightStmtNumber))
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu
            }
            else if (int.TryParse(leftRef, out leftStmtNumber) && rightRef == "_")
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu
            }
            else if (leftRef == "_" && int.TryParse(rightRef, out rightStmtNumber))
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu 
            }
            return results;
        }

        private Dictionary<string, List<string>> UsesSRelation(UsesS usesS, Dictionary<string, List<string>> results)
        {
            string stmtRef = usesS.StmtRef.Value;
            string entRef = usesS.EntRef.Value;

            string[] keys = results.Keys.ToArray();

            int stmt;
            int ent;

            if (keys.Contains(stmtRef) && keys.Contains(entRef))
            {
                List<int> statements = Pkb.GetAllStatementsThatUseVariables();
                statements = IntersectByproductWithEntityList(stmtRef, statements);
                if (statements.Count > 0)
                {
                    foreach (int stmtNumber in statements)
                    {
                        List<string> usedVariables = Pkb.GetUsed(stmtNumber);
                        usedVariables = IntersectByproductWithEntityList(entRef,usedVariables);
                        if (usedVariables.Count > 0)
                        {
                            foreach (string variable in usedVariables)
                            {
                                results[stmtRef].Add(stmtNumber.ToString());
                                results[entRef].Add(variable);
                            }
                        }
                    }
                }
            }
            else if (keys.Contains(stmtRef) && entRef == "_")
            {
                List<int> statements = Pkb.GetAllStatementsThatUseVariables();
                statements = IntersectByproductWithEntityList(stmtRef, statements);
                if (statements.Count > 0)
                {
                    List<string> stmtString = statements.ConvertAll<string>(x => x.ToString());
                    results[stmtRef].AddRange(stmtString);
                }
            }
            else if (keys.Contains(stmtRef) && entRef.StartsWith("\""))
            {
                string variable = entRef.Substring(1,entRef.Length-2);
                List<int> statements = Pkb.GetUses(variable);
                statements = IntersectByproductWithEntityList(stmtRef, statements);
                if (statements.Count > 0)
                {
                    List<string> stmtString = statements.ConvertAll<string>(x => x.ToString());
                    results[stmtRef].AddRange(stmtString);
                }
            }
            else if (int.TryParse(stmtRef, out stmt) && keys.Contains(entRef))
            {
                List<string> variables = Pkb.GetUsed(stmt);
                variables = IntersectByproductWithEntityList(entRef, variables);
                if (variables.Count > 0)
                {
                    results[entRef].AddRange(variables);
                }
            }
            else if (int.TryParse(stmtRef, out stmt) && entRef.StartsWith("\""))
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu 
            }
            else if (int.TryParse(stmtRef, out stmt) && entRef == "_")
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu 
            }

            return results;
        }

        private Dictionary<string, List<string>> ModifiesSRelation(ModifiesS modifiesS, Dictionary<string, List<string>> results)
        {
            string stmtRef = modifiesS.StmtRef.Value;
            string entRef = modifiesS.EntRef.Value;

            string[] keys = results.Keys.ToArray();

            int stmt;
            int ent;

            if (keys.Contains(stmtRef) && keys.Contains(entRef))
            {
                List<int> statements = Pkb.GetAllStatementsThatModifieVariables();
                statements = IntersectByproductWithEntityList(stmtRef, statements);
                if (statements.Count > 0)
                {
                    foreach (int stmtNumber in statements)
                    {
                        List<string> modifiedVariables = Pkb.GetModified(stmtNumber);
                        modifiedVariables = IntersectByproductWithEntityList(entRef, modifiedVariables);
                        if (modifiedVariables.Count > 0)
                        {
                            foreach (string variable in modifiedVariables)
                            {
                                results[stmtRef].Add(stmtNumber.ToString());
                                results[entRef].Add(variable);
                            }
                        }
                    }
                }
            }
            else if (keys.Contains(stmtRef) && entRef == "_")
            {
                List<int> statements = Pkb.GetAllStatementsThatModifieVariables();
                statements = IntersectByproductWithEntityList(stmtRef, statements);
                if (statements.Count > 0)
                {
                    List<string> stmtString = statements.ConvertAll<string>(x => x.ToString());
                    results[stmtRef].AddRange(stmtString);
                }
            }
            else if (keys.Contains(stmtRef) && entRef.StartsWith("\""))
            {
                string variable = entRef.Substring(1, entRef.Length - 2);
                List<int> statements = Pkb.GetModifies(variable);
                statements = IntersectByproductWithEntityList(stmtRef, statements);
                if (statements.Count > 0)
                {
                    List<string> stmtString = statements.ConvertAll<string>(x => x.ToString());
                    results[stmtRef].AddRange(stmtString);
                }
            }
            else if (int.TryParse(stmtRef, out stmt) && keys.Contains(entRef))
            {
                List<string> variables = Pkb.GetModified(stmt);
                variables = IntersectByproductWithEntityList(entRef, variables);
                if (variables.Count > 0)
                {
                    results[entRef].AddRange(variables);
                }
            }
            else if (int.TryParse(stmtRef, out stmt) && entRef.StartsWith("\""))
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu 
            }
            else if (int.TryParse(stmtRef, out stmt) && entRef == "_")
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu 
            }

            return results;
        }

        private Dictionary<string, List<string>> ParentTRelation(ParentT parentT, Dictionary<string, List<string>> results)
        {
            string parentRef = parentT.StmtRef.Value;
            string childRef = parentT.StmtRef2.Value;

            string[] keys = results.Keys.ToArray();

            int parentStmtNumber;
            int childStmtNumber;

            if (parentRef == "_" && childRef == "_")
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu 
            }
            else if (keys.Contains(parentRef) && childRef == "_")
            {
                List<int> parentsInts = Pkb.GetAllParents();
                parentsInts = IntersectByproductWithEntityList(parentRef, parentsInts);
                if (parentsInts.Count > 0)
                {
                    List<string> parentStrings = parentsInts.ConvertAll<string>(x => x.ToString());
                    results[parentRef].AddRange(parentStrings);
                }
            }
            else if (parentRef == "_" && keys.Contains(childRef))
            {
                List<int> childrenInts = Pkb.GetAllChildren();
                childrenInts = IntersectByproductWithEntityList(childRef, childrenInts);
                if (childrenInts.Count > 0)
                {
                    List<string> childrenStrings = childrenInts.ConvertAll<string>(x => x.ToString());
                    results[childRef].AddRange(childrenStrings);
                }
            }
            else if (keys.Contains(parentRef) && keys.Contains(childRef))
            {
                List<int> parents = Pkb.GetAllParents();
                parents = IntersectByproductWithEntityList(parentRef, parents);
                if (parents.Count > 0)
                {
                    foreach (int parentStmt in parents)
                    {
                        List<int> children = Pkb.GetChildren(parentStmt);
                        children = IntersectByproductWithEntityList(childRef, children);
                        if (children.Count > 0)
                        {
                            foreach (int childstmt in children)
                            {
                                results[childRef].Add(childstmt.ToString());
                                results[parentRef].Add(parentStmt.ToString());
                                results = recursiveChildrenAndParentSearch(parentRef, parentStmt, childRef, childstmt, results);
                            }
                        }
                    }
                }
            }
            else if (keys.Contains(parentRef) && int.TryParse(childRef, out childStmtNumber))
            {
                if (Pkb.GetParent(childStmtNumber) != -1)
                {
                    List<int> parentInts = new List<int> { Pkb.GetParent(childStmtNumber) };
                    parentInts = IntersectByproductWithEntityList(parentRef, parentInts);
                    if (parentInts.Count > 0)
                    {
                        results[parentRef].Add(parentInts.First().ToString());
                        results = recursiveParentSearch(parentRef, parentInts.First(), results);
                    }
                }
            }
            else if (int.TryParse(parentRef, out parentStmtNumber) && keys.Contains(childRef))
            {
                if (Pkb.GetChildren(parentStmtNumber).Count > 0)
                {
                    List<int> childrenInts = Pkb.GetChildren(parentStmtNumber);
                    childrenInts = IntersectByproductWithEntityList(childRef, childrenInts);
                    if (childrenInts.Count > 0)
                    {
                        foreach (int childstmt in childrenInts)
                        {
                            results[childRef].Add(childstmt.ToString());
                            results = recursiveChildrenSearch(childRef, childstmt, results);
                        }
                    }
                }
            }
            return results;
        }
        private Dictionary<string, List<string>> recursiveParentSearch(string parentRef, int childStmtNumber, Dictionary<string, List<string>> results)
        {
            List<int>  parentInts = new List<int> { Pkb.GetParent(childStmtNumber) };
            parentInts = IntersectByproductWithEntityList(parentRef, parentInts);
            if (parentInts.Count > 0)
            {
                results[parentRef].Add(parentInts.First().ToString());
                results = recursiveParentSearch(parentRef, parentInts.First(), results);
            }

            return results;
        }

        private Dictionary<string, List<string>> recursiveChildrenAndParentSearch(string parentRef, int parent, string childRef, int stmt, Dictionary<string, List<string>> results)
        {
            List<int> childrenInts = Pkb.GetChildren(stmt);
            childrenInts = IntersectByproductWithEntityList(childRef, childrenInts);
            if (childrenInts.Count > 0)
            {
                foreach (int childstmt in childrenInts)
                {
                    results[childRef].Add(childstmt.ToString());
                    results[parentRef].Add(parent.ToString());
                    results = recursiveChildrenAndParentSearch(parentRef, parent, childRef, childstmt, results);
                }
            }

            return results;
        }

        private Dictionary<string, List<string>> recursiveChildrenSearch(string childRef, int stmt, Dictionary<string, List<string>> results)
        {
            List<int> childrenInts = Pkb.GetChildren(stmt);
            childrenInts = IntersectByproductWithEntityList(childRef, childrenInts);
            if (childrenInts.Count > 0)
            {
                foreach (int childstmt in childrenInts)
                {
                    results[childRef].Add(childstmt.ToString());
                    results = recursiveChildrenSearch(childRef, childstmt, results);
                }
            }

                return results;
        }

        private Dictionary<string, List<string>> ParentRelation(Parent parent, Dictionary<string, List<string>> results)
        {
            string parentRef = parent.StmtRef.Value;
            string childRef = parent.StmtRef2.Value;

            string[] keys = results.Keys.ToArray();

            int parentStmtNumber;
            int childStmtNumber;

            if (parentRef == "_" && childRef == "_")
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu 
            }
            else if (keys.Contains(parentRef) && childRef == "_")
            {
                List<int> parentsInts = Pkb.GetAllParents();
                parentsInts = IntersectByproductWithEntityList(parentRef, parentsInts);
                if (parentsInts.Count > 0)
                {
                    List<string> parentStrings = parentsInts.ConvertAll<string>(x => x.ToString());
                    results[parentRef].AddRange(parentStrings);
                }
            }
            else if (parentRef == "_" && keys.Contains(childRef))
            {
                List<int> childrenInts = Pkb.GetAllChildren();
                childrenInts = IntersectByproductWithEntityList(childRef, childrenInts);
                if (childrenInts.Count > 0) { 
                    List<string> childrenStrings = childrenInts.ConvertAll<string>(x => x.ToString());
                    results[childRef].AddRange(childrenStrings);
                }
            }
            else if (keys.Contains(parentRef) && keys.Contains(childRef))
            {
                List<int> parents = Pkb.GetAllParents();
                parents = IntersectByproductWithEntityList(parentRef,parents);
                if (parents.Count > 0)
                {
                    foreach (int parentStmt in parents)
                    {
                        List<int> children = Pkb.GetChildren(parentStmt);
                        children = IntersectByproductWithEntityList(childRef,children);
                        if (children.Count > 0)
                        {
                            foreach (int childstmt in children)
                            {
                                results[childRef].Add(childstmt.ToString());
                                results[parentRef].Add(parentStmt.ToString());
                            }
                        }
                    }
                }
            }
            else if (keys.Contains(parentRef) && int.TryParse(childRef, out childStmtNumber))
            {
                if (Pkb.GetParent(childStmtNumber) != -1)
                {
                    List<int> parentInts = new List<int> { Pkb.GetParent(childStmtNumber) };
                    parentInts = IntersectByproductWithEntityList(parentRef, parentInts);
                    if (parentInts.Count > 0)
                    {
                        results[parentRef].Add(parentInts.First().ToString());
                    }
                }
            }
            else if (int.TryParse(parentRef, out parentStmtNumber) && keys.Contains(childRef))
            {
                if (Pkb.GetChildren(parentStmtNumber).Count > 0)
                {
                    List<int> childrenInts = Pkb.GetChildren(parentStmtNumber);
                    childrenInts = IntersectByproductWithEntityList(childRef, childrenInts);
                    if (childrenInts.Count > 0) { 
                        List<string> childrenStrings = childrenInts.ConvertAll<string>(x => x.ToString());
                        results[childRef].AddRange(childrenStrings);
                    }
                }               
            }
            else if (parentRef == "_" && int.TryParse(childRef, out childStmtNumber))
            {
                //  to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu 
            }
            else if (int.TryParse(parentRef, out parentStmtNumber) && childRef == "_")
            {
                //  to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu 
            }
            else if (int.TryParse(parentRef, out parentStmtNumber) && int.TryParse(childRef, out childStmtNumber))
            {
                //  to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu 
            }
            return results;
        }

        private Dictionary<string, List<string>> FollowsTRelation(FollowsT followsT, Dictionary<string, List<string>> results)
        {
            string leftRef = followsT.leftStmtRef.Value;
            string rightRef = followsT.rightStmtRef.Value;
            // tworze liste synonimow
            string[] keys = results.Keys.ToArray();

            int leftStmtNumber;
            int rightStmtNumber;
            if (leftRef == "_" && rightRef == "_")
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu 
            }
            else if (keys.Contains(leftRef) && keys.Contains(rightRef))
            {
                List<int> stmtInt = Pkb.GetAllFollowed();
                stmtInt = IntersectByproductWithEntityList(leftRef, stmtInt);
                if (stmtInt.Count > 0)
                {
                    foreach (int stmtNumber in stmtInt)
                    {
                        List<int> followingStmt = new List<int> { Pkb.GetFollowed(stmtNumber) };
                        followingStmt = IntersectByproductWithEntityList(rightRef, followingStmt);
                        if (followingStmt.Count > 0)
                        {
                            results[leftRef].Add(stmtNumber.ToString());
                            results[rightRef].Add(followingStmt.First().ToString());
                            while (Pkb.GetFollowed(followingStmt.First()) != -1) 
                            {
                                followingStmt = new List<int> { Pkb.GetFollowed(followingStmt.First()) };
                                followingStmt = IntersectByproductWithEntityList(rightRef, followingStmt);
                                if (followingStmt.Count > 0)
                                {
                                    results[leftRef].Add(stmtNumber.ToString());
                                    results[rightRef].Add(followingStmt.First().ToString());
                                }
                            }
                        }
                    }
                }
            }
            else if (keys.Contains(leftRef) && rightRef == "_")
            {
                List<int> stmtInt = Pkb.GetAllFollowed();
                stmtInt = IntersectByproductWithEntityList(leftRef, stmtInt);
                if (stmtInt.Count > 0)
                {
                    List<string> stmtString = stmtInt.ConvertAll<string>(x => x.ToString());
                    results[leftRef].AddRange(stmtString);
                }
            }
            else if (leftRef == "_" && keys.Contains(rightRef))
            {
                List<int> stmtInt = Pkb.GetAllFollows();
                stmtInt = IntersectByproductWithEntityList(rightRef, stmtInt);
                if (stmtInt.Count > 0)
                {
                    List<string> stmtString = stmtInt.ConvertAll<string>(x => x.ToString());
                    results[rightRef].AddRange(stmtString);
                }
            }
            else if (keys.Contains(leftRef) && int.TryParse(rightRef, out rightStmtNumber))
            {
                List<int> stmtInt = new List<int> { Pkb.GetFollows(rightStmtNumber) };
                stmtInt = IntersectByproductWithEntityList(leftRef, stmtInt);
                if (stmtInt.Count > 0)
                {
                    results[leftRef].Add(stmtInt.First().ToString());
                    while (Pkb.GetFollows(stmtInt.First()) != -1)
                    {
                        stmtInt = new List<int> { Pkb.GetFollows(stmtInt.First()) };
                        stmtInt = IntersectByproductWithEntityList(leftRef, stmtInt);
                        if (stmtInt.Count > 0)
                        {
                            results[leftRef].Add(stmtInt.First().ToString());
                        }
                    }
                }
            }
            else if (int.TryParse(leftRef, out leftStmtNumber) && keys.Contains(rightRef))
            {
                if (Pkb.GetFollowed(leftStmtNumber) != -1)
                {
                    List<int> stmtInt = new List<int> { Pkb.GetFollowed(leftStmtNumber) };
                    stmtInt = IntersectByproductWithEntityList(rightRef, stmtInt);
                    if (stmtInt.Count > 0)
                    {
                        results[rightRef].Add(stmtInt.First().ToString());
                        while (Pkb.GetFollowed(stmtInt.First()) != -1)
                        {
                            stmtInt = new List<int> { Pkb.GetFollowed(leftStmtNumber) };
                            stmtInt = IntersectByproductWithEntityList(rightRef, stmtInt);
                            if (stmtInt.Count > 0)
                            {
                                results[rightRef].Add(stmtInt.First().ToString());
                            }
                        }
                    }
                }
            }
            else if (int.TryParse(leftRef, out leftStmtNumber) && int.TryParse(rightRef, out rightStmtNumber))
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu
            }
            else if (int.TryParse(leftRef, out leftStmtNumber) && rightRef == "_")
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu
            }
            else if (leftRef == "_" && int.TryParse(rightRef, out rightStmtNumber))
            {
                // to sprawdza tylko cos sensowengo jak jest boolean w zapytaniu 
            }
            return results;
        }

        // przypadek z samym With
        private Dictionary<string, List<string>> SelectWith(With with, Dictionary<string, List<string>> results)
        {
            string AttrName = with.AttrName;

            if (AttrName == "varName")
            {
                //string variable = with.Value;
                //Dictionary<string, List<string>> FilteredResults = new Dictionary<string, List<string>>();
            
                //foreach (string key in results.Keys)
                //{
                //    FilteredResults.Add(key, new List<string>());
                //}
                //foreach (string varInResults in results[with.Synonym])
                //{
                //    if (varInResults == variable)
                //    {
                        
                //    }
                //}
                //results = FilteredResults;
          
            }
            else if (AttrName == "value")
            {
                
            }
            else if (AttrName == "stmt#")
            {
                
            }
            else if (AttrName == "procName")
            {
                 
            }

            return results;
        }

        private List<T> GetStatementLines<T>(VarType varType)
        {
            int programLength = Pkb.GetProgramLength();

            if (typeof(T) == typeof(int))
            {
                return varType switch
                {
                    var type when type == VarType.STMT => new List<T>(Enumerable.Range(1, programLength).Cast<T>().ToList()),
                    var type when type == VarType.ASSIGN => Pkb.GetAssigns().Cast<T>().ToList(),
                    var type when type == VarType.PROG_LINE => new List<T>(Enumerable.Range(1, programLength).Cast<T>().ToList()),
                    var type when type == VarType.WHILE => Pkb.GetWhiles().Cast<T>().ToList(),
                    _ => throw new Exception("Błąd składniowy!"),
                };
            }
            else if (typeof(T) == typeof(string))
            {
                return varType switch
                {
                    var type when type == VarType.VARIABLE => Pkb.GetVariables().Cast<T>().ToList(),
                    var type when type == VarType.CONSTANT => Pkb.GetConstants().Cast<T>().ToList(),
                    _ => throw new Exception("Błąd składniowy!"),
                };
            }
            else
            {
                throw new Exception("Unsupported type!");
            }
        }

        private List<T> IntersectByproductWithEntityList<T>(string synonym, List<T> results)
        {
            string synonymType = declarationsMap[synonym];
            VarType varType = VarType.GetVarType(synonymType);
            List<T> designEntities = GetStatementLines<T>(varType);
            var commonPart = designEntities.Intersect(results);
            results = commonPart.ToList();
            return results;
        }
    }
}
