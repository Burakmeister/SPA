using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SPA.QueryProcessor
{
    internal class QueryPreprocessor
    {
        // Klasa sprawdza poprawność zapytania i tworzy jego drzewo

        // Table-driven approach dla tablicy relacji
        private Dictionary<string, (int, TokenType[], TokenType[],
            Func<StmtRef, StmtRef, Relation> createStmtStmtInstance,
            Func<StmtRef, EntRef, Relation> createStmtEntInstance)> relTable;

        // Table-driven approach dla tablicy design entities
        private Dictionary<string, string> entTable;

        private string query;
        private Query _query;

        private int position = 0;   // pozycja analizatora parsera
        private string[] parts;

        public QueryPreprocessor(string query, Query _query)
        {
            relTable = new Dictionary<string, (int, TokenType[], TokenType[],
                Func<StmtRef, StmtRef, Relation> createStmtStmtInstance,
                Func<StmtRef, EntRef, Relation> createStmtEntInstance)>
            {
                { "Follows",
                    (2, new[] { TokenType.IDENT, TokenType.INTEGER }, new[] { TokenType.IDENT, TokenType.INTEGER },
                    (arg1, arg2) => new Follows { StmtRef = arg1, StmtRef2 = arg2 },
                    null)
                },
                { "Follows*",
                    (2, new[] { TokenType.IDENT, TokenType.INTEGER }, new[] { TokenType.IDENT, TokenType.INTEGER },
                    (arg1, arg2) => new FollowsT { StmtRef = arg1, StmtRef2 = arg2 },
                    null)
                },
                { "Parent", 
                    (2, new[] { TokenType.IDENT, TokenType.INTEGER }, new[] { TokenType.IDENT, TokenType.INTEGER },
                    (arg1, arg2) => new Parent { StmtRef = arg1, StmtRef2 = arg2 },
                    null)
                },
                { "Parent*",
                    (2, new[] { TokenType.IDENT, TokenType.INTEGER }, new[] { TokenType.IDENT, TokenType.INTEGER },
                    (arg1, arg2) => new ParentT { StmtRef = arg1, StmtRef2 = arg2 },
                    null)
                },
                { "Modifies",
                    (2, new[] { TokenType.IDENT, TokenType.INTEGER }, new[] { TokenType.IDENT, TokenType.IDENT },
                    null,
                    (arg1, arg2) => new ModifiesS { StmtRef = arg1, EntRef = arg2 })
                },
                { "Uses",
                    (2, new[] { TokenType.IDENT, TokenType.INTEGER }, new[] { TokenType.IDENT, TokenType.IDENT },
                    null,
                    (arg1, arg2) => new UsesS { StmtRef = arg1, EntRef = arg2 })
                }

            };

            entTable = new Dictionary<string, string>
            {
                { "procedure", "procName" },
                { "variable", "varName" },
                { "constant", "value" },
                { "stmt", "stmt#" },
                { "call", "procName" }
            };

            this.query = query;
            this._query = _query;

            ValidateQuery();
        }

        private void ValidateQuery()
        {
            // Zapytanie od razu dzielimy na tokeny, aby się pozbyć problemu z białymi znakami
            string pattern = @"(\s+|;|\.|\(|\)|,|=)"; // separatory
            parts = Regex.Split(query, pattern);
            parts = parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray(); // usuń puste elementy

            _query.Declarations = ValidateDeclarations();
            Match("Select");
            _query.Synonym = ValidateSynonyms();

            // Sprawdź dwa następne tokeny
            if (Peek(1) == "such" && Peek(2) == "that") 
            {
                Advance(2);
                _query.SuchThatClause = ValidateSuchThatClause();
            }

            if (Peek(1) == "with")
            {
                Advance();
                _query.WithClause = ValidateWithClause();
            }

            QueryValidator();

            Console.WriteLine("Query is valid");
        }

        private List<Declaration> ValidateDeclarations()
        {
            List<Declaration> declarations = new List<Declaration>();

            while (Peek() != "Select")
            {
                string designEntity = MatchDesignEntity().Value;
                List<string> synonyms = ValidateSynonyms();

                Declaration declaration = new Declaration
                {
                    DesignEntity = designEntity,
                    Synonyms = synonyms
                };

                declarations.Add(declaration);

                if (Peek() != ";")
                {
                    throw new Exception("Expected ;");
                }

                Advance();
            }

            return declarations;
        }

        private List<string> ValidateSynonyms()
        {
            List<string> synonyms = new List<string>();
            synonyms.Add(Match(TokenType.IDENT).Value);

            // W przypadku kolejnych synonyms:
            while (Peek() == ",")
            {
                Advance();
                synonyms.Add(Match(TokenType.IDENT).Value);
            }

            return synonyms;
        }

        private SuchThat ValidateSuchThatClause()
        {
            SuchThat clause = new SuchThat();
            clause.Relation = ValidateRelation();
            return clause;
        }

        private Relation ValidateRelation()
        {
            Token relToken = PeekToken();

            // Sprawdź, czy relacja istnieje
            if (relTable.ContainsKey(relToken.Value))
            {
                string relType = relToken.Value;
                // Pobranie informacji o danej relacji
                var (numArgs, argTypes, argTypes2, createStmtStmtInstance, createStmtEntInstance) = relTable[relToken.Value];

                Match(relToken.Value);
                Match(TokenType.SYMBOL, "(");

                // Sprawdź, czy pierwszy argument zgadza się z definicją
                Token[] argsTokens = new Token[numArgs];
                argsTokens[0] = MatchArgument(argTypes, relType, true);

                Match(TokenType.SYMBOL, ",");

                // Sprawdź, czy drugi argument zgadza się z definicją
                argsTokens[1] = MatchArgument(argTypes2, relType, false);

                Match(TokenType.SYMBOL, ")");

                StmtRef stmtRef1 = new StmtRef { Value = argsTokens[0].Value };
                
                // Wybór odpowiedniej funkcji w zależności od relacji
                if (createStmtStmtInstance != null)
                {
                    StmtRef stmtRef2 = new StmtRef { Value = argsTokens[1].Value };
                    return createStmtStmtInstance(stmtRef1, stmtRef2);
                }
                else if (createStmtEntInstance != null)
                {
                    EntRef entRef = new EntRef { Value = argsTokens[1].Value };
                    return createStmtEntInstance(stmtRef1, entRef);
                }
                else
                {
                    throw new Exception("Invalid relationship function");
                }
            }
            else
            {
                throw new Exception("Invalid relationship");
            }
        }

        private With ValidateWithClause()
        {
            With clause = new With();
            clause.Synonym = Match(TokenType.IDENT).Value;
            Match(TokenType.SYMBOL, ".");
            clause.AttrName = MatchAttrName();
            Match(TokenType.SYMBOL, "=");
            clause.Value = MatchValue(clause.AttrName);
            return clause;
        }

        private Token MatchDesignEntity()
        {
            string[] validDesignEntities = { "stmt", "assign", "while", "variable", "constant", "prog_line", 
                                             "call", "procedure", "if" };
            Token token = Match(TokenType.IDENT);

            if (!validDesignEntities.Contains(token.Value))
            {
                throw new Exception($"Invalid design entity: {token.Value}");
            }

            return token;
        }

        private Token Match(params object[] expectedValues)
        {
            Token token = PeekToken();

            // Sprawdzenie czy wartość bieżącego tokena
            // jest zgodna z oczekiwaną:
            if (expectedValues.Any(ev => ev is string))
            {
                string expectedValue = (string)expectedValues.First(ev => ev is string);
                if (token.Value != expectedValue)
                {
                    throw new Exception($"Expected '{expectedValue}', but got '{token.Value}'");
                }
            }

            // Sprawdzenie czy typ bieżącego tokena
            // jest zgodny z oczekiwanym:
            if (expectedValues.Any(ev => ev is TokenType))
            {
                TokenType expectedType = (TokenType)expectedValues.First(ev => ev is TokenType);
                if (token.Type != expectedType)
                {
                    throw new Exception($"Expected {expectedType}, but got {token.Type}");
                }
            }

            Advance();
            return token;
        }

        private string MatchAttrName()
        {
            string[] validAttrNames = { "procName", "varName", "value", "stmt#" };
            Token token = Match(TokenType.IDENT);

            if (!validAttrNames.Contains(token.Value))
            {
                throw new Exception($"Invalid attribute name: {token.Value}");
            }

            return token.Value;
        }

        private string MatchValue(string attribute)
        {
            // Służy do sprawdzenia ref w klauzuli with
            Token token = PeekToken();
            if (token.Type == TokenType.IDENT || token.Type == TokenType.INTEGER)
            {
                // Sprawdź, czy dla danego atrybutu synonimu jest odpowiedni typ wartości (IDENT LUB INTEGER)
                Dictionary<string, TokenType> attrValues = new Dictionary<string, TokenType>
                {
                    { "procName", TokenType.IDENT },
                    { "varName", TokenType.IDENT },
                    { "value", TokenType.INTEGER },
                    { "stmt#", TokenType.INTEGER }
                };

                attrValues.TryGetValue(attribute, out TokenType expectedType);
                if(token.Type != expectedType)
                {
                    throw new Exception($"Invalid value type {token.Type} for attribute {attribute}");
                }

                Advance();
                return token.Value;
            }
            else
            {
                throw new Exception("Expected identifier or integer");
            }
        }

        private Token MatchArgument(TokenType[] type, string relType, bool firstArg)
        {
            // Służy do sprawdzania zgodności typu podanego argumentu w relacji
            Token token = PeekToken();
            if (token.Value == "_")
            {
                if ((relType == "Modifies" || relType == "Uses") && firstArg)   // tzn. jeśli pierwszy arg. "_"
                    // oraz typ relacji Modifies lub Uses
                {
                    throw new Exception($"First argument in relation of type {relType} cannot be {token.Value}");
                }

                Advance();
                return token;
            }
            else if (!type.Contains(token.Type))
            {
                throw new Exception($"Invalid argument type: {token.Type}");
            }
            else
            {
                Advance();
                return token;
            }
        }

        private Token PeekToken()
        {
            // Służy do sprawdzenia typu tokena w zapytaniu
            // Jeśli tablica jest pusta, osiągnięto
            // koniec zapytania:
            if (parts.Length == 0)
            {
                return new Token(TokenType.EOF, "");
            }

            // Wyszukiwanie typu tokena
            TokenType tokenType;
            if (int.TryParse(parts[0], out _))
            {
                tokenType = TokenType.INTEGER;
            }
            // IDENT może być w cudzysłowiu lub bez
            else if (Regex.IsMatch(parts[0], @"^\""[a-zA-Z][a-zA-Z0-9#]*\""$|^[a-zA-Z][a-zA-Z0-9#]*$"))
            {
                tokenType = TokenType.IDENT;
            }
            else
            {
                tokenType = TokenType.SYMBOL;
            }

            return new Token(tokenType, parts[0]);
        }

        private void Advance(int numTokens = 1)
        {
            // Służy do przesuwania pozycji analizatora
            // o określoną liczbę tokenów do przodu
            if (numTokens > 1)
            {
                parts = parts.Skip(numTokens).ToArray();    // usuń wykorzystane już tokeny z tablicy
                position += numTokens;
            }
            else
            {
                parts = parts.Skip(1).ToArray();    // usuń wykorzystany już token z tablicy
                position++;
            }
        }

        private string Peek(int numTokens = 1)
        {
            // Służy do sprawdzenia n-tego słowa w zapytaniu,
            // bez konieczności przesuwania pozycji analizatora
            if (parts.Length > 0 && numTokens <= parts.Length)   // jeśli możliwe jest sprawdzenie n-tego tokena
            {
                return parts[numTokens - 1];
            }
            else
            {
                return "";
            }
        }

        private void QueryValidator()
        {
            // Pobranie wszystkich zadeklarowanych synonimów
            var allDeclarationSynonyms = new HashSet<string>(_query.Declarations.SelectMany(d => d.Synonyms));

            // Słownik synonim-typ
            var synonymToEntity = _query.Declarations
            .SelectMany(declaration => declaration.Synonyms.Select(synonym => new { synonym, declaration.DesignEntity }))
            .ToDictionary(x => x.synonym, x => x.DesignEntity);

            // Pobranie ilości linii kodu w programie z Parsera?
            int codeLength = 2; // tymczasowe zanim się pobierze

            List<string> stmtTypes = new List<string> { "stmt", "assign", "call", "while", "if" };

            // Walidacja synonimów w Select
            foreach (string i in _query.Synonym)
            {
                if(!allDeclarationSynonyms.Contains(i))
                {
                    throw new Exception($"Uninitialized synonym: {i}");
                }
            }

            // Walidacja w Relacjach
            if (_query.SuchThatClause != null)
            {
                List<string> relElements = new List<string>
                {
                    _query.SuchThatClause.Relation.FirstElement(),
                    _query.SuchThatClause.Relation.SecondElement()
                };

                bool firstIter = true;
                foreach (string element in relElements)
                {
                    // Gdy synonimy (czyli IDENT bez "var")
                    if (Regex.IsMatch(element, @"^[a-zA-Z][a-zA-Z0-9#]*$"))
                    {
                        // Niezadeklarowane synonimy w relacji
                        if (!allDeclarationSynonyms.Contains(element))
                        {
                            throw new Exception($"Uninitialized synonym: {element}");
                        }
                        
                        switch (_query.SuchThatClause.Relation)
                        {
                            // Jeśli Follows, każdy z argumentów musi być stmtTypes
                            // (czyli stmt, assign, call, while, if)
                            case Follows:
                            case FollowsT:
                                foreach (var pair in synonymToEntity)
                                {
                                    if(pair.Key == element)
                                    {
                                        if(!stmtTypes.Contains(pair.Value))
                                        {
                                            throw new Exception($"Synonym of type {pair.Value} cannot be used in relation" +
                                                $" of type {_query.SuchThatClause.Relation.GetType().Name}");
                                        }
                                    }
                                }
                                break;

                            // Jeśli Parent, pierwszy argument może być: stmt, while, if
                            // a drugi stmtTypes
                            case Parent:
                            case ParentT:
                                foreach (var pair in synonymToEntity)
                                {
                                    if (pair.Key == element)
                                    {
                                        if (firstIter)
                                        {
                                            List<string> parentArg = new List<string> { "stmt", "while", "if" };
                                            if (!parentArg.Contains(pair.Value))
                                            {
                                                throw new Exception($"Synonym of type {pair.Value} cannot be used as a first" +
                                                    $" argument in relation of type {_query.SuchThatClause.Relation.GetType().Name}");
                                            }

                                            firstIter = false;
                                        }
                                        else
                                        {
                                            if (!stmtTypes.Contains(pair.Value))
                                            {
                                                throw new Exception($"Synonym of type {pair.Value} cannot be used as a second " +
                                                    $"argument in relation of type {_query.SuchThatClause.Relation.GetType().Name}");
                                            }
                                        }
                                    }
                                }
                                break;

                            // Jeśli Modifies/Uses, pierwszy może być: stmtTypes, procedure
                            // a drugi variable
                            case ModifiesS:
                            case UsesS:
                                foreach (var pair in synonymToEntity)
                                {
                                    if (pair.Key == element)
                                    {
                                        if (firstIter)
                                        {
                                            if (!stmtTypes.Contains(pair.Value) && pair.Value != "procedure")
                                            {
                                                throw new Exception($"Synonym of type {pair.Value} cannot be used as a first" +
                                                    $" argument in relation of type {_query.SuchThatClause.Relation.GetType().Name}");
                                            }

                                            firstIter = false;
                                        }
                                        else
                                        {
                                            if (pair.Value != "variable")
                                            {
                                                throw new Exception($"Synonym of type {pair.Value} cannot be used as a second " +
                                                    $"argument in relation of type {_query.SuchThatClause.Relation.GetType().Name}");
                                            }
                                        }
                                    }
                                }
                                break;

                            // Jeśli Calls, oba muszą być procedure
                            /*case Calls:
                                foreach (var pair in synonymToEntity)
                                {
                                    if (pair.Key == element)
                                    {
                                        if (pair.Value != "procedure")
                                        {
                                            throw new Exception($"Synonym of type {pair.Value} cannot be used in relation" +
                                                $" of type Calls");
                                        }
                                    }
                                }
                                break;*/
                        }        
                    }

                    // Jeśli któryś argument to linia programu (INTEGER token), sprawdź czy w zakresie
                    if(Regex.IsMatch(element, @"^\d+$"))
                    {
                        if (!(int.Parse(element) <= codeLength))
                        {
                            throw new Exception($"Line number: {int.Parse(element)} out of bounds for" +
                                $" program of length: {codeLength}");
                        }
                    }
                }
            }

            // Walidacja w With
            if(_query.WithClause != null)
            {
                // Niezadeklarowany synonim w With
                if(!allDeclarationSynonyms.Contains(_query.WithClause.Synonym))
                {
                    throw new Exception($"Uninitialized synonym: {_query.WithClause.Synonym}");
                }

                // Sprawdź, czy synonim danego typu może zostać wykorzystany w klauzuli With (entTable)
                // oraz czy synonim posiada dany atrybut (np. proc p -> p.varName - źle)
                foreach (var pair in synonymToEntity)
                {
                    if(pair.Key == _query.WithClause.Synonym)
                    {
                        if (!entTable.ContainsKey(pair.Value))
                        {
                            throw new Exception($"Synonym of type {pair.Value} cannot be used in a with clause.");
                        }
                        else  // typ synonimu jest w entTable, sprawdzamy teraz, czy posiada dany atrybut
                        {
                            entTable.TryGetValue(pair.Value, out var entity);
                            if(_query.WithClause.AttrName != entity)
                            {
                                throw new Exception($"Synonym of type {pair.Value} doesn't support attribute {_query.WithClause.AttrName}.");
                            }
                            break;
                        }
                    }
                }

                // Jeśli stmt#, to sprawdź czy linia programu w zakresie
                if(_query.WithClause.AttrName == "stmt#")
                {
                    if(!(int.Parse(_query.WithClause.Value) <= codeLength))
                    {
                        throw new Exception($"Line number: {int.Parse(_query.WithClause.Value)} out of bounds for" +
                            $" program of length: {codeLength}");
                    }
                }
            }
        }
    }
}
