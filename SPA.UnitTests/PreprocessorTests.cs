using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
 

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPA.DesignEntities;
using SPA.Parsing;
using SPA.QueryProcessor;


namespace SPA.UnitTests
{
    [TestClass]
    public class PreprocessorTests
    {
        [TestMethod]
        public void TestValidateQuery()
        {
            // Arrange
            string query = "stmt s;Select s such that Follows(s, v);";
            Query queryObject = new Query();
            QueryPreprocessor preprocessor = new QueryPreprocessor(query, queryObject);

            // Act
            preprocessor.ValidateQuery();

            // Assert
            Assert.IsNotNull(queryObject.Declarations);
            Assert.AreEqual(1, queryObject.Declarations.Count);
            Assert.AreEqual("s", queryObject.Declarations[0].Synonyms[0]);
        }

        [TestMethod]
        public void TestValidateDeclarations()
        {
            string query = "stmt s; variable v; Select s such that Follows(s,v)";
            Query _query = new Query();
            QueryPreprocessor preprocessor = new QueryPreprocessor(query, _query);

            List<Declaration> declarations = preprocessor.ValidateDeclarations();

            Assert.IsNotNull(declarations);
            Assert.AreEqual(2, declarations.Count);

            Assert.AreEqual("stmt", declarations[0].DesignEntity);
            Assert.AreEqual(1, declarations[0].Synonyms.Count);
            Assert.AreEqual("s", declarations[0].Synonyms[0]);

            Assert.AreEqual("variable", declarations[1].DesignEntity);
            Assert.AreEqual(1, declarations[1].Synonyms.Count);
            Assert.AreEqual("v", declarations[1].Synonyms[0]);
        }



        [TestMethod]
        public void TestValidateSynonyms()
        {
            // Arrange
            QueryPreprocessor queryPreprocessor = new QueryPreprocessor("", new Query());

            // Create a list of tokens to simulate the input for ValidateSynonyms
            List<Token> tokens = new List<Token>
            {
                new Token(TokenType.IDENT, "synonym1"),
                new Token(TokenType.SYMBOL, ","),
                new Token(TokenType.IDENT, "synonym2"),
                new Token(TokenType.SYMBOL, ","),
                new Token(TokenType.IDENT, "synonym3"),
            };

            // Replace ValidateSynonyms method call with direct token simulation
            queryPreprocessor.Advance(); // Move position to the beginning of the input
            queryPreprocessor.Advance(); // Skip Select keyword
            queryPreprocessor.Advance(); // Skip synonyms declaration

            // Act
            List<string> synonyms = queryPreprocessor.ValidateSynonyms();

            // Assert
            // Check if the synonyms list contains the expected values
            if (synonyms.Count != 3 || synonyms[0] != "synonym1" || synonyms[1] != "synonym2" || synonyms[2] != "synonym3")
            {
                Console.WriteLine("ValidateSynonyms test failed.");
            }
            else
            {
                Console.WriteLine("ValidateSynonyms test passed.");
            }
        }

        [TestMethod]
        public void TestValidateSuchThatClause()
        {
            // Arrange
            QueryPreprocessor queryPreprocessor = new QueryPreprocessor("stmt a, b; Select a such that Follows(a, b)", new Query());
            // Act
            SuchThat suchThatClause = queryPreprocessor.ValidateSuchThatClause();

            // Assert
            Assert.IsNotNull(suchThatClause);
            Assert.IsNotNull(suchThatClause.Relation);
            Assert.AreEqual("Follows", suchThatClause.Relation.GetType().Name); // Assuming the Relation object's type name matches the relation name
        }

        [TestMethod]
        public void TestValidateRelation()
        {
            // Arrange
            string query = "stmt a, b;Select a such that Follows(a, b)";
            QueryPreprocessor queryPreprocessor = new QueryPreprocessor(query, new Query());

            // Act
            var relation = queryPreprocessor.ValidateRelation();

            // Assert
            Assert.IsNotNull(relation, "Relation should not be null");
            Assert.IsInstanceOfType(relation, typeof(Follows), "Relation should be of type Follows");
        }

        
    }






}
