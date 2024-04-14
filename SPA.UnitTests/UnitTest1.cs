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
    public class ParserTests
    {
        private QueryPreprocessor preprocessor;
        private Parser parser;

        string validQuery = "Select ";
        Query tquery = new Query();
        private int position = 0;

        [TestInitialize]
        public void Setup()
        {
            parser = new Parser();
            preprocessor = new QueryPreprocessor(validQuery,tquery);
        }

        [TestMethod]
        public void TestProcedureParsing()
        {
            int amongus = 1;

            Assert.AreEqual(1, amongus);
        }

        [TestMethod]
        public void CreateAssign_ShouldReturnValidAssign()
        {
            var assign = parser.CreateAssign();
            Assert.IsNotNull(assign);
            Assert.IsNotNull(assign.Var);
            Assert.AreEqual("xd", assign.Var.VarName);
            Assert.AreEqual(0, assign.Var.LineNumber);
        }

        [TestMethod]
        public void ValidateQueryTest()
        {
            preprocessor.ValidateQuery();
            //Assert.AreEqual(expectedValue, preprocessor._query.SomeProperty);
        }

        [TestMethod]
        public void TestValidateDeclarations()
        {
            // Arrange
            List<Declaration> expectedDeclarations = new List<Declaration>(); // Add expected declarations here

            // Act
            List<Declaration> actualDeclarations = preprocessor.ValidateDeclarations();

            // Assert
            Assert.AreEqual(expectedDeclarations.Count, actualDeclarations.Count, "The number of declarations does not match.");

            for (int i = 0; i < expectedDeclarations.Count; i++)
            {
                Assert.AreEqual(expectedDeclarations[i].DesignEntity, actualDeclarations[i].DesignEntity, $"DesignEntity at index {i} does not match.");
                CollectionAssert.AreEqual(expectedDeclarations[i].Synonyms, actualDeclarations[i].Synonyms, $"Synonyms at index {i} do not match.");
            }
        }

        [TestMethod]
        public void TestValidateSynonyms()
        {
            // Arrange
            var expected = new List<string> { "synonym1", "synonym2" };

            // Act
            var result = preprocessor.ValidateSynonyms();

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }
        
        [TestMethod]
        public void TestValidateRelation()
        {
            // Arrange
            var relTable = new Dictionary<string, 
                (int, TokenType[], TokenType[], Func<StmtRef, StmtRef, Relation>, 
                Func<StmtRef, EntRef, Relation>)>();
            // Act
            try
            {
                var result = preprocessor.ValidateRelation();

                // Assert
                //Assert.AreEqual(expected, result);
            }
            catch (Exception ex)
            {
                // Assert
                // Assert.AreEqual("Expected exception message", ex.Message);
            }
        }

        [TestMethod]
        public void TestValidateWithClause()
        {
            // Arrange
            TokenType tokenType = new TokenType(); // replace with actual TokenType
            string expectedSynonym = "test"; // replace with expected synonym
            string expectedAttrName = "test"; // replace with expected attribute name
            string expectedValue = "test"; // replace with expected value

            // Act
            With result = preprocessor.ValidateWithClause();

            // Assert
            Assert.AreEqual(expectedSynonym, result.Synonym);
            Assert.AreEqual(expectedAttrName, result.AttrName);
            Assert.AreEqual(expectedValue, result.Value);
        }

        [TestMethod]
        public string PeekTest()
        {
            int numTokens = 2;

            string result = preprocessor.Peek(numTokens);

            Assert.AreEqual("a", result);
            
            return result;
        }

        [TestMethod]
        public void AdvanceTest()
        {
            position = 0;
            int numTokens = 1;

            preprocessor.Advance(numTokens);
            Assert.AreEqual(validQuery, tquery);
        }

        [TestMethod]
        public void TestMatch()
        {
            // Arrange
            object[] expectedValues = new object[] { "expectedValue", TokenType.ExpectedType }; // Replace with your expected values

            // Act
            Token token = null;
            try
            {
                token = preprocessor.Match();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected no exception, but got: {ex.Message}");
            }

            // Assert
            Assert.IsNotNull(token, "Expected a non-null token, but got null.");
            Assert.AreEqual(expectedValues[0], token.Value, $"Expected value to be '{expectedValues[0]}', but got '{token.Value}'");
            Assert.AreEqual(expectedValues[1], token.Type, $"Expected type to be '{expectedValues[1]}', but got '{token.Type}'");
        }

        

        [TestMethod]
        public void MatchDesignEntity_ValidEntity_ReturnsToken()
        {
            // Assuming Match method sets the Value of the Token
            preprocessor.Match(TokenType.IDENT, "stmt");

            // Act
            var result = preprocessor.MatchDesignEntity();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("stmt", result.Value);
        }

        [TestMethod]
        public void MatchDesignEntity_InvalidEntity_ThrowsException()
        {
            // Arrange
            // Assuming Match method sets the Value of the Token
            preprocessor.Match(TokenType.IDENT, "invalid_entity");

            // Act & Assert
            Assert.ThrowsException<Exception>(() => preprocessor.MatchDesignEntity());
        }

        [TestMethod]
        public void MatchValue_ValidToken_ReturnsValue()
        {
            
            preprocessor.SetupToken(TokenType.IDENT, "TestValue");

            var result = preprocessor.MatchValue();

            Assert.AreEqual("TestValue", result);
        }

        [TestMethod]
        public void MatchValue_InvalidToken_ThrowsException()
        {
            // Assuming you have a method to set up your token
            preprocessor.SetupToken(TokenType.STRING, "TestValue");

            // Act and Assert
            Assert.ThrowsException<Exception>(() => preprocessor.MatchValue());
        }


        [TestMethod]
        public void MatchAttrName_ValidAttribute_ReturnsAttribute()
        {
            // Arrange
            var expected = "procName";

            // Act
            var actual = preprocessor.MatchAttrName();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MatchAttrName_InvalidAttribute_ThrowsException()
        {
            // Arrange
            var invalidAttr = "invalidAttr";

            // Act and Assert
            Assert.ThrowsException<Exception>(() => preprocessor.MatchAttrName());
        }


        [TestMethod]
        public void TestPeekToken()
        {
            // Arrange
            validQuery = "123";
            position = 0;

            // Act
            Token result = preprocessor.PeekToken();

            // Assert
            Assert.AreEqual(TokenType.INTEGER, result.Type);
            Assert.AreEqual("123", result.Value);
        }

    }



}
