using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SPA.QueryProcessor
{
    internal class QueryPreprocessor
    {
        // Klasa sprawdza poprawność zapytania i tworzy jego drzewo

        // Table-driven approach dla tablicy relacji
        private Dictionary<string, (int, TokenType[], TokenType[],
            Func<StmtRef, StmtRef, Relation> createStmtStmtInstance,
            Func<StmtRef, EntRef, Relation> createStmtEntInstance)> relTable;

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

            this.query = query;
            this._query = _query;

            ValidateQuery();

        }

        private void ValidateQuery()
        {
            // Zapytanie od razu dzielimy na tokeny, aby się pozbyć problemu z białymi znakami
            string pattern = @"(\s+|;|\.|\(|\)|,)"; // separatory
            parts = Regex.Split(query, pattern);
            parts = parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray(); // usuń puste elementy

            // Tutaj powinno być peek() na deklaracje bo są opcjonalne!!
            // Wsm bez różnicy, to strata pamięci wielkości małej listy
            _query.Declarations = ValidateDeclarations();
            Match("Select");
            _query.Synonym = Match(TokenType.IDENT).Value;

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
                // Pobranie informacji o danej relacji
                var (numArgs, argTypes, argTypes2, createStmtStmtInstance, createStmtEntInstance) = relTable[relToken.Value];

                Match(relToken.Value);
                Match(TokenType.SYMBOL, "(");

                // Sprawdź, czy pierwszy argument zgadza się z definicją
                Token[] argsTokens = new Token[numArgs];
                argsTokens[0] = MatchArgument(argTypes);

                Match(TokenType.SYMBOL, ",");

                // Sprawdź, czy drugi argument zgadza się z definicją
                argsTokens[1] = MatchArgument(argTypes2);

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
            clause.Value = MatchValue();
            return clause;
        }

        private Token MatchDesignEntity()
        {
            string[] validDesignEntities = { "stmt", "assign", "while", "variable", "constant", "prog_line" };
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

        private string MatchValue()
        {
            // Służy do sprawdzenia ref w klauzuli with
            Token token = PeekToken();
            if (token.Type == TokenType.IDENT || token.Type == TokenType.INTEGER)
            {
                Advance();
                return token.Value;
            }
            else
            {
                throw new Exception("Expected identifier or integer");
            }
        }

        private Token MatchArgument(TokenType[] type)
        {
            // Służy do sprawdzania zgodności typu podanego argumentu w relacji
            Token token = PeekToken();
            if (token.Value == "_")
            {
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

    }
}
